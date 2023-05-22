using UnityEngine;

[CreateAssetMenu(fileName = "New Health Potion", menuName = "Inventory/Health Potion")]
public class HealthPotion : Item
{
    
    public Damage healAmount;

    public override void Use()
    {
        healAmount.value = -25f;
        // Restore player's health by "healAmount" points
        playerStats.TakeDamage(healAmount);
    }
}