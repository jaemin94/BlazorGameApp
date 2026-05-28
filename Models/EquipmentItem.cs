namespace MyBlazorApp.Models;

// 장비 아이템
public class EquipmentItem
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public int AttackBonus { get; set; } = 0;
    public int DefenseBonus { get; set; } = 0;
    public string Rarity { get; set; } = "Normal";
    public int BuyPrice { get; set; } = 0;
}
