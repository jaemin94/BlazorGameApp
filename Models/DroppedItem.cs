namespace MyBlazorApp.Models;

// 바닥에 떨어진 아이템 / 골드
public class DroppedItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";

    public int X { get; set; }
    public int Y { get; set; }

    public int GoldAmount { get; set; } = 0;
    public string ItemType { get; set; } = "Material";
    public int AttackBonus { get; set; } = 0;
    public int DefenseBonus { get; set; } = 0;
    public string Rarity { get; set; } = "Normal";

    public bool IsGold => GoldAmount > 0;
}
