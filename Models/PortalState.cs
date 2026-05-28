namespace MyBlazorApp.Models;

// 맵 이동 포탈 정보
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
