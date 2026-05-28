namespace MyBlazorApp.Models;

// 플레이어 상태
public class PlayerState
{
    public string ConnectionId { get; set; } = "";
    public string Name { get; set; } = "";
    public string RoomCode { get; set; } = "";

    public MapType CurrentMap { get; set; } = MapType.Town;
    public int X { get; set; } = 250;
    public int Y { get; set; } = 250;

    public int Hp { get; set; } = 100;
    public int MaxHp { get; set; } = 100;
    public int Level { get; set; } = 1;
    public int Exp { get; set; } = 0;
    public int Gold { get; set; } = 0;

    // 캐릭터 커스터마이징 정보
    // CSS에서 색상으로 캐릭터를 그리기 때문에 이미지 파일 없이도 외형 변경 가능
    public string SkinColor { get; set; } = "#f2c7a5";
    public string HairColor { get; set; } = "#2b1b12";
    public string OutfitColor { get; set; } = "#2563eb";
    public string HairStyle { get; set; } = "short";
    public string FaceIcon { get; set; } = "🙂";
    public string OutfitStyle { get; set; } = "adventurer";
    public string Accessory { get; set; } = "none";

    public int Str { get; set; } = 1;
    public int Def { get; set; } = 1;
    public int Int { get; set; } = 1;
    public int StatPoint { get; set; } = 0;

    public int SkillPoint { get; set; } = 0;
    public JobType Job { get; set; } = JobType.Beginner;
    public bool IsJobChanged => Job != JobType.Beginner;

    public int BaseAttackPower { get; set; } = 10;
    public int BaseDefense { get; set; } = 0;

    public EquipmentItem? Weapon { get; set; }
    public EquipmentItem? Armor { get; set; }

    public int SlimeKillCount { get; set; } = 0;
    public int BatKillCount { get; set; } = 0;
    public int GoblinKillCount { get; set; } = 0;
    public int BossKillCount { get; set; } = 0;

    public bool QuestAccepted { get; set; } = false;
    public bool QuestCompleted { get; set; } = false;
    public bool QuestRewardReceived { get; set; } = false;

    public List<PlayerQuestState> Quests { get; set; } = new();
    public List<InventoryItem> Inventory { get; set; } = new();

    // 배운 스킬. 초보자는 강타만 기본 보유
    public List<PlayerSkillState> LearnedSkills { get; set; } = new()
    {
        new PlayerSkillState { SkillId = "basic_strike", Level = 1, MaxLevel = 1 }
    };

    // 1~9 퀵슬롯. 자동 등록하지 않고 빈칸으로 시작
    public List<SkillSlot> SkillSlots { get; set; } = Enumerable.Range(1, 9)
        .Select(i => new SkillSlot { Slot = i })
        .ToList();

    public int AttackPower => BaseAttackPower + Str * 2 + (Weapon?.AttackBonus ?? 0);
    public int Defense => BaseDefense + Def + (Armor?.DefenseBonus ?? 0);
    public int MagicPower => Int * 4;
    public int HealPower => 30 + Int * 5;
}
