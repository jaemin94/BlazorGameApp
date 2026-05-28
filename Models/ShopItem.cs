namespace MyBlazorApp.Models;

// 상점 판매 아이템
public class ShopItem
{
    public string Name { get; set; } = "";
    public string ItemType { get; set; } = "Material";
    public int Price { get; set; }
    public int AttackBonus { get; set; }
    public int DefenseBonus { get; set; }
    public string Rarity { get; set; } = "Normal";
    public BuildingType ShopType { get; set; }
}
