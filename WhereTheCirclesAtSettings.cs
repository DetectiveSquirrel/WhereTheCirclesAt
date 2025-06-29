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
        public string Name = "Doryani Merc";
        public float Size = 75;
        public int Thickness = 5;
        public int Segments = 40;
        public bool LargeMap = false;
        public bool EnableLargeMapColor = false;
        public Color LargeMapColor = Color.Yellow;
        public bool World = true;
        public Color GameWorldColor = Color.Yellow;
    }
}

[Submenu(CollapsedByDefault = false)]
public class DoryaniMercSettings
{
    public ToggleNode DrawDoryanisOnMerc { get; set; } = new(false);
    public WhereTheCirclesAtSettings.CircleData Circle { get; set; } = new WhereTheCirclesAtSettings.CircleData();
}