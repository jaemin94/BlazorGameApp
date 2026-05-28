namespace MyBlazorApp.Models;

// 플레이어가 배운 스킬 상태
public class PlayerSkillState
{
    public string SkillId { get; set; } = "";
    public int Level { get; set; } = 1;
    public int MaxLevel { get; set; } = 10;
}
