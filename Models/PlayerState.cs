namespace MyBlazorApp.Models;

// 플레이어 전체 상태
public class PlayerState
{
    // 접속 / 방 정보
    public string ConnectionId { get; set; } = "";
    public string Name { get; set; } = "";
    public string RoomCode { get; set; } = "";

    // 현재 맵 / 위치
    public MapType CurrentMap { get; set; } = MapType.Town;
    public int X { get; set; } = 250;
    public int Y { get; set; } = 250;

    // 기본 능력치
    public int Hp { get; set; } = 100;
    public int MaxHp { get; set; } = 100;
    public int Level { get; set; } = 1;
    public int Exp { get; set; } = 0;
    public int Gold { get; set; } = 0;

    // 스탯
    public int Str { get; set; } = 1;
    public int Def { get; set; } = 1;
    public int Int { get; set; } = 1;
    public int StatPoint { get; set; } = 0;

    // 기본 공격력 / 방어력
    public int BaseAttackPower { get; set; } = 10;
    public int BaseDefense { get; set; } = 0;

    // 장비
    public EquipmentItem? Weapon { get; set; }
    public EquipmentItem? Armor { get; set; }

    // 처치 수
    public int SlimeKillCount { get; set; } = 0;
    public int BatKillCount { get; set; } = 0;
    public int GoblinKillCount { get; set; } = 0;
    public int BossKillCount { get; set; } = 0;

    // 구버전 단일 퀘스트 호환용
    public bool QuestAccepted { get; set; } = false;
    public bool QuestCompleted { get; set; } = false;
    public bool QuestRewardReceived { get; set; } = false;

    // 신버전 다중 퀘스트
    public List<PlayerQuestState> Quests { get; set; } = new();

    // 인벤토리
    public List<InventoryItem> Inventory { get; set; } = new();

    // 퀵슬롯
    public List<SkillSlot> SkillSlots { get; set; } = new()
    {
        new SkillSlot { Slot = 1, SkillName = "파이어볼", CooldownSeconds = 2 },
        new SkillSlot { Slot = 2, SkillName = "힐", CooldownSeconds = 5 },
        new SkillSlot { Slot = 3, SkillName = "회전베기", CooldownSeconds = 3 }
    };

    // 계산 능력치
    public int AttackPower => BaseAttackPower + Str * 2 + (Weapon?.AttackBonus ?? 0);
    public int Defense => BaseDefense + Def + (Armor?.DefenseBonus ?? 0);
    public int HealPower => 30 + Int * 5;
}
