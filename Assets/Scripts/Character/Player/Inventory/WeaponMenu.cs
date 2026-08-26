using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WeaponMenu : MonoBehaviour
{
    public TextMeshProUGUI NameText;
    public Image Icon;
    public PlayerInventory playerInventory;

    [System.Serializable]
    public class WeaponModSlot
    {
        public GameObject slotObj;
        public PlayerInventory.ItemSlot ModSlot;
        public TextMeshProUGUI DetailsText;
    }
    public WeaponModSlot[] weaponModSlots;

    private void Start()
    {
        for (int i = 0; i < weaponModSlots.Length; i++)
        {
            weaponModSlots[i].ModSlot.slotType = Item.ItemTypes.Mod;
            playerInventory.RegisterExternalSlot(weaponModSlots[i].ModSlot);
        }
    }

    private void OnDestroy()
    {
        if (playerInventory != null)
        {
            for (int i = 0; i < weaponModSlots.Length; i++)
                playerInventory.UnregisterExternalSlot(weaponModSlots[i].ModSlot);
        }
    }

    private void OnEnable()
    {
        LoadSelectedWeaponData();
    }

    private void Update()
    {
        if (playerInventory.selectedItem?.item != null)
        {
            UpdateDisplay();
            SaveModDataToWeapon();
        }
    }

    void LoadSelectedWeaponData()
    {
        var weapon = playerInventory.selectedItem;
        if (weapon?.item?.itemType != Item.ItemTypes.Weapon) return;
        
        if (weapon.itemData == null) weapon.itemData = new ItemInstanceData();
        
        for (int i = 0; i < weaponModSlots.Length && i < weapon.itemData.ModSlots.Count; i++)
        {
            var modItem = weapon.itemData.ModSlots[i].modItem;
            weaponModSlots[i].ModSlot.itemInstance.item = modItem;
            weaponModSlots[i].ModSlot.itemInstance.itemData = modItem != null ? new ItemInstanceData() : null;
        }
        
        playerInventory.RenderItems();
    }

    void SaveModDataToWeapon()
    {
        var weapon = playerInventory.selectedItem;
        if (weapon?.itemData?.ModSlots == null) return;
        
        for (int i = 0; i < weaponModSlots.Length && i < weapon.itemData.ModSlots.Count; i++)
        {
            weapon.itemData.ModSlots[i].modItem = weaponModSlots[i].ModSlot.itemInstance.item;
        }
    }

    void UpdateDisplay()
    {
        var weapon = playerInventory.selectedItem;
        
        NameText.text = weapon.itemData.level > 0 ? 
            $"{weapon.item.itemName}+{weapon.itemData.level}" : 
            weapon.item.itemName;
        
        Icon.sprite = weapon.item.itemIcon;

        for (int i = 0; i < weaponModSlots.Length; i++)
        {
            bool hasSlot = i < weapon.itemData.ModSlots.Count;
            weaponModSlots[i].slotObj.SetActive(hasSlot);
            
            if (hasSlot)
            {
                var modItem = weaponModSlots[i].ModSlot.itemInstance.item;
                weaponModSlots[i].DetailsText.text = modItem != null ? 
                    $"{modItem.itemName}\n{modItem.itemDescription}" : 
                    "No Mod Equipped";
            }
        }
    }
}