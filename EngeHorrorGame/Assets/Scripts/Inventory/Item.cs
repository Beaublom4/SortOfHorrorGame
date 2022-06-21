using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "New Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public int itemId;
    public Sprite itemSprite;
    [TextArea]
    public string itemInfo;

    [Tooltip("0: Item, 1: Key Item")]
    public int itemType;
    public int maxStack;
}
