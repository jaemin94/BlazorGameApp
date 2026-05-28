namespace MyBlazorApp.Models;

// 필드 드랍 아이템
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
    public int BuyPrice { get; set; } = 0;
    public bool IsGold => GoldAmount > 0;
}
