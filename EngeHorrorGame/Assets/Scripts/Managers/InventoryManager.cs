using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    [SerializeField] GameObject invHolder;

    [Header("Tabs")]
    [SerializeField] GameObject[] tabs;
    [Header("Slots")]
    [SerializeField] Transform itemSlotHolder;
    [SerializeField] Transform keyItemSlotHolder;
    [HideInInspector] public Slot[] itemSlots, keyItemSlots;
    public Sprite emptySlotSprite;
    [Header("Info")]
    [SerializeField] TMP_Text infoTitleText;
    [SerializeField] TMP_Text infoText;

    [Header("Interacting")]
    public Object interactObj;
    Slot selectedCurrentSlot;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        itemSlots = new Slot[itemSlotHolder.childCount];
        keyItemSlots = new Slot[keyItemSlotHolder.childCount];

        for (int i = 0; i < itemSlots.Length; i++)
        {
            itemSlots[i] = itemSlotHolder.GetChild(i).GetComponent<Slot>();
        }
        for (int i = 0; i < keyItemSlots.Length; i++)
        {
            keyItemSlots[i] = keyItemSlotHolder.GetChild(i).GetComponent<Slot>();
        }

        keyItemSlotHolder.gameObject.SetActive(false);
        invHolder.gameObject.SetActive(false);

        HoverExit();
    }
    public void ToggleInventory(PlayerLook lookScript, PlayerMove moveScript, PlayerAttack attackScript)
    {
        bool toggle = !invHolder.activeSelf;
        invHolder.SetActive(toggle);
        lookScript.canLook = !toggle;
        moveScript.Stop();
        moveScript.canMove = !toggle;
        attackScript.canAttack = !toggle;
        if (toggle)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    public void SwitchTab(int id)
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            if (i == id)
                tabs[i].SetActive(true);
            else
                tabs[i].SetActive(false);
        }
    }
    public void AddItem(ItemPickUp _item)
    {
        Slot selectedSlot = null;
        switch (_item.item.itemType)
        {
            case 0:
                //Item
                selectedSlot = CheckHasItemInASlot(itemSlots, _item.item);
                if(selectedSlot == null)
                    selectedSlot = CheckFirstFreeSlot(itemSlots);
                if (selectedSlot != null)
                {
                    AddItemToSlot(selectedSlot, _item.item, _item.itemCount, _item);
                    _item.DestroyObj();
                }
                else
                {
                    //Inventory full
                }
                break;
            case 1:
                //KeyItem
                selectedSlot = CheckHasItemInASlot(keyItemSlots, _item.item);
                if (selectedSlot == null)
                    selectedSlot = CheckFirstFreeSlot(keyItemSlots);
                if (selectedSlot != null)
                {
                    AddItemToSlot(selectedSlot, _item.item, _item.itemCount, _item);
                    _item.DestroyObj();
                }
                else
                {
                    //Inventory full
                }
                break;
        }
    }
    public void RemoveItem(int _itemId, int _count)
    {
        RemoveItemFromArray(itemSlots, _itemId, _count);
        RemoveItemFromArray(keyItemSlots, _itemId, _count);
    }
    void RemoveItemFromArray(Slot[] slots, int _itemId, int _count)
    {
        foreach (Slot s in slots)
        {
            if (s.itemInSlot == null)
                continue;
            if (s.itemInSlot.itemId == _itemId)
                s.RemoveItem(_count);
        }
    }
    void AddItemToSlot(Slot _slot, Item _item, int _count, ItemPickUp _pickUpSlot)
    {
        if(_slot.itemCountSlot + _count > _item.maxStack)
        {
            int newValue = _item.maxStack - _slot.itemCountSlot;
            _slot.AddItem(_item, newValue);
            _pickUpSlot.ChangeCount(_count - newValue);
            AddItem(_pickUpSlot);
        }
        else
        {
            _pickUpSlot.ChangeCount(0);
            _slot.AddItem(_item, _count);
        }
    }
    Slot CheckHasItemInASlot(Slot[] slotsArray, Item _item)
    {
        foreach (Slot s in slotsArray)
        {
            if (s.itemInSlot == _item)
            {
                if (s.itemCountSlot >= _item.maxStack)
                    continue;
                return s;
            }
        }
        return null;
    }
    Slot CheckFirstFreeSlot(Slot[] slotsArray)
    {
        foreach(Slot s in slotsArray)
        {
            if (s.itemInSlot == null)
                return s;
        }
        return null;
    }
    public void SelectSlot(Slot _slot)
    {
        if(selectedCurrentSlot == null)
        {
            if (_slot.itemInSlot == null)
                return;

            selectedCurrentSlot = _slot;
        }
        else
        {
            if (selectedCurrentSlot == _slot)
                return;

            Item firstItem = selectedCurrentSlot.itemInSlot;
            int firstItemCount = selectedCurrentSlot.itemCountSlot;
            Item SecondItem = _slot.itemInSlot;
            int secondItemCount = _slot.itemCountSlot;

            _slot.RemoveItem(secondItemCount);
            selectedCurrentSlot.RemoveItem(firstItemCount);

            _slot.AddItem(firstItem, firstItemCount);

            if (SecondItem != null)
                selectedCurrentSlot.AddItem(SecondItem, secondItemCount);
            else
                selectedCurrentSlot.RemoveItem(firstItemCount);


            selectedCurrentSlot = null;
        }
    }
    public void HoverShow(Item item)
    {
        infoTitleText.text = item.itemName;
        infoText.text = item.itemInfo;
    }
    public void HoverExit()
    {
        infoTitleText.text = "";
        infoText.text = "";
    }
    public bool SearchForItem(int invIndex, int itemId)
    {
        Slot[] testSlots = null;
        switch (invIndex) 
        {
            case 0:
                testSlots = itemSlots;
                break;
            case 1:
                testSlots = keyItemSlots;
                break;
        }
        foreach(Slot s in testSlots)
        {
            if (s.itemInSlot == null)
                continue;
            if (s.itemInSlot.itemId == itemId)
            {
                return true;
            }
        }
        return false;
    }
    public int GetItemCount(int invIndex, int itemId)
    {
        int counter = 0;
        Slot[] testSlots = null;
        switch (invIndex)
        {
            case 0:
                testSlots = itemSlots;
                break;
            case 1:
                testSlots = keyItemSlots;
                break;
        }
        foreach (Slot s in testSlots)
        {
            if (s.itemInSlot == null)
                continue;
            if (s.itemInSlot.itemId == itemId)
            {
                counter += s.itemCountSlot;
            }
        }
        return counter;
    }
    public void UseItemFromInventory(Slot _slot)
    {
        if (interactObj == null)
            return;

        if(interactObj is Door)
        {
            Door door = interactObj as Door;
            if(SearchForItem(1, door.keyId))
            {
                RemoveItem(door.keyId, 1);
                door.Unlock();
            }
        }
        else if(interactObj is PianoPuzzle)
        {
            PianoPuzzle piano = interactObj as PianoPuzzle;
            if (SearchForItem(1, piano.keyId))
            {
                RemoveItem(piano.keyId, 1);
                piano.AddKey();
            }
        }
        else if(_slot.itemInSlot.itemId == 1)
        {
            if(PlayerHealth.Instance.currentHealth < PlayerHealth.Instance.health)
            {
                PlayerHealth.Instance.AddHealth(50);
                RemoveItem(1, 1);
            }
        }

        interactObj = null;
        ToggleInventory(PlayerLook.Instance, PlayerMove.Instance, PlayerAttack.Instance);
    }
}
