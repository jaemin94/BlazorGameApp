namespace MyBlazorApp.Models;

// 스킬 데이터베이스
public static class SkillDatabase
{
    public static readonly List<SkillDefinition> Skills = new()
    {
        new SkillDefinition
        {
            SkillId = "basic_strike",
            Name = "강타",
            Description = "초보자 기본 공격 스킬입니다.",
            RequiredJob = JobType.Beginner,
            RequiredLevel = 1,
            MaxLevel = 1,
            CooldownSeconds = 1,
            BaseDamage = 5,
            BaseRange = 90
        },
        new SkillDefinition
        {
            SkillId = "mage_fireball",
            Name = "파이어볼",
            Description = "불덩이를 날려 주변 적에게 피해를 줍니다.",
            RequiredJob = JobType.Mage,
            RequiredLevel = 8,
            CooldownSeconds = 2,
            BaseDamage = 20,
            DamagePerLevel = 7,
            BaseHitCount = 1,
            HitCountPerLevels = 4,
            BaseTargetCount = 2,
            TargetCountPerLevels = 3,
            BaseRange = 180,
            RangePerLevel = 5
        },
        new SkillDefinition
        {
            SkillId = "mage_heal",
            Name = "힐",
            Description = "자신의 HP를 회복합니다.",
            RequiredJob = JobType.Mage,
            RequiredLevel = 8,
            CooldownSeconds = 5,
            BaseDamage = 0
        },
        new SkillDefinition
        {
            SkillId = "warrior_spin",
            Name = "회전베기",
            Description = "주변 적을 베어 광역 피해를 줍니다.",
            RequiredJob = JobType.Warrior,
            RequiredLevel = 10,
            CooldownSeconds = 3,
            BaseDamage = 18,
            DamagePerLevel = 6,
            BaseTargetCount = 2,
            TargetCountPerLevels = 3,
            BaseHitCount = 1,
            HitCountPerLevels = 5,
            BaseRange = 130,
            RangePerLevel = 3
        },
        new SkillDefinition
        {
            SkillId = "thief_double_slash",
            Name = "더블 슬래시",
            Description = "한 대상에게 빠르게 여러 번 공격합니다.",
            RequiredJob = JobType.Thief,
            RequiredLevel = 10,
            CooldownSeconds = 2,
            BaseDamage = 12,
            DamagePerLevel = 5,
            BaseHitCount = 2,
            HitCountPerLevels = 3,
            BaseTargetCount = 1,
            BaseRange = 90
        },
        new SkillDefinition
        {
            SkillId = "archer_power_shot",
            Name = "파워샷",
            Description = "먼 거리의 적을 강하게 공격합니다.",
            RequiredJob = JobType.Archer,
            RequiredLevel = 10,
            CooldownSeconds = 3,
            BaseDamage = 24,
            DamagePerLevel = 8,
            BaseHitCount = 1,
            HitCountPerLevels = 5,
            BaseTargetCount = 1,
            TargetCountPerLevels = 4,
            BaseRange = 260,
            RangePerLevel = 8
        }
    };

    public static SkillDefinition? GetSkill(string skillId)
    {
        return Skills.FirstOrDefault(s => s.SkillId == skillId);
    }

    public static string GetName(string skillId)
    {
        return GetSkill(skillId)?.Name ?? skillId;
    }
}
