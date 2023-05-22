using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public PlayerStats playerStats;
    public new string name;
    public int price;
    public Sprite icon;
    public string description;
    public bool isConsumable;
    
    void Start(){
        // Get a reference to the PlayerStats script attached to the player object
        playerStats = GameObject.FindWithTag("Player").GetComponent<PlayerStats>();
    }

    // Special effects for each item
    public virtual void Use()
    {
        // Implement special effect for this item
    }
}
