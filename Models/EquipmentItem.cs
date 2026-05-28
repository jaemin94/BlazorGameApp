namespace MyBlazorApp.Models;

// 장착 아이템 정보
public class EquipmentItem
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = ""; // Weapon / Armor
    public int AttackBonus { get; set; } = 0;
    public int DefenseBonus { get; set; } = 0;
    public string Rarity { get; set; } = "Normal";
}
