namespace MyBlazorApp.Models;

// NPC 위치 / 대사 / 연결 퀘스트
public class NpcState
{
    public string Name { get; set; } = "상인";
    public int X { get; set; } = 500;
    public int Y { get; set; } = 300;
    public string Message { get; set; } = "안녕 모험가!";
    public string QuestId { get; set; } = "";
}
