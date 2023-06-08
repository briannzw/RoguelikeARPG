using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public new string name;
    public int price;
    public Sprite icon;
    public string description;
    public bool isConsumable;
    public int treasureSpawnChance;
    public bool isDisabled;
    
    void Start(){
        // Get a reference to the PlayerStats script attached to the player object
        //player = GameObject.FindWithTag("Player");
    }

    // Special effects for each item
    public virtual void Use()
    {
        // Implement special effect for this item
    }

    public virtual int GetPrice()
    {
        return price;
    }
}
