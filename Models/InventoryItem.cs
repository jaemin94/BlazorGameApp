namespace MyBlazorApp.Models;

// 인벤토리 아이템
public class InventoryItem
{
    public string Name { get; set; } = "";
    public int Count { get; set; } = 0;

    // Weapon, Armor, Consumable, Material
    public string ItemType { get; set; } = "Material";

    public int AttackBonus { get; set; } = 0;
    public int DefenseBonus { get; set; } = 0;
    public string Rarity { get; set; } = "Normal";

    // 구매가 / 판매가 계산용
    public int BuyPrice { get; set; } = 0;
    public int SellPrice => Math.Max(1, BuyPrice / 2);
}
