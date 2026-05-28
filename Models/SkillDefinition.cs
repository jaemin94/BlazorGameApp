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

    public int BaseDamage { get; set; } = 0;
    public int DamagePerLevel { get; set; } = 0;

    public int BaseRange { get; set; } = 80;
    public int RangePerLevel { get; set; } = 0;

    public int BaseHitCount { get; set; } = 1;
    public int HitCountBonusEveryLevel { get; set; } = 0;

    public int BaseTargetCount { get; set; } = 1;
    public int TargetCountBonusEveryLevel { get; set; } = 0;

    public int CooldownSeconds { get; set; } = 1;

    public int GetDamage(int level) => BaseDamage + DamagePerLevel * Math.Max(0, level - 1);
    public int GetRange(int level) => BaseRange + RangePerLevel * Math.Max(0, level - 1);

    public int GetHitCount(int level)
    {
        if (HitCountBonusEveryLevel <= 0)
            return BaseHitCount;

        return BaseHitCount + (level - 1) / HitCountBonusEveryLevel;
    }

    public int GetTargetCount(int level)
    {
        if (TargetCountBonusEveryLevel <= 0)
            return BaseTargetCount;

        return BaseTargetCount + (level - 1) / TargetCountBonusEveryLevel;
    }
}
