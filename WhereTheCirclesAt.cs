using System.Linq;
using System.Text.RegularExpressions;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ImGuiNET;
using SharpDX;
using static WhereTheCirclesAt.WhereTheCirclesAtSettings;
using Vector2 = SharpDX.Vector2;
using Vector2N = System.Numerics.Vector2;
using Vector3N = System.Numerics.Vector3;
using Vector4N = System.Numerics.Vector4;

namespace WhereTheCirclesAt;

public class WhereTheCirclesAt : BaseSettingsPlugin<WhereTheCirclesAtSettings>
{
    private ImDrawListPtr _backGroundWindowPtr;
    private Entity _mercenaryEntity = new();
    private RectangleF _rect;

    public WhereTheCirclesAt()
    {
        Name = "Where The Circles At";
    }

    public Vector3N PlayerPos { get; set; }
    public IngameData InGameData { get; set; }

    public override bool Initialise()
    {
        return true;
    }

    public override void DrawSettings()
    {
        base.DrawSettings();
        var doryMercSetting = Settings.DoryaniMerc;
        if (doryMercSetting.DrawDoryanisOnMerc)
        {
            ImGui.PushID("doryCircle_Settings");
            if (ImGui.CollapsingHeader("", ImGuiTreeNodeFlags.Framed | ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Indent();
                ImGui.InputText("Name", ref doryMercSetting.Circle.Name, 200);
                ImGui.DragFloat("Closely Resembles in game distance", ref doryMercSetting.Circle.Size);
                ImGui.DragInt("Width", ref doryMercSetting.Circle.Thickness);
                ImGui.DragInt("Segments (high = bad)", ref doryMercSetting.Circle.Segments);
                ImGui.Checkbox("Draw in World", ref doryMercSetting.Circle.World);
                ImGui.Checkbox("World Follow Terrain", ref doryMercSetting.Circle.WorldFollowTerrain);
                ImGui.Checkbox("Draw on Large Map", ref doryMercSetting.Circle.LargeMap);
                ImGui.Checkbox("Large Map Follow Terrain", ref doryMercSetting.Circle.LargeMapFollowTerrain);
                ImGui.Checkbox("Large Map World Color", ref doryMercSetting.Circle.EnableLargeMapColor);
                doryMercSetting.Circle.LargeMapColor = ColorPicker("Active Large Map World Color", doryMercSetting.Circle.LargeMapColor);
                doryMercSetting.Circle.GameWorldColor = ColorPicker("Active World Color", doryMercSetting.Circle.GameWorldColor);

                ImGui.Unindent();
                ImGui.Separator();
            }

            ImGui.PopID();
        }

        if (Settings.Circles.Count > 0)
            foreach (var (item, i) in Settings.Circles.Select((x, i) => (x, i)).ToList())
            {
                ImGui.PushID($"{i}Settings");
                if (ImGui.CollapsingHeader("", ImGuiTreeNodeFlags.Framed | ImGuiTreeNodeFlags.DefaultOpen))
                {
                    ImGui.Indent();
                    ImGui.InputText("Name", ref item.Name, 200);
                    ImGui.DragFloat("Closely Resembles in game distance", ref item.Size);
                    ImGui.DragInt("Width", ref item.Thickness);
                    ImGui.DragInt("Segments (high = bad)", ref item.Segments);
                    ImGui.Checkbox("Draw in World", ref item.World);
                    ImGui.Checkbox("World Follow Terrain", ref item.WorldFollowTerrain);
                    ImGui.Checkbox("Draw on Large Map", ref item.LargeMap);
                    ImGui.Checkbox("Large Map Follow Terrain", ref item.LargeMapFollowTerrain);
                    ImGui.Checkbox("Large Map World Color", ref item.EnableLargeMapColor);
                    item.LargeMapColor = ColorPicker("Active Large Map World Color", item.LargeMapColor);
                    item.GameWorldColor = ColorPicker("Active World Color", item.GameWorldColor);

                    if (ImGui.Button("Delete"))
                        Settings.Circles.RemoveAt(i);

                    ImGui.Unindent();
                    ImGui.Separator();
                }

                ImGui.PopID();
            }

        if (ImGui.Button("New"))
            Settings.Circles.Add(new CircleData());
    }

    public static Color ColorPicker(string labelName, Color inputColor)
    {
        var color = inputColor.ToVector4();
        var colorToVect4 = new Vector4N(color.X, color.Y, color.Z, color.W);

        return ImGui.ColorEdit4(labelName, ref colorToVect4, ImGuiColorEditFlags.AlphaBar)
            ? new Color(colorToVect4.X, colorToVect4.Y, colorToVect4.Z, colorToVect4.W)
            : inputColor;
    }

    public override Job Tick()
    {
        _mercenaryEntity = null;
        InGameData = GameController.IngameState.Data;
        var player = GameController?.Player;

        if (player == null)
            return null;

        if (InGameData != null)
            PlayerPos = InGameData.ToWorldWithTerrainHeight(player.PosNum);

        if (Settings.DoryaniMerc.DrawDoryanisOnMerc)
        {
            if (!GameController.Player.TryGetComponent<Actor>(out var actorComp))
                return null;

            foreach (var deployedObject in actorComp.DeployedObjects)
            {
                if (deployedObject?.Entity is not { IsValid: true, IsAlive: true } entity)
                    continue;

                if (Regex.IsMatch(entity.Metadata, "^Metadata/Monsters/Mercenaries/") && entity.Buffs.Any(x => x.Name == "lightning_resistance_override"))
                    _mercenaryEntity = entity;
            }
        }

        return null;
    }

    public override void Render()
    {
        if (!Settings.Enable.Value || !GameController.InGame || InGameData == null || PlayerPos == Vector3N.Zero)
            return;

        var inGameUi = GameController.Game.IngameState.IngameUi;

        if (Settings.DisableDrawOnLeftOrRightPanelsOpen && (inGameUi.OpenLeftPanel.IsVisible || inGameUi.OpenRightPanel.IsVisible))
            return;

        if (!Settings.IgnoreFullscreenPanels && inGameUi.FullscreenPanels.Any(x => x.IsVisible))
            return;

        if (!Settings.IgnoreLargePanels && inGameUi.LargePanels.Any(x => x.IsVisible))
            return;

        _rect = GameController.Window.GetWindowRectangle() with
        {
            Location = Vector2.Zero
        };

        if (!Settings.DisableDrawRegionLimiting)
        {
            if (inGameUi.OpenRightPanel.IsVisible)
                _rect.Right = inGameUi.OpenRightPanel.GetClientRectCache.Left;

            if (inGameUi.OpenLeftPanel.IsVisible)
                _rect.Left = inGameUi.OpenLeftPanel.GetClientRectCache.Right;
        }

        ImGui.SetNextWindowSize(new Vector2N(_rect.Width, _rect.Height));
        ImGui.SetNextWindowPos(new Vector2N(_rect.Left, _rect.Top));

        ImGui.Begin(
            "wherethecirclesat_drawregion",
            ImGuiWindowFlags.NoDecoration |
            ImGuiWindowFlags.NoInputs |
            ImGuiWindowFlags.NoMove |
            ImGuiWindowFlags.NoScrollWithMouse |
            ImGuiWindowFlags.NoSavedSettings |
            ImGuiWindowFlags.NoFocusOnAppearing |
            ImGuiWindowFlags.NoBringToFrontOnFocus |
            ImGuiWindowFlags.NoBackground);

        _backGroundWindowPtr = ImGui.GetWindowDrawList();

        if (_mercenaryEntity != null)
        {
            var doryaniMercCircle = Settings.DoryaniMerc.Circle;
            if (doryaniMercCircle.World)
            {
                var colorToUse = inGameUi.Map.LargeMap.IsVisible && doryaniMercCircle.EnableLargeMapColor
                    ? doryaniMercCircle.LargeMapColor
                    : doryaniMercCircle.GameWorldColor;

                Graphics.DrawCircleInWorld(
                    InGameData.ToWorldWithTerrainHeight(_mercenaryEntity.PosNum), doryaniMercCircle.Size * 10, colorToUse, doryaniMercCircle.Thickness,
                    doryaniMercCircle.Segments, doryaniMercCircle.WorldFollowTerrain);
            }

            if (doryaniMercCircle.LargeMap && inGameUi.Map.LargeMap.IsVisible)
                Graphics.DrawCircleOnLargeMap(
                    _mercenaryEntity.GridPosNum, doryaniMercCircle.LargeMapFollowTerrain, doryaniMercCircle.Size, doryaniMercCircle.GameWorldColor,
                    doryaniMercCircle.Thickness, doryaniMercCircle.Segments);
        }

        foreach (var drawing in Settings.Circles)
        {
            if (drawing.World)
            {
                var colorToUse = inGameUi.Map.LargeMap.IsVisible && drawing.EnableLargeMapColor ? drawing.LargeMapColor : drawing.GameWorldColor;

                Graphics.DrawCircleInWorld(PlayerPos, drawing.Size * 10, colorToUse, drawing.Thickness, drawing.Segments, drawing.WorldFollowTerrain);
            }

            if (drawing.LargeMap && inGameUi.Map.LargeMap.IsVisible)
                Graphics.DrawCircleOnLargeMap(
                    GameController.Player.GridPosNum, drawing.LargeMapFollowTerrain, drawing.Size, drawing.GameWorldColor, drawing.Thickness, drawing.Segments);
        }

        ImGui.End();
    }
}