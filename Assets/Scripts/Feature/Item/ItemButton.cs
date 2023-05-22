using UnityEngine;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour
{
    public Image icon;
    public Text nameText;
    public Text priceText;

    public Item item;
    private ShopSystem shopSystem;

    public void SetItem(Item newItem, ShopSystem newShopSystem)
    {
        item = newItem;
        icon.sprite = item.icon;
        nameText.text = item.name;
        priceText.text = item.price.ToString();
        shopSystem = newShopSystem;
    }

    public void OnClick()
    {
        shopSystem.BuySelectedItem();
        gameObject.SetActive(false);
    }
}
