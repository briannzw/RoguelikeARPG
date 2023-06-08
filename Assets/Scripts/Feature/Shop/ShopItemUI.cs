using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text itemText;
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text priceText;

    private Shop shop;
    private Item item;

    public void Initialize(Shop shop, Item item)
    {
        this.shop = shop;
        this.item = item;
        shop.OnPanelOpen += SetUI;
    }

    public void Buy()
    {
        shop.Buy(item);
        SetUI();
    }

    private void SetUI()
    {
        GetComponent<Button>().interactable = !item.isDisabled;
        itemText.text = item.name.ToString();
        itemImage.sprite = item.icon;
        priceText.text = (item.isDisabled) ? "-" : item.GetPrice().ToString();
    }
}
