using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameSaveLoad : MonoBehaviour
{
    public static GameSaveLoad Instance;

    public string saveFileName = "GameSave.json";

    [System.Flags]
    public enum OperationMode
    {
        DISK_OP = 1,
        RAM_OP = 2
    }

    [System.Flags]
    public enum LoadComponents
    {
        LOADBIT_PLAYER_INVENTORY = 1,
        LOADBIT_GAME_MANAGER = 2,
        LOADBIT_SAVETIMESTAMP = 4,
        LOADBIT_SAVECOUNT = 8,
        ALL_COMPONENTS = LOADBIT_PLAYER_INVENTORY | LOADBIT_GAME_MANAGER | LOADBIT_SAVETIMESTAMP | LOADBIT_SAVECOUNT
    }

    private string saveFilePath;
    private float autoSaveTimer;

    public int saveCount = 0;
    
    private GameSaveData ramSaveData;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        saveFilePath = Path.Combine(Application.persistentDataPath, saveFileName);
        Debug.Log($"Save file path: {saveFilePath}");
    }

    private void Update()
    {
    }

    public void SaveGame(OperationMode operation = OperationMode.DISK_OP)
    {
        try
        {
            if (UnitManager.Instance?.playerInventory == null)
            {
                Debug.LogWarning("Cannot save game: Player inventory not found");
                return;
            }

            GameSaveData saveData = new GameSaveData();

            saveData.playerInventory = SavePlayerInventory();
            saveData.gameManagerData = SaveGameManagerData();
            saveData.saveTimestamp = System.DateTime.Now.ToBinary();
            
            // Only increment save count when saving to disk
            if ((operation & OperationMode.DISK_OP) != 0)
            {
                saveCount++;
                saveData.saveCount = saveCount;
            }
            else
            {
                // For RAM saves, use the current save count without incrementing
                saveData.saveCount = saveCount;
            }

            string jsonData = JsonUtility.ToJson(saveData, true);

            Debug.Log("=== SAVE DATA STRUCTURE ===");
            Debug.Log($"Save Operation: {operation}");
            Debug.Log($"Save Timestamp: {System.DateTime.FromBinary(saveData.saveTimestamp):yyyy-MM-dd HH:mm:ss}");
            Debug.Log($"Save Count: {saveData.saveCount}");
            Debug.Log($"JSON Data to be saved:\n{jsonData}");
            Debug.Log("=== END SAVE DATA ===");

            if ((operation & OperationMode.RAM_OP) != 0)
            {
                ramSaveData = saveData;
                Debug.Log("Game saved to RAM successfully!");
            }

            if ((operation & OperationMode.DISK_OP) != 0)
            {
                File.WriteAllText(saveFilePath, jsonData);
                Debug.Log("Game saved to disk successfully!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
        }
    }

    public void LoadGame(OperationMode operation = OperationMode.DISK_OP, LoadComponents components = LoadComponents.ALL_COMPONENTS)
    {
        try
        {
            GameSaveData saveData = null;
            string loadSource = "";

            if ((operation & OperationMode.RAM_OP) != 0 && ramSaveData != null)
            {
                saveData = ramSaveData;
                loadSource = "RAM";
                Debug.Log("Loading game from RAM");
            }
            else if ((operation & OperationMode.DISK_OP) != 0)
            {
                if (!File.Exists(saveFilePath))
                {
                    Debug.LogWarning("Save file not found. Starting new game.");
                    return;
                }

                string jsonData = File.ReadAllText(saveFilePath);
                saveData = JsonUtility.FromJson<GameSaveData>(jsonData);
                loadSource = "Disk";
                Debug.Log("Loading game from disk");
            }

            if (saveData == null)
            {
                Debug.LogError("No save data available for the requested operation");
                return;
            }

            Debug.Log("=== LOAD COMPONENTS ===");
            Debug.Log($"Load Source: {loadSource}");
            Debug.Log($"Components to load: {components}");
            if ((components & LoadComponents.LOADBIT_PLAYER_INVENTORY) != 0) Debug.Log("- Player Inventory");
            if ((components & LoadComponents.LOADBIT_GAME_MANAGER) != 0) Debug.Log("- Game Manager Data");
            if ((components & LoadComponents.LOADBIT_SAVETIMESTAMP) != 0) Debug.Log("- Save Timestamp");
            if ((components & LoadComponents.LOADBIT_SAVECOUNT) != 0) Debug.Log("- Save Count");
            Debug.Log("=== END LOAD COMPONENTS ===");

            if ((components & LoadComponents.LOADBIT_SAVECOUNT) != 0)
            {
                saveCount = saveData.saveCount;
                Debug.Log($"Loaded save count: {saveCount}");
            }

            StartCoroutine(LoadGameDelayed(saveData, loadSource, components));

            Debug.Log($"Game loaded from {loadSource} successfully!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load game: {e.Message}");
        }
    }

    private System.Collections.IEnumerator LoadGameDelayed(GameSaveData saveData, string loadSource, LoadComponents components)
    {
        if ((components & LoadComponents.LOADBIT_PLAYER_INVENTORY) != 0)
        {
            while (UnitManager.Instance?.playerInventory == null)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        Debug.Log($"Applying save data from {loadSource}...");

        if ((components & LoadComponents.LOADBIT_PLAYER_INVENTORY) != 0)
        {
            LoadPlayerInventory(saveData.playerInventory);
            Debug.Log("Player inventory loaded successfully");
        }

        if ((components & LoadComponents.LOADBIT_GAME_MANAGER) != 0)
        {
            LoadGameManagerData(saveData.gameManagerData);
            Debug.Log("Game manager data loaded successfully");
        }

        if ((components & LoadComponents.LOADBIT_SAVETIMESTAMP) != 0)
        {
            System.DateTime saveTime = System.DateTime.FromBinary(saveData.saveTimestamp);
            Debug.Log($"Save timestamp loaded: {saveTime:yyyy-MM-dd HH:mm:ss}");
        }
    }

    private PlayerInventoryData SavePlayerInventory()
    {
        PlayerInventory inventory = UnitManager.Instance.playerInventory;
        PlayerInventoryData inventoryData = new PlayerInventoryData();

        // Save equipped weapon
        if (inventory.EquippedWeapon?.itemInstance?.item != null)
        {
            inventoryData.equippedWeapon = SaveItemInstanceData(inventory.EquippedWeapon.itemInstance);
        }

        // Save equipped relics
        inventoryData.equippedRelics = new List<ItemInstanceSaveData>();
        for (int i = 0; i < inventory.EquippedRelics.Length; i++)
        {
            if (inventory.EquippedRelics[i]?.itemInstance?.item != null)
            {
                ItemInstanceSaveData relicData = SaveItemInstanceData(inventory.EquippedRelics[i].itemInstance);
                relicData.slotIndex = i;
                inventoryData.equippedRelics.Add(relicData);
            }
        }

        // Save inventory slots
        inventoryData.inventorySlots = new List<ItemInstanceSaveData>();
        for (int i = 0; i < inventory.InventoryItems.Length; i++)
        {
            if (inventory.InventoryItems[i]?.itemInstance?.item != null)
            {
                ItemInstanceSaveData slotData = SaveItemInstanceData(inventory.InventoryItems[i].itemInstance);
                slotData.slotIndex = i;
                inventoryData.inventorySlots.Add(slotData);
            }
        }

        return inventoryData;
    }

    private ItemInstanceSaveData SaveItemInstanceData(ItemInstance itemInstance)
    {
        ItemInstanceSaveData saveData = new ItemInstanceSaveData();

        if (itemInstance?.item != null)
        {
            saveData.itemID = itemInstance.item.itemID;
            saveData.itemData = SaveItemInstanceDataDetails(itemInstance.itemData);
        }

        return saveData;
    }

    private ItemInstanceDataSave SaveItemInstanceDataDetails(ItemInstanceData itemData)
    {
        if (itemData == null) return null;

        ItemInstanceDataSave itemDataSave = new ItemInstanceDataSave();
        itemDataSave.level = itemData.level;

        itemDataSave.modSlots = new List<ModSlotSave>();
        foreach (ItemInstanceData.ModSlot modSlot in itemData.ModSlots)
        {
            ModSlotSave modSlotSave = new ModSlotSave();
            modSlotSave.modSlotType = (int)modSlot.modSlotType;
            modSlotSave.modItemID = modSlot.modItem?.itemID ?? "";
            itemDataSave.modSlots.Add(modSlotSave);
        }

        return itemDataSave;
    }

    private GameManagerData SaveGameManagerData()
    {
        GameManagerData data = new GameManagerData();

        if (GameManager.Instance != null)
        {
            data.selectedPlayerClass = GameManager.Instance.selectedPlayerClass;
            data.currentResurrections = GameManager.Instance.currentResurrections;
            data.maxResurrections = GameManager.Instance.maxResurrections;
        }

        return data;
    }

    private void LoadPlayerInventory(PlayerInventoryData inventoryData)
    {
        if (inventoryData == null) return;

        PlayerInventory inventory = UnitManager.Instance.playerInventory;

        ClearInventory(inventory);

        // Load equipped weapon
        if (inventoryData.equippedWeapon != null)
        {
            LoadItemToEquippedSlot(inventoryData.equippedWeapon, inventory.EquippedWeapon);
        }

        // Load equipped relics
        if (inventoryData.equippedRelics != null)
        {
            foreach (ItemInstanceSaveData relicData in inventoryData.equippedRelics)
            {
                if (relicData.slotIndex >= 0 && relicData.slotIndex < inventory.EquippedRelics.Length)
                {
                    LoadItemToEquippedSlot(relicData, inventory.EquippedRelics[relicData.slotIndex]);
                }
            }
        }

        // Load inventory slots
        if (inventoryData.inventorySlots != null)
        {
            foreach (ItemInstanceSaveData slotData in inventoryData.inventorySlots)
            {
                if (slotData.slotIndex >= 0 && slotData.slotIndex < inventory.InventoryItems.Length)
                {
                    LoadItemToInventorySlot(slotData, inventory.InventoryItems[slotData.slotIndex]);
                }
            }
        }

        inventory.RenderItems();
    }

    private void LoadItemToEquippedSlot(ItemInstanceSaveData saveData, PlayerInventory.ItemSlot slot)
    {
        if (saveData == null || string.IsNullOrEmpty(saveData.itemID)) return;

        Item item = FindItemByID(saveData.itemID);
        if (item == null)
        {
            Debug.LogWarning($"Item with ID '{saveData.itemID}' not found. Skipping.");
            return;
        }

        ItemInstanceData itemData = LoadItemInstanceDataDetails(saveData.itemData);

        slot.itemInstance = new ItemInstance
        {
            item = item,
            itemData = itemData ?? new ItemInstanceData()
        };
    }

    private void LoadItemToInventorySlot(ItemInstanceSaveData saveData, PlayerInventory.ItemSlot slot)
    {
        if (saveData == null || string.IsNullOrEmpty(saveData.itemID)) return;

        Item item = FindItemByID(saveData.itemID);
        if (item == null)
        {
            Debug.LogWarning($"Item with ID '{saveData.itemID}' not found. Skipping.");
            return;
        }

        ItemInstanceData itemData = LoadItemInstanceDataDetails(saveData.itemData);

        slot.itemInstance = new ItemInstance
        {
            item = item,
            itemData = itemData ?? new ItemInstanceData()
        };
    }

    private ItemInstanceData LoadItemInstanceDataDetails(ItemInstanceDataSave itemDataSave)
    {
        if (itemDataSave == null) return new ItemInstanceData();

        ItemInstanceData itemData = new ItemInstanceData();
        itemData.level = itemDataSave.level;

        if (itemDataSave.modSlots != null)
        {
            foreach (ModSlotSave modSlotSave in itemDataSave.modSlots)
            {
                ItemInstanceData.ModSlot modSlot = new ItemInstanceData.ModSlot();
                modSlot.modSlotType = (Item.ModType)modSlotSave.modSlotType;

                if (!string.IsNullOrEmpty(modSlotSave.modItemID))
                {
                    modSlot.modItem = FindItemByID(modSlotSave.modItemID);
                }

                itemData.ModSlots.Add(modSlot);
            }
        }

        return itemData;
    }

    private void LoadGameManagerData(GameManagerData data)
    {
        if (data == null || GameManager.Instance == null) return;

        GameManager.Instance.selectedPlayerClass = data.selectedPlayerClass;
        
        // Load resurrection data if available (for backwards compatibility)
        if (data.currentResurrections > 0 || data.maxResurrections > 0)
        {
            GameManager.Instance.currentResurrections = data.currentResurrections;
            GameManager.Instance.maxResurrections = data.maxResurrections;
            Debug.Log($"Loaded resurrection data: {data.currentResurrections}/{data.maxResurrections}");
        }
    }

    private void ClearInventory(PlayerInventory inventory)
    {
        // Clear equipped weapon
        if (inventory.EquippedWeapon != null)
        {
            inventory.EquippedWeapon.itemInstance = new ItemInstance();
        }

        // Clear equipped relics
        for (int i = 0; i < inventory.EquippedRelics.Length; i++)
        {
            if (inventory.EquippedRelics[i] != null)
            {
                inventory.EquippedRelics[i].itemInstance = new ItemInstance();
            }
        }

        // Clear inventory slots
        for (int i = 0; i < inventory.InventoryItems.Length; i++)
        {
            if (inventory.InventoryItems[i] != null)
            {
                inventory.InventoryItems[i].itemInstance = new ItemInstance();
            }
        }

        // Clear held item
        inventory.heldItem = null;
    }

    private Item FindItemByID(string itemID)
    {
        Item[] allItems = Resources.LoadAll<Item>("");

        foreach (Item item in allItems)
        {
            if (item.itemID == itemID)
            {
                return item;
            }
        }

        Debug.LogWarning($"Item with ID '{itemID}' not found in Resources.");
        return null;
    }

    public bool SaveExists(OperationMode operation = OperationMode.DISK_OP)
    {
        bool diskExists = File.Exists(saveFilePath);
        bool ramExists = ramSaveData != null;

        if ((operation & OperationMode.DISK_OP) != 0 && (operation & OperationMode.RAM_OP) != 0)
        {
            return diskExists || ramExists;
        }
        else if ((operation & OperationMode.RAM_OP) != 0)
        {
            return ramExists;
        }
        else
        {
            return diskExists;
        }
    }

    public void DeleteSave(OperationMode operation = OperationMode.DISK_OP)
    {
        Debug.Log($"Trying to delete save with operation: {operation}");
        try
        {
            if ((operation & OperationMode.RAM_OP) != 0)
            {
                ramSaveData = null;
                Debug.Log("RAM save data cleared successfully!");
            }

            if ((operation & OperationMode.DISK_OP) != 0)
            {
                if (File.Exists(saveFilePath))
                {
                    File.Delete(saveFilePath);
                    Debug.Log("Save file deleted successfully!");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to delete save: {e.Message}");
        }
    }

    public string GetSaveInfo(OperationMode operation = OperationMode.DISK_OP)
    {
        string result = "";

        if ((operation & OperationMode.RAM_OP) != 0)
        {
            if (ramSaveData != null)
            {
                System.DateTime ramSaveTime = System.DateTime.FromBinary(ramSaveData.saveTimestamp);
                result += $"RAM Save - Date: {ramSaveTime:yyyy-MM-dd HH:mm:ss}, Count: {ramSaveData.saveCount}";
            }
            else
            {
                result += "RAM Save: No data found";
            }
        }

        if ((operation & OperationMode.DISK_OP) != 0)
        {
            if ((operation & OperationMode.RAM_OP) != 0 && !string.IsNullOrEmpty(result))
            {
                result += "\n";
            }

            if (!File.Exists(saveFilePath))
            {
                result += "Disk Save: No file found";
            }
            else
            {
                try
                {
                    FileInfo fileInfo = new FileInfo(saveFilePath);
                    string jsonData = File.ReadAllText(saveFilePath);
                    GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(jsonData);

                    System.DateTime saveTime = System.DateTime.FromBinary(saveData.saveTimestamp);
                    result += $"Disk Save - Date: {saveTime:yyyy-MM-dd HH:mm:ss}, Size: {fileInfo.Length} bytes, Count: {saveData.saveCount}";
                }
                catch (System.Exception e)
                {
                    result += $"Disk Save Error: {e.Message}";
                }
            }
        }

        return string.IsNullOrEmpty(result) ? "No save information available" : result;
    }
}

[System.Serializable]
public class GameSaveData
{
    public PlayerInventoryData playerInventory;
    public GameManagerData gameManagerData;
    public long saveTimestamp;
    public int saveCount;
}

[System.Serializable]
public class PlayerInventoryData
{
    public ItemInstanceSaveData equippedWeapon;
    public List<ItemInstanceSaveData> equippedRelics;
    public List<ItemInstanceSaveData> inventorySlots;
}

[System.Serializable]
public class ItemInstanceSaveData
{
    public string itemID;
    public ItemInstanceDataSave itemData;
    public int slotIndex = -1; // Used for inventory slots
}

[System.Serializable]
public class ItemInstanceDataSave
{
    public int level;
    public List<ModSlotSave> modSlots;
}

[System.Serializable]
public class ModSlotSave
{
    public int modSlotType;
    public string modItemID;
}

[System.Serializable]
public class GameManagerData
{
    public string selectedPlayerClass;
    public int currentResurrections;
    public int maxResurrections;
}
