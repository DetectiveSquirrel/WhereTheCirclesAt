using System.Collections.Generic;
using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using SharpDX;

namespace WhereTheCirclesAt;

public class WhereTheCirclesAtSettings : ISettings
{
    public ToggleNode Enable { get; set; } = new ToggleNode(false);
    public ToggleNode DisableDrawRegionLimiting { get; set; } = new ToggleNode(false);
    public ToggleNode DisableDrawOnLeftOrRightPanelsOpen { get; set; } = new ToggleNode(false);
    public ToggleNode IgnoreFullscreenPanels { get; set; } = new ToggleNode(false);
    public ToggleNode IgnoreLargePanels { get; set; } = new ToggleNode(false);
    public DoryaniMercSettings DoryaniMerc { get; set; } = new DoryaniMercSettings();

    public List<CircleData> Circles = [];

    public class CircleData
    {
        public string Name = "Rename Me";
        public float Size = 100;
        public int Thickness = 7;
        public int Segments = 40;
        public bool LargeMap = false;
        public bool LargeMapFilledCircle = false;
        public bool LargeMapFollowTerrain = false;
        public bool EnableLargeMapColor = false;
        public Color LargeMapColor = Color.White;
        public bool World = true;
        public bool WorldFilledCircle = false;
        public bool WorldFollowTerrain = false;
        public Color GameWorldColor = Color.White;
    }
}

[Submenu(CollapsedByDefault = false)]
public class DoryaniMercSettings
{
    public ToggleNode DrawDoryanisOnMerc { get; set; } = new(false);

    public WhereTheCirclesAtSettings.CircleData Circle { get; set; } = new()
        { Name = "Doryani Merc", Size = 75, Thickness = 5, LargeMapColor = Color.Yellow, GameWorldColor = Color.Yellow };
}