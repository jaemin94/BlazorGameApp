namespace MyBlazorApp.Models;

public class PlayerState
{
    // ==============================
    // 접속 / 위치
    // ==============================
    public string ConnectionId { get; set; } = "";
    public string Name { get; set; } = "";
    public string RoomCode { get; set; } = "";
    public MapType CurrentMap { get; set; } = MapType.Town;

    public int X { get; set; } = 250;
    public int Y { get; set; } = 250;

    // ==============================
    // 기본 능력치
    // ==============================
    public int Hp { get; set; } = 100;
    public int MaxHp { get; set; } = 100;

    public int Level { get; set; } = 1;
    public int Exp { get; set; } = 0;
    public int Gold { get; set; } = 0;

    public int Str { get; set; } = 1;
    public int Def { get; set; } = 1;
    public int Int { get; set; } = 1;
    public int StatPoint { get; set; } = 0;

    public int BaseAttackPower { get; set; } = 10;
    public int BaseDefense { get; set; } = 0;

    // ==============================
    // 직업 / 스킬
    // ==============================
    public JobType Job { get; set; } = JobType.Beginner;
    public bool IsJobChanged => Job != JobType.Beginner;

    // 전직 전에는 스킬포인트를 주지 않음.
    // 전직 시 +1, 전직 이후 레벨업마다 +1.
    public int SkillPoint { get; set; } = 0;

    // 처음에는 기본 스킬만 보유
    public List<PlayerSkillState> LearnedSkills { get; set; } = new()
    {
        new PlayerSkillState
        {
            SkillId = "basic_strike",
            Level = 1,
            MaxLevel = 1
        }
    };

    // 퀵슬롯은 비워둔다. 플레이어가 직접 등록.
    public List<SkillSlot> SkillSlots { get; set; } = new();

    // ==============================
    // 장비 / 인벤토리
    // ==============================
    public EquipmentItem? Weapon { get; set; }
    public EquipmentItem? Armor { get; set; }
    public List<InventoryItem> Inventory { get; set; } = new();

    // ==============================
    // 처치 카운트
    // ==============================
    public int SlimeKillCount { get; set; } = 0;
    public int BatKillCount { get; set; } = 0;
    public int GoblinKillCount { get; set; } = 0;
    public int BossKillCount { get; set; } = 0;

    // ==============================
    // 기존 상인 퀘스트 호환용
    // ==============================
    public bool QuestAccepted { get; set; } = false;
    public bool QuestCompleted { get; set; } = false;
    public bool QuestRewardReceived { get; set; } = false;

    public List<PlayerQuestState> Quests { get; set; } = new();

    // ==============================
    // 계산 능력치
    // ==============================
    public int AttackPower =>
        BaseAttackPower + Str * 2 + (Weapon?.AttackBonus ?? 0);

    public int Defense =>
        BaseDefense + Def + (Armor?.DefenseBonus ?? 0);

    public int MagicPower =>
        Int * 3;

    public int HealPower =>
        30 + Int * 5;
}
