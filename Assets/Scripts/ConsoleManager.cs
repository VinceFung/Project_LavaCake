using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConsoleManager : MonoBehaviour
{
    public GameObject consoleObject;
    public TMP_InputField inputField;
    public TextMeshProUGUI outputText;

    private Dictionary<string, Action<String[]>> commands = new Dictionary<string, Action<String[]>>();

    private void Start()
    {
        RegisterCommand("spawnItem", SpawnItemPickUp);
        RegisterCommand("give", Give);
        RegisterCommand("regenerate", RegenerateWorld);
        RegisterCommand("savegame", CallSaveGame);
        RegisterCommand("loadgame", CallLoadGame);
        RegisterCommand("saveinfo", CallSaveInfo);
        RegisterCommand("deletesave", CallDeleteSave);
        RegisterCommand("tptotp", TeleportToTeleporter);
        RegisterCommand("settimescale", SetTimeScale);
    }

    public void RegisterCommand(string commandName, Action<String[]> action)
    {
        if (!commands.ContainsKey(commandName))
        {
            commands.Add(commandName, action);
        }
    }

    public void ExecuteCommand(string input)
    {
        string[] parts = input.Split(' ');
        if(parts.Length == 0)
        {
            AppendOutput("Invalid Command");
            return;
        }

        string commandName = parts[0];
        string[] args = new string[parts.Length - 1];
        Array.Copy(parts, 1, args, 0, args.Length);

        if(commands.TryGetValue(commandName, out Action<string[]> command))
        {
            command(args);
        }
        else
        {
            AppendOutput("Command not found: " + commandName);
        }
    }

    void SpawnItemPickUp(string[] args)
    {
        if(args.Length != 1)
        {
            AppendOutput("Usage: spawnItem <path>");
            return;
        }

        Item spawnItem = Resources.Load($"items/{args[0]}") as Item;
        if (spawnItem != null)
        {
            GameManager.Instance.SpawnItemPickUp(spawnItem, UnitManager.Instance.playerObj.transform.position);
            AppendOutput("Spawned " + args[0]);
        }
        else
        {
            AppendOutput($"resources/items/{args[0]} not found");
        }
    }

    void RegenerateWorld(string[] args)
    {
        WorldGenerator generator = FindFirstObjectByType<WorldGenerator>();
        if (generator != null)
        {
            generator.RegenerateWorldAndTeleportPlayer();
            AppendOutput("World regenerated using WorldGenerator");
        }
        else
        {
            // Fallback to LevelGenerator if WorldGenerator is not found
            LevelGenerator legacyGenerator = FindFirstObjectByType<LevelGenerator>();
            if (legacyGenerator != null)
            {
                legacyGenerator.playerTeleported = false;
                legacyGenerator.GeneratePathNodes();
                AppendOutput("World regenerated using LevelGenerator (fallback)");
            }
            else
            {
                AppendOutput("No world generator found!");
            }
        }
    }

    void CallSaveGame(string[] args)
    {
        if (GameSaveLoad.Instance != null)
        {
            GameSaveLoad.Instance.SaveGame();
            AppendOutput("Game saved successfully!");
        }
        else
        {
            AppendOutput("Error: GameSaveLoad instance not found!");
        }
    }

    void CallLoadGame(string[] args)
    {
        if (GameSaveLoad.Instance != null)
        {
            GameSaveLoad.Instance.LoadGame();
            AppendOutput("Game loaded successfully!");
        }
        else
        {
            AppendOutput("Error: GameSaveLoad instance not found!");
        }
    }

    void CallSaveInfo(string[] args)
    {
        if (GameSaveLoad.Instance != null)
        {
            string info = GameSaveLoad.Instance.GetSaveInfo(GameSaveLoad.OperationMode.DISK_OP | GameSaveLoad.OperationMode.RAM_OP);
            AppendOutput($"Save Info:\n{info}");
        }
        else
        {
            AppendOutput("Error: GameSaveLoad instance not found!");
        }
    }

    void CallDeleteSave(string[] args)
    {
        if (GameSaveLoad.Instance != null)
        {
            GameSaveLoad.Instance.DeleteSave();
            AppendOutput("Save deleted successfully!");
        }
        else
        {
            AppendOutput("Error: GameSaveLoad instance not found!");
        }
    }

    void TeleportToTeleporter(string[] args)
    {
        UnitManager.Instance.playerObj.transform.position = UnitManager.Instance.teleporterTpPoint.position;
    }

    void SetTimeScale(string[] args)
    {
        if (args.Length != 1)
        {
            AppendOutput("Please provide a time scale value.");
            return;
        }
        
        if (float.TryParse(args[0], out float timeScale))
        {
            Time.timeScale = timeScale;
            AppendOutput($"Time scale set to {timeScale}");
        }
        else
        {
            AppendOutput("Invalid time scale value. Please provide a valid number.");
        }
    }

    void Give(string[] args)
    {
        if (args.Length != 1)
        {
            AppendOutput("Usage: give <path>");
            return;
        }

        Item spawnItem = Resources.Load($"items/{args[0]}") as Item;
        if (spawnItem != null)
        {
            PlayerInventory playerInventory = UnitManager.Instance.playerInventory;
            if (playerInventory == null)
            {
                AppendOutput("Player inventory not found");
                return;
            }

            ItemInstance itemInstance = new ItemInstance();
            itemInstance.item = spawnItem;
            itemInstance.itemData = new ItemInstanceData();

            bool itemAdded = false;

            for (int i = 0; i < playerInventory.InventoryItems.Length; i++)
            {
                PlayerInventory.ItemSlot slot = playerInventory.InventoryItems[i];
                if (!itemAdded && (slot.itemInstance == null || slot.itemInstance.item == null))
                {
                    if (slot.slotType == Item.ItemTypes.Any || slot.slotType == spawnItem.itemType)
                    {
                        itemAdded = true;
                        GameManager.Instance.AddItemToSlot(itemInstance, slot, playerInventory);
                        AppendOutput("Gave Player " + args[0]);
                        break;
                    }
                }
            }

            if (!itemAdded)
            {
                if (spawnItem.itemType == Item.ItemTypes.Weapon && 
                    (playerInventory.EquippedWeapon.itemInstance == null || playerInventory.EquippedWeapon.itemInstance.item == null))
                {
                    GameManager.Instance.AddItemToSlot(itemInstance, playerInventory.EquippedWeapon, playerInventory);
                    AppendOutput("Equipped " + args[0] + " as weapon");
                    itemAdded = true;
                }
                else if (spawnItem.itemType == Item.ItemTypes.Relic)
                {
                    foreach (PlayerInventory.ItemSlot relicSlot in playerInventory.EquippedRelics)
                    {
                        if (relicSlot.itemInstance == null || relicSlot.itemInstance.item == null)
                        {
                            GameManager.Instance.AddItemToSlot(itemInstance, relicSlot, playerInventory);
                            AppendOutput("Equipped " + args[0] + " as relic");
                            itemAdded = true;
                            break;
                        }
                    }
                }
            }

            if (!itemAdded)
            {
                GameManager.Instance.SpawnItemPickUp(spawnItem, UnitManager.Instance.playerObj.transform.position);
                AppendOutput("Inventory full - Spawned " + args[0]);
            }
        }
        else
        {
            AppendOutput($"resources/items/{args[0]} not found");
        }
    }

    private void AppendOutput(string message)
    {
        outputText.text += message + "\n";
    }

    void OnSubmit()
    {
        string input = inputField.text;
        ExecuteCommand(input);
        inputField.text = "";
        inputField.ActivateInputField();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            consoleObject.SetActive(!consoleObject.activeSelf);
        }

        if (Input.GetKeyUp(KeyCode.Return))
        {
            OnSubmit();
            inputField.text = "";
        }
    }
}