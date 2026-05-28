namespace MyBlazorApp.Models;

// 스킬 목록 DB 역할
public static class SkillDatabase
{
    public static readonly List<SkillDefinition> Skills = new()
    {
        // 전직 전 기본 스킬. 처음부터 배워져 있음.
        new SkillDefinition
        {
            SkillId = "basic_strike",
            Name = "강타",
            Description = "가까운 적 1명을 강하게 공격합니다.",
            RequiredJob = JobType.Beginner,
            RequiredLevel = 1,
            MaxLevel = 1,
            BaseDamage = 8,
            DamagePerLevel = 0,
            BaseRange = 90,
            BaseHitCount = 1,
            BaseTargetCount = 1,
            CooldownSeconds = 1
        },

        // 마법사
        new SkillDefinition
        {
            SkillId = "mage_fireball",
            Name = "파이어볼",
            Description = "불덩이를 날려 범위 내 적을 공격합니다. 레벨이 오르면 데미지/타깃수/사거리가 증가합니다.",
            RequiredJob = JobType.Mage,
            RequiredLevel = 8,
            MaxLevel = 10,
            BaseDamage = 20,
            DamagePerLevel = 7,
            BaseRange = 180,
            RangePerLevel = 8,
            BaseHitCount = 1,
            HitCountBonusEveryLevel = 5,
            BaseTargetCount = 2,
            TargetCountBonusEveryLevel = 3,
            CooldownSeconds = 2
        },
        new SkillDefinition
        {
            SkillId = "mage_heal",
            Name = "힐",
            Description = "자신의 HP를 회복합니다. 레벨이 오르면 회복량이 증가합니다.",
            RequiredJob = JobType.Mage,
            RequiredLevel = 8,
            MaxLevel = 10,
            BaseDamage = 0,
            DamagePerLevel = 0,
            BaseRange = 0,
            BaseHitCount = 1,
            BaseTargetCount = 1,
            CooldownSeconds = 5
        },

        // 전사
        new SkillDefinition
        {
            SkillId = "warrior_spin",
            Name = "회전베기",
            Description = "주변 적을 베어냅니다. 레벨이 오르면 데미지/타깃수/타격수가 증가합니다.",
            RequiredJob = JobType.Warrior,
            RequiredLevel = 10,
            MaxLevel = 10,
            BaseDamage = 18,
            DamagePerLevel = 6,
            BaseRange = 120,
            RangePerLevel = 4,
            BaseHitCount = 1,
            HitCountBonusEveryLevel = 4,
            BaseTargetCount = 3,
            TargetCountBonusEveryLevel = 2,
            CooldownSeconds = 3
        },

        // 도적
        new SkillDefinition
        {
            SkillId = "thief_double_slash",
            Name = "더블 슬래시",
            Description = "가까운 적을 빠르게 여러 번 공격합니다. 레벨이 오르면 타격수와 데미지가 증가합니다.",
            RequiredJob = JobType.Thief,
            RequiredLevel = 10,
            MaxLevel = 10,
            BaseDamage = 13,
            DamagePerLevel = 5,
            BaseRange = 90,
            RangePerLevel = 3,
            BaseHitCount = 2,
            HitCountBonusEveryLevel = 3,
            BaseTargetCount = 1,
            TargetCountBonusEveryLevel = 0,
            CooldownSeconds = 2
        },

        // 궁수
        new SkillDefinition
        {
            SkillId = "archer_power_shot",
            Name = "파워샷",
            Description = "먼 거리의 적을 강하게 공격합니다. 레벨이 오르면 사거리와 데미지가 증가합니다.",
            RequiredJob = JobType.Archer,
            RequiredLevel = 10,
            MaxLevel = 10,
            BaseDamage = 24,
            DamagePerLevel = 8,
            BaseRange = 260,
            RangePerLevel = 12,
            BaseHitCount = 1,
            HitCountBonusEveryLevel = 5,
            BaseTargetCount = 1,
            TargetCountBonusEveryLevel = 4,
            CooldownSeconds = 3
        }
    };

    public static SkillDefinition? GetSkill(string skillId)
    {
        return Skills.FirstOrDefault(s => s.SkillId == skillId);
    }

    public static List<SkillDefinition> GetSkillsForJob(JobType job)
    {
        return Skills.Where(s => s.RequiredJob == job).ToList();
    }
}
