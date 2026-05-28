namespace MyBlazorApp.Models;

// 인벤토리 아이템 정보
public class InventoryItem
{
    public string Name { get; set; } = "";
    public int Count { get; set; } = 0;
    public string ItemType { get; set; } = "Material"; // Material / Consumable / Weapon / Armor
    public int AttackBonus { get; set; } = 0;
    public int DefenseBonus { get; set; } = 0;
    public string Rarity { get; set; } = "Normal";
}
