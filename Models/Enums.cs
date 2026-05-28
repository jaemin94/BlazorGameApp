namespace MyBlazorApp.Models;

// 맵 종류
public enum MapType
{
    Town,
    Field,
    BossRoom
}

// 몬스터 종류
public enum MonsterType
{
    Slime,
    Bat,
    Goblin,
    Boss
}

// 직업 종류
public enum JobType
{
    Beginner,
    Mage,
    Warrior,
    Thief,
    Archer
}

// 인벤토리 탭
public enum InventoryTab
{
    All,
    Equipment,
    Potion,
    Etc
}

// 건물 종류
public enum BuildingType
{
    PotionShop,
    WeaponShop,
    ArmorShop,
    JobHall,
    QuestHall
}
