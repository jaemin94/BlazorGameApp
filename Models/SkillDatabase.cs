namespace MyBlazorApp.Models;

// 직업별 스킬 데이터베이스
// Beginner: 기본 스킬 1개
// Mage/Warrior/Thief/Archer: 각 직업별 5개씩
public static class SkillDatabase
{
    public static readonly List<SkillDefinition> Skills = new()
    {
        // ===============================
        // 초보자 기본 스킬
        // ===============================
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

        // ===============================
        // 마법사 스킬 5개
        // ===============================
        new SkillDefinition
        {
            SkillId = "mage_fireball",
            Name = "파이어볼",
            Description = "불덩이로 주변 적에게 피해를 줍니다.",
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
            Description = "자신의 HP를 회복합니다. 스킬 레벨이 오르면 회복량이 증가합니다.",
            RequiredJob = JobType.Mage,
            RequiredLevel = 8,
            CooldownSeconds = 5,
            BaseDamage = 0
        },
        new SkillDefinition
        {
            SkillId = "mage_ice_lance",
            Name = "아이스 랜스",
            Description = "차가운 창으로 먼 거리의 적을 공격합니다.",
            RequiredJob = JobType.Mage,
            RequiredLevel = 9,
            CooldownSeconds = 3,
            BaseDamage = 24,
            DamagePerLevel = 8,
            BaseHitCount = 1,
            HitCountPerLevels = 5,
            BaseTargetCount = 1,
            TargetCountPerLevels = 4,
            BaseRange = 230,
            RangePerLevel = 7
        },
        new SkillDefinition
        {
            SkillId = "mage_lightning_chain",
            Name = "체인 라이트닝",
            Description = "번개가 여러 적에게 연쇄 피해를 줍니다.",
            RequiredJob = JobType.Mage,
            RequiredLevel = 12,
            CooldownSeconds = 5,
            BaseDamage = 18,
            DamagePerLevel = 6,
            BaseHitCount = 1,
            HitCountPerLevels = 4,
            BaseTargetCount = 3,
            TargetCountPerLevels = 2,
            BaseRange = 220,
            RangePerLevel = 5
        },
        new SkillDefinition
        {
            SkillId = "mage_meteor",
            Name = "메테오",
            Description = "넓은 범위에 강력한 마법 피해를 줍니다.",
            RequiredJob = JobType.Mage,
            RequiredLevel = 15,
            CooldownSeconds = 8,
            BaseDamage = 45,
            DamagePerLevel = 12,
            BaseHitCount = 1,
            HitCountPerLevels = 3,
            BaseTargetCount = 4,
            TargetCountPerLevels = 2,
            BaseRange = 260,
            RangePerLevel = 6
        },

        // ===============================
        // 전사 스킬 5개
        // ===============================
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
            SkillId = "warrior_power_slash",
            Name = "파워 슬래시",
            Description = "한 대상에게 강력한 일격을 가합니다.",
            RequiredJob = JobType.Warrior,
            RequiredLevel = 10,
            CooldownSeconds = 2,
            BaseDamage = 30,
            DamagePerLevel = 9,
            BaseHitCount = 1,
            HitCountPerLevels = 4,
            BaseTargetCount = 1,
            BaseRange = 100,
            RangePerLevel = 2
        },
        new SkillDefinition
        {
            SkillId = "warrior_dash_strike",
            Name = "돌진베기",
            Description = "넓은 사거리 안의 적에게 빠르게 접근하듯 피해를 줍니다.",
            RequiredJob = JobType.Warrior,
            RequiredLevel = 12,
            CooldownSeconds = 4,
            BaseDamage = 26,
            DamagePerLevel = 8,
            BaseHitCount = 1,
            HitCountPerLevels = 4,
            BaseTargetCount = 2,
            TargetCountPerLevels = 4,
            BaseRange = 190,
            RangePerLevel = 6
        },
        new SkillDefinition
        {
            SkillId = "warrior_ground_break",
            Name = "대지 가르기",
            Description = "전방 넓은 범위에 피해를 줍니다.",
            RequiredJob = JobType.Warrior,
            RequiredLevel = 15,
            CooldownSeconds = 6,
            BaseDamage = 36,
            DamagePerLevel = 10,
            BaseHitCount = 1,
            HitCountPerLevels = 3,
            BaseTargetCount = 3,
            TargetCountPerLevels = 2,
            BaseRange = 170,
            RangePerLevel = 4
        },
        new SkillDefinition
        {
            SkillId = "warrior_berserk",
            Name = "광전사의 일격",
            Description = "강력한 다단히트 근접 공격입니다.",
            RequiredJob = JobType.Warrior,
            RequiredLevel = 18,
            CooldownSeconds = 7,
            BaseDamage = 28,
            DamagePerLevel = 11,
            BaseHitCount = 2,
            HitCountPerLevels = 3,
            BaseTargetCount = 2,
            TargetCountPerLevels = 3,
            BaseRange = 140,
            RangePerLevel = 3
        },

        // ===============================
        // 도적 스킬 5개
        // ===============================
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
            BaseRange = 95,
            RangePerLevel = 2
        },
        new SkillDefinition
        {
            SkillId = "thief_poison_dagger",
            Name = "맹독 단검",
            Description = "독을 바른 단검으로 강한 단일 피해를 줍니다.",
            RequiredJob = JobType.Thief,
            RequiredLevel = 10,
            CooldownSeconds = 3,
            BaseDamage = 20,
            DamagePerLevel = 7,
            BaseHitCount = 2,
            HitCountPerLevels = 4,
            BaseTargetCount = 1,
            BaseRange = 100,
            RangePerLevel = 2
        },
        new SkillDefinition
        {
            SkillId = "thief_shadow_step",
            Name = "섀도우 스텝",
            Description = "넓은 사거리 안의 적을 기습합니다.",
            RequiredJob = JobType.Thief,
            RequiredLevel = 12,
            CooldownSeconds = 4,
            BaseDamage = 25,
            DamagePerLevel = 8,
            BaseHitCount = 1,
            HitCountPerLevels = 3,
            BaseTargetCount = 2,
            TargetCountPerLevels = 4,
            BaseRange = 210,
            RangePerLevel = 7
        },
        new SkillDefinition
        {
            SkillId = "thief_fan_knives",
            Name = "표창 난무",
            Description = "여러 대상에게 표창을 던집니다.",
            RequiredJob = JobType.Thief,
            RequiredLevel = 15,
            CooldownSeconds = 5,
            BaseDamage = 18,
            DamagePerLevel = 6,
            BaseHitCount = 2,
            HitCountPerLevels = 4,
            BaseTargetCount = 3,
            TargetCountPerLevels = 2,
            BaseRange = 190,
            RangePerLevel = 6
        },
        new SkillDefinition
        {
            SkillId = "thief_assassinate",
            Name = "암살",
            Description = "가장 가까운 적에게 매우 강한 피해를 줍니다.",
            RequiredJob = JobType.Thief,
            RequiredLevel = 18,
            CooldownSeconds = 7,
            BaseDamage = 55,
            DamagePerLevel = 14,
            BaseHitCount = 1,
            HitCountPerLevels = 3,
            BaseTargetCount = 1,
            BaseRange = 120,
            RangePerLevel = 4
        },

        // ===============================
        // 궁수 스킬 5개
        // ===============================
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
        },
        new SkillDefinition
        {
            SkillId = "archer_multi_shot",
            Name = "멀티샷",
            Description = "여러 적에게 화살을 발사합니다.",
            RequiredJob = JobType.Archer,
            RequiredLevel = 10,
            CooldownSeconds = 4,
            BaseDamage = 16,
            DamagePerLevel = 6,
            BaseHitCount = 1,
            HitCountPerLevels = 4,
            BaseTargetCount = 3,
            TargetCountPerLevels = 2,
            BaseRange = 240,
            RangePerLevel = 7
        },
        new SkillDefinition
        {
            SkillId = "archer_piercing_arrow",
            Name = "관통 화살",
            Description = "일직선으로 관통하듯 여러 대상에게 피해를 줍니다.",
            RequiredJob = JobType.Archer,
            RequiredLevel = 12,
            CooldownSeconds = 5,
            BaseDamage = 28,
            DamagePerLevel = 8,
            BaseHitCount = 1,
            HitCountPerLevels = 4,
            BaseTargetCount = 2,
            TargetCountPerLevels = 3,
            BaseRange = 290,
            RangePerLevel = 8
        },
        new SkillDefinition
        {
            SkillId = "archer_rain_arrow",
            Name = "애로우 레인",
            Description = "화살비로 넓은 범위의 적을 공격합니다.",
            RequiredJob = JobType.Archer,
            RequiredLevel = 15,
            CooldownSeconds = 6,
            BaseDamage = 22,
            DamagePerLevel = 7,
            BaseHitCount = 2,
            HitCountPerLevels = 4,
            BaseTargetCount = 4,
            TargetCountPerLevels = 2,
            BaseRange = 260,
            RangePerLevel = 6
        },
        new SkillDefinition
        {
            SkillId = "archer_snipe",
            Name = "스나이프",
            Description = "가장 가까운 적에게 초장거리 강력한 한 방을 날립니다.",
            RequiredJob = JobType.Archer,
            RequiredLevel = 18,
            CooldownSeconds = 8,
            BaseDamage = 60,
            DamagePerLevel = 15,
            BaseHitCount = 1,
            HitCountPerLevels = 3,
            BaseTargetCount = 1,
            BaseRange = 340,
            RangePerLevel = 10
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
