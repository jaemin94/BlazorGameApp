namespace MyBlazorApp.Models;

// 포탈 상태
public class PortalState
{
    public string Name { get; set; } = "";
    public MapType FromMap { get; set; }
    public MapType ToMap { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int TargetX { get; set; }
    public int TargetY { get; set; }
    public string Message { get; set; } = "";
}
