namespace MyBlazorApp.Models;

// 마을 건물
public class BuildingState
{
    public string Name { get; set; } = "";
    public BuildingType Type { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; } = 120;
    public int Height { get; set; } = 90;
    public string Icon { get; set; } = "🏠";
    public string Message { get; set; } = "";
}
