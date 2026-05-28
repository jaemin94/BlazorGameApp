namespace MyBlazorApp.Models;

// NPC 상태
public class NpcState
{
    public string Name { get; set; } = "";
    public int X { get; set; }
    public int Y { get; set; }
    public string Message { get; set; } = "";
    public string Role { get; set; } = "";
    public string QuestId { get; set; } = "";
    public JobType JobToChange { get; set; } = JobType.Beginner;
}
