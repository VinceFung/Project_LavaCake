using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerInventory : MonoBehaviour
{
    public Entity entity;
    public GameObject InventoryObject;
    public AttackAnimationController animController;
    public Transform playerWeaponHolder;
    public Transform playerRelicHolder;
    public GameObject Fists;

    [System.Serializable]
    public class ItemSlot
    {
        public Item.ItemTypes slotType = Item.ItemTypes.Any;
        public ItemInstance itemInstance;
        public Button slotButton;
        public Image slotImage;
    }

    [Space]
    public ItemSlot EquippedWeapon;
    public ItemSlot[] EquippedRelics = new ItemSlot[4];
    public ItemSlot[] InventoryItems = new ItemSlot[40];

    [Space]
    public GameObject InventoryItemHolder;
    public GameObject ItemSlotPrefab;
    public GameObject WeaponMenu;
    public GameObject DescriptionMenu;

    public ItemInstance selectedItem;
    public ItemInstance heldItem;
    public Image itemCursor;

    [Space]
    [Header("Gamepad Support")]
    public GameObject gamepadCursor;
    public float gamepadCursorSpeed = 500f;
    public InputActionReference gamepadMoveAction;
    public InputActionReference gamepadSelectAction;
    public InputActionReference gamepadInspectAction;
    
    [Space]
    [Header("Item Drop Settings")]
    public float dropRadius = 2f; // How far from player to spawn dropped items
    
    private Vector2 gamepadCursorPosition;
    private bool usingGamepad;
    private ItemSlot highlightedSlot;
    private List<ItemSlot> allInteractableSlots = new List<ItemSlot>();
    private PlayerInput playerInput;

    List<EntityRelic> instantiatedRelics = new List<EntityRelic>();
    bool activeStateLastFrame;
    private List<ItemSlot> externalSlots = new List<ItemSlot>();

    private void Start()
    {
        if (heldItem == null) heldItem = new ItemInstance();
        ConfigureSlotTypes();
        InitializeInventorySlots();
        
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null) playerInput = FindFirstObjectByType<PlayerInput>();
        
        if (gamepadCursor != null)
        {
            gamepadCursorPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            gamepadCursor.SetActive(false);
        }
        
        if (gamepadSelectAction != null) gamepadSelectAction.action.Enable();
        if (gamepadInspectAction != null) gamepadInspectAction.action.Enable();
        if (gamepadMoveAction != null) gamepadMoveAction.action.Enable();
    }

    private void OnDestroy()
    {
        if (gamepadSelectAction != null) gamepadSelectAction.action.Disable();
        if (gamepadInspectAction != null) gamepadInspectAction.action.Disable();
        if (gamepadMoveAction != null) gamepadMoveAction.action.Disable();
    }

    private void Update()
    {
        SpawnWeapon();
        UpdateRelicObjects();

        if (activeStateLastFrame != InventoryObject.activeSelf)
        {
            if (InventoryObject.activeSelf) RenderItems();
        }
        activeStateLastFrame = InventoryObject.activeSelf;

        UpdateCursor();
        UpdateGamepadInput();
        
        // Handle mouse clicks outside slots when inventory is open
        if (InventoryObject.activeSelf && !usingGamepad)
        {
            HandleMouseDropInput();
        }

        if (selectedItem?.item != null)
        {
            if (selectedItem.item.itemType == Item.ItemTypes.Weapon)
            {
                bool wasInactive = !WeaponMenu.activeSelf;
                WeaponMenu.SetActive(true);
                DescriptionMenu.SetActive(false);
                if (wasInactive) StartCoroutine(RenderAfterFrame());
            }
            else
            {
                WeaponMenu.SetActive(false);
                DescriptionMenu.SetActive(true);
            }
        }
        else
        {
            WeaponMenu.SetActive(false);
            DescriptionMenu.SetActive(false);
        }
    }

    System.Collections.IEnumerator RenderAfterFrame()
    {
        yield return null;
        RenderItems();
    }

    void UpdateCursor()
    {
        if (heldItem?.item != null)
        {
            itemCursor.sprite = heldItem.item.itemIcon;
            itemCursor.enabled = true;
            itemCursor.transform.position = usingGamepad ? gamepadCursorPosition : Mouse.current.position.ReadValue();
        }
        else
        {
            itemCursor.enabled = false;
        }
        
        if (gamepadCursor != null)
        {
            gamepadCursor.SetActive(usingGamepad && InventoryObject.activeSelf);
        }
    }

    public void RenderItems()
    {
        InitializeInventorySlots();
        
        if (InventoryItemHolder != null)
        {
            foreach (Transform child in InventoryItemHolder.transform) 
                if (child != null) Destroy(child.gameObject);

            foreach (var slot in InventoryItems) 
                if (slot != null) RenderSlot(slot, InventoryItemHolder.transform);
        }
        
        RenderEquippedSlot(EquippedWeapon);
        foreach (var slot in EquippedRelics) 
            if (slot != null) RenderEquippedSlot(slot);

        foreach (var slot in externalSlots)
            if (slot != null) RenderEquippedSlot(slot);
    }

    void RenderSlot(ItemSlot slot, Transform parent)
    {
        if (slot?.itemInstance == null) return;
        
        var obj = Instantiate(ItemSlotPrefab, parent);
        var image = obj.transform.childCount > 0 ? obj.transform.GetChild(0).GetComponent<Image>() : null;
        
        if (image != null)
        {
            bool hasItem = slot.itemInstance.item != null;
            image.gameObject.SetActive(hasItem);
            if (hasItem) image.sprite = slot.itemInstance.item.itemIcon;
            
            slot.slotImage = image;
        }
        
        AddClickHandlers(obj, slot);
    }

    void RenderEquippedSlot(ItemSlot slot)
    {
        if (slot?.itemInstance == null) return;
        
        bool hasItem = slot.itemInstance.item != null;
        if (slot.slotImage != null)
        {
            slot.slotImage.gameObject.SetActive(hasItem);
            if (hasItem) slot.slotImage.sprite = slot.itemInstance.item.itemIcon;
        }
        
        if (slot.slotButton != null)
        {
            var trigger = slot.slotButton.GetComponent<EventTrigger>();
            if (trigger != null) trigger.triggers.Clear();
            AddClickHandlers(slot.slotButton.gameObject, slot);
        }
    }

    void AddClickHandlers(GameObject obj, ItemSlot slot)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>() ?? obj.AddComponent<EventTrigger>();
        trigger.triggers.Clear();

        EventTrigger.Entry leftClick = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        leftClick.callback.AddListener((data) =>
        {
            PointerEventData ped = (PointerEventData)data;
            if (ped.button == PointerEventData.InputButton.Left) HandleSlotClick(slot);
        });

        EventTrigger.Entry rightClick = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        rightClick.callback.AddListener((data) =>
        {
            PointerEventData ped = (PointerEventData)data;
            if (ped.button == PointerEventData.InputButton.Right) 
            {
                selectedItem = slot.itemInstance;
                RenderItems();
            }
        });

        trigger.triggers.Add(leftClick);
        trigger.triggers.Add(rightClick);
    }

    public void HandleSlotClick(ItemSlot slot)
    {
        if (slot?.itemInstance == null) return;

        if (heldItem.item == null)
        {
            if (slot.itemInstance.item != null)
            {
                heldItem.item = slot.itemInstance.item;
                heldItem.itemData = slot.itemInstance.itemData;
                slot.itemInstance.item = null;
                slot.itemInstance.itemData = null;
                RenderItems();
            }
            return;
        }

        if (!CanPlaceItemInSlot(heldItem.item, slot.slotType)) return;

        if (slot.itemInstance.item == null)
        {
            slot.itemInstance.item = heldItem.item;
            slot.itemInstance.itemData = heldItem.itemData;
            heldItem.item = null;
            heldItem.itemData = null;
        }
        else
        {
            var tempItem = slot.itemInstance.item;
            var tempData = slot.itemInstance.itemData;
            slot.itemInstance.item = heldItem.item;
            slot.itemInstance.itemData = heldItem.itemData;
            heldItem.item = tempItem;
            heldItem.itemData = tempData;
        }
        
        RenderItems();
    }

    bool CanPlaceItemInSlot(Item item, Item.ItemTypes slotType) 
    {
        if (item == null) return false;
        return slotType == Item.ItemTypes.Any || item.itemType == slotType;
    }

    void SpawnWeapon()
    {
        var weaponItem = EquippedWeapon.itemInstance.item;
        
        if (weaponItem != null)
        {
            Fists.SetActive(false);
            
            if (entity.meleeWeapon?.weaponItem != weaponItem)
            {
                if (entity.meleeWeapon?.gameObject != Fists) 
                    Destroy(entity.meleeWeapon?.gameObject);
                
                var obj = Instantiate(weaponItem.itemObject, playerWeaponHolder);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;

                var weapon = obj.GetComponent<MeleeWeapon>();
                weapon.Wielder = entity;
                weapon.animController = animController;
                weapon.stats.data = EquippedWeapon.itemInstance.itemData;
            }
        }
        else
        {
            if (!Fists.activeSelf)
            {
                foreach (Transform child in playerWeaponHolder) Destroy(child.gameObject);
                Fists.SetActive(true);
            }
        }
    }

    private void UpdateRelicObjects()
    {
        var currentRelics = new List<string>();
        foreach (var slot in EquippedRelics)
            if (slot?.itemInstance?.item != null)
                currentRelics.Add(slot.itemInstance.item.itemID);

        bool changed = currentRelics.Count != instantiatedRelics.Count;
        if (!changed)
        {
            for (int i = 0; i < currentRelics.Count; i++)
            {
                if (instantiatedRelics[i].name != currentRelics[i])
                {
                    changed = true;
                    break;
                }
            }
        }

        if (changed)
        {
            foreach (Transform child in playerRelicHolder) Destroy(child.gameObject);
            instantiatedRelics.Clear();

            foreach (var slot in EquippedRelics)
            {
                if (slot.itemInstance?.item != null)
                {
                    var obj = Instantiate(slot.itemInstance.item.itemObject, playerRelicHolder);
                    obj.name = slot.itemInstance.item.itemID;

                    var relic = obj.GetComponent<EntityRelic>();
                    if (relic != null)
                    {
                        relic.owner = entity;
                        instantiatedRelics.Add(relic);
                    }
                }
            }
        }
    }

    void ConfigureSlotTypes()
    {
        EquippedWeapon.itemInstance ??= new ItemInstance();
        EquippedWeapon.slotType = Item.ItemTypes.Weapon;

        for (int i = 0; i < EquippedRelics.Length; i++)
        {
            EquippedRelics[i].itemInstance ??= new ItemInstance();
            EquippedRelics[i].slotType = Item.ItemTypes.Relic;
        }

        for (int i = 0; i < InventoryItems.Length; i++)
        {
            InventoryItems[i].itemInstance ??= new ItemInstance();
            InventoryItems[i].slotType = Item.ItemTypes.Any;
        }
    }

    void InitializeInventorySlots()
    {
        foreach (var slot in InventoryItems) 
            if (slot.itemInstance == null) slot.itemInstance = new ItemInstance();
        
        if (EquippedWeapon.itemInstance == null) EquippedWeapon.itemInstance = new ItemInstance();
            
        foreach (var slot in EquippedRelics) 
            if (slot.itemInstance == null) slot.itemInstance = new ItemInstance();

        foreach (var slot in externalSlots)
            if (slot.itemInstance == null) slot.itemInstance = new ItemInstance();
    }

    public bool TryAddItem(Item item, ItemInstanceData data = null)
    {
        if (item == null) return false;
        
        foreach (var slot in InventoryItems)
        {
            if (slot.itemInstance?.item == null && CanPlaceItemInSlot(item, slot.slotType))
            {
                slot.itemInstance.item = item;
                slot.itemInstance.itemData = data ?? new ItemInstanceData();
                RenderItems();
                return true;
            }
        }
        
        return false;
    }

    public void RegisterExternalSlot(ItemSlot slot)
    {
        if (slot != null && !externalSlots.Contains(slot))
        {
            externalSlots.Add(slot);
            if (slot.itemInstance == null) slot.itemInstance = new ItemInstance();
        }
    }

    public void UnregisterExternalSlot(ItemSlot slot)
    {
        externalSlots.Remove(slot);
    }

    void UpdateGamepadInput()
    {
        if (playerInput == null || !InventoryObject.activeSelf) return;
        
        bool isGamepad = playerInput.currentControlScheme == "Gamepad";
        
        if (isGamepad != usingGamepad)
        {
            usingGamepad = isGamepad;
            if (gamepadCursor != null) gamepadCursor.SetActive(usingGamepad && InventoryObject.activeSelf);
        }

        if (!usingGamepad) return;

        Vector2 moveInput = gamepadMoveAction != null ? gamepadMoveAction.action.ReadValue<Vector2>() : Vector2.zero;
        
        gamepadCursorPosition.x += moveInput.x * gamepadCursorSpeed * Time.unscaledDeltaTime;
        gamepadCursorPosition.y += moveInput.y * gamepadCursorSpeed * Time.unscaledDeltaTime;
        
        gamepadCursorPosition.x = Mathf.Clamp(gamepadCursorPosition.x, 0, Screen.width);
        gamepadCursorPosition.y = Mathf.Clamp(gamepadCursorPosition.y, 0, Screen.height);
        
        if (gamepadCursor != null)
            gamepadCursor.transform.position = gamepadCursorPosition;

        UpdateGamepadHighlight();
        HandleGamepadInput();
    }

    void UpdateGamepadHighlight()
    {
        RefreshAllSlots();
        
        ItemSlot closestSlot = null;
        float closestDistance = float.MaxValue;
        float maxSlotDistance = 100f; // Maximum distance to consider a slot as highlighted
        
        foreach (var slot in allInteractableSlots)
        {
            Vector3 slotPos = GetSlotPosition(slot);
            if (slotPos == Vector3.zero) continue;
            
            float distance = Vector2.Distance(gamepadCursorPosition, slotPos);
            
            if (distance < closestDistance && distance < maxSlotDistance)
            {
                closestDistance = distance;
                closestSlot = slot;
            }
        }
        
        highlightedSlot = closestSlot;
    }

    Vector3 GetSlotPosition(ItemSlot slot)
    {
        if (slot?.slotButton != null)
            return slot.slotButton.transform.position;
        
        if (slot?.slotImage != null)
            return slot.slotImage.transform.position;
        
        return Vector3.zero;
    }

    void HandleGamepadInput()
    {
        if (gamepadSelectAction != null && gamepadSelectAction.action.WasPressedThisFrame())
        {
            if (highlightedSlot != null)
            {
                HandleSlotClick(highlightedSlot);
            }
            else
            {
                // No slot highlighted - drop held item if we have one
                DropHeldItem();
            }
        }
        
        if (gamepadInspectAction != null && gamepadInspectAction.action.WasPressedThisFrame())
        {
            if (highlightedSlot != null)
            {
                selectedItem = highlightedSlot.itemInstance;
                RenderItems();
            }
        }
    }

    void HandleMouseDropInput()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && heldItem?.item != null)
        {
            // Check if mouse is over any UI element that's a slot
            if (!IsMouseOverSlot())
            {
                DropHeldItem();
            }
        }
    }
    
    bool IsMouseOverSlot()
    {
        // Use EventSystem to check what's under the mouse
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };
        
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        foreach (var result in results)
        {
            // Check if we're over any slot-related UI
            GameObject obj = result.gameObject;
            
            // Check for Button component (slots are buttons)
            if (obj.GetComponent<Button>() != null)
                return true;
            
            // Check for slot-related names
            if (obj.name.Contains("Slot") || obj.name.Contains("slot"))
                return true;
            
            // Check parent names
            Transform parent = obj.transform.parent;
            if (parent != null && (parent.name.Contains("Slot") || parent.name.Contains("slot")))
                return true;
            
            // Check if this is one of our ItemSlotPrefab instances
            if (obj.name.Contains("ItemSlotPrefab") || obj.name.Contains("ItemSlot"))
                return true;
        }
        
        return false;
    }
    
    void DropHeldItem()
    {
        if (heldItem?.item == null) return;
        
        // Calculate drop position near the player
        Vector3 dropPosition = transform.position + transform.forward * dropRadius;
        dropPosition.y = transform.position.y; // Keep same Y level as player
        
        // Spawn the held item in the world
        GameManager.Instance.SpawnItemPickUp(heldItem.item, heldItem.itemData, dropPosition);
        
        // Clear the held item
        heldItem.item = null;
        heldItem.itemData = null;
        
        // Update UI
        RenderItems();
    }
    
    void RefreshAllSlots()
    {
        allInteractableSlots.Clear();
        allInteractableSlots.Add(EquippedWeapon);
        allInteractableSlots.AddRange(EquippedRelics);
        allInteractableSlots.AddRange(InventoryItems);
        allInteractableSlots.AddRange(externalSlots);
    }
}
