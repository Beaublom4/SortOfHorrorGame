using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Slot : MonoBehaviour
{
    public Item itemInSlot;
    [SerializeField] Image itemSlotImage;
    public int itemCountSlot;
    [SerializeField] TMP_Text itemCountSlotText;

    public void AddItem(Item _item, int _count)
    {
        itemInSlot = _item;
        if (itemInSlot != _item)
            itemCountSlot = 0;
        itemCountSlot += _count;
        itemCountSlotText.text = itemCountSlot.ToString();
        itemCountSlotText.gameObject.SetActive(true);
        itemSlotImage.sprite = _item.itemSprite;
    }
    public void RemoveItem(int _count)
    {
        if (itemCountSlot - _count <= 0)
        {
            itemInSlot = null;
            itemCountSlot -= _count;
            itemCountSlotText.text = 0.ToString();
            itemCountSlotText.gameObject.SetActive(false);
            itemSlotImage.sprite = GetComponentInParent<InventoryManager>().emptySlotSprite;
        }
        else
        {
            itemCountSlot -= _count;
            itemCountSlotText.text = itemCountSlot.ToString();
        }
    }
    public void SelectSlot()
    {
        GetComponentInParent<InventoryManager>().SelectSlot(this);
    }
    public void HoverEnter()
    {
        if (itemInSlot != null)
            GetComponentInParent<InventoryManager>().HoverShow(itemInSlot);
    }
    public void HoverExit()
    {
        GetComponentInParent<InventoryManager>().HoverExit();
    }
}
