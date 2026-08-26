using UnityEngine;

[CreateAssetMenu]
public class Item : ScriptableObject
{
    public string itemID;
    public enum ItemTypes
    {
        Any, Mod, Relic, Weapon
    }
    public ItemTypes itemType;
    public enum ModType
    {
        Red, Purple, Blue
    }
    public ModType modType;

    [Space]

    public string itemName;
    public string shortItemDescription;
    public string itemDescription;
    public Sprite itemIcon;
    public GameObject itemObject;
}
