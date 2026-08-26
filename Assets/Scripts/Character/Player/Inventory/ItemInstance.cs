using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ItemInstance
{
    public Item item;
    public ItemInstanceData itemData;
}

[System.Serializable]
public class ItemInstanceData
{
    public int level;
    [System.Serializable]
    public class ModSlot
    {
        public Item.ModType modSlotType;
        public Item modItem;
    }

    public List<ModSlot> ModSlots = new List<ModSlot>();
}