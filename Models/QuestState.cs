namespace MyBlazorApp.Models;

// 퀘스트 원본 정의
public class QuestState
{
    public string QuestId { get; set; } = "";
    public string QuestName { get; set; } = "";
    public string Description { get; set; } = "";

    public MonsterType TargetMonsterType { get; set; }
    public int RequiredCount { get; set; }

    public int RewardGold { get; set; }
    public int RewardExp { get; set; }

    public string RewardItemName { get; set; } = "";
    public string RewardItemType { get; set; } = "Material";
    public int RewardAttackBonus { get; set; }
    public int RewardDefenseBonus { get; set; }
    public string RewardRarity { get; set; } = "Normal";
}
