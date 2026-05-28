namespace MyBlazorApp.Models;

// 퀵슬롯 스킬 정보
public class SkillSlot
{
    public int Slot { get; set; }
    public string SkillName { get; set; } = "";
    public int CooldownSeconds { get; set; } = 1;
}
