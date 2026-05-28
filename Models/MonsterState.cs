namespace MyBlazorApp.Models;

// 몬스터 상태
public class MonsterState
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public MonsterType Type { get; set; } = MonsterType.Slime;
    public string Name { get; set; } = "슬라임";
    public int X { get; set; } = 500;
    public int Y { get; set; } = 250;
    public int Hp { get; set; } = 30;
    public int MaxHp { get; set; } = 30;
    public int AttackDamage { get; set; } = 5;
    public int ExpReward { get; set; } = 10;
    public int GoldReward { get; set; } = 10;
    public DateTime LastAttackTime { get; set; } = DateTime.MinValue;
}
