namespace MyBlazorApp.Models;

// 플레이어가 받은 퀘스트 진행 상태
public class PlayerQuestState
{
    public string QuestId { get; set; } = "";
    public int CurrentCount { get; set; } = 0;
    public bool Completed { get; set; } = false;
    public bool RewardReceived { get; set; } = false;
}
