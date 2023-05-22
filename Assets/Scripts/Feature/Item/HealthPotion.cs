using UnityEngine;

[CreateAssetMenu(fileName = "New Health Potion", menuName = "Inventory/Health Potion")]
public class HealthPotion : Item
{
    public float healAmount;

    public override void Use()
    {
        healAmount = 25f;
        // Restore player's health by "healAmount" points
        playerStats.Heal(healAmount);
    }
}