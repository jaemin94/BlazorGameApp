namespace MyBlazorApp.Models;

// 스킬 원본 데이터
public class SkillDefinition
{
    public string SkillId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public JobType RequiredJob { get; set; } = JobType.Beginner;
    public int RequiredLevel { get; set; } = 1;
    public int MaxLevel { get; set; } = 10;
    public int CooldownSeconds { get; set; } = 1;

    public int BaseDamage { get; set; } = 0;
    public int DamagePerLevel { get; set; } = 0;
    public int BaseHitCount { get; set; } = 1;
    public int HitCountPerLevels { get; set; } = 0;
    public int BaseTargetCount { get; set; } = 1;
    public int TargetCountPerLevels { get; set; } = 0;
    public int BaseRange { get; set; } = 80;
    public int RangePerLevel { get; set; } = 0;

    public int GetDamage(int level) => BaseDamage + DamagePerLevel * Math.Max(0, level - 1);
    public int GetHitCount(int level) => BaseHitCount + (HitCountPerLevels <= 0 ? 0 : (level - 1) / HitCountPerLevels);
    public int GetTargetCount(int level) => BaseTargetCount + (TargetCountPerLevels <= 0 ? 0 : (level - 1) / TargetCountPerLevels);
    public int GetRange(int level) => BaseRange + RangePerLevel * Math.Max(0, level - 1);
}
