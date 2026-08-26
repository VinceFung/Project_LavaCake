using UnityEngine;
using UnityEditor;
using System.IO;

public class ImpaledEditorTools : EditorWindow
{
    /*[MenuItem("Impaled/Save & Preferences Tools")]
    public static void ShowWindow()
    {
        GetWindow<ImpaledEditorTools>("Impaled Tools");
    }

    [MenuItem("Impaled/Quick Actions/Delete Save File")]
    public static void QuickDeleteSaveFile()
    {
        if (EditorUtility.DisplayDialog("Delete Save File", 
            "Are you sure you want to delete the game save file?\n\nThis action cannot be undone.", 
            "Delete", "Cancel"))
        {
            DeleteSaveFile();
        }
    }

    [MenuItem("Impaled/Quick Actions/Clear Player Preferences")]
    public static void QuickClearPlayerPrefs()
    {
        if (EditorUtility.DisplayDialog("Clear Player Preferences", 
            "Are you sure you want to clear all Player Preferences?\n\nThis action cannot be undone.", 
            "Clear", "Cancel"))
        {
            ClearPlayerPreferences();
        }
    }

    [MenuItem("Impaled/Quick Actions/Delete All Game Data")]
    public static void QuickDeleteAllData()
    {
        if (EditorUtility.DisplayDialog("Delete All Game Data", 
            "Are you sure you want to delete:\n• Save Files\n• Player Preferences\n• RAM Save Data\n\nThis action cannot be undone.", 
            "Delete All", "Cancel"))
        {
            DeleteAllGameData();
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Impaled Development Tools", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Save File Management Section
        GUILayout.Label("Save File Management", EditorStyles.label);
        EditorGUILayout.BeginVertical("box");

        // Display current save file info
        DisplaySaveFileInfo();
        
        EditorGUILayout.Space();

        // Delete save file button
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Delete Save File", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Confirm Delete", 
                "Delete the game save file?", "Delete", "Cancel"))
            {
                DeleteSaveFile();
            }
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

        // Player Preferences Section
        GUILayout.Label("Player Preferences", EditorStyles.label);
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.HelpBox("Player Preferences store settings like volume, graphics options, etc.", MessageType.Info);
        
        EditorGUILayout.Space();

        // Clear player prefs button
        GUI.backgroundColor = Color.yellow;
        if (GUILayout.Button("Clear Player Preferences", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Confirm Clear", 
                "Clear all Player Preferences?", "Clear", "Cancel"))
            {
                ClearPlayerPreferences();
            }
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

        // RAM Data Section
        GUILayout.Label("RAM Save Data", EditorStyles.label);
        EditorGUILayout.BeginVertical("box");

        DisplayRAMSaveInfo();
        
        EditorGUILayout.Space();

        // Clear RAM data button
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Clear RAM Save Data", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Confirm Clear", 
                "Clear RAM save data? (Only available during play mode)", "Clear", "Cancel"))
            {
                ClearRAMSaveData();
            }
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

        // Danger Zone
        GUILayout.Label("Danger Zone", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        
        EditorGUILayout.HelpBox("This will delete ALL game data including save files, preferences, and RAM data.", MessageType.Warning);
        
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("DELETE ALL GAME DATA", GUILayout.Height(40)))
        {
            if (EditorUtility.DisplayDialog("CONFIRM DELETE ALL", 
                "This will permanently delete:\n• Save Files\n• Player Preferences\n• RAM Save Data\n\nThis action cannot be undone!\n\nAre you absolutely sure?", 
                "DELETE ALL", "Cancel"))
            {
                DeleteAllGameData();
            }
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // Refresh button
        if (GUILayout.Button("Refresh Info"))
        {
            Repaint();
        }
    }

    private void DisplaySaveFileInfo()
    {
        string saveFilePath = Path.Combine(Application.persistentDataPath, "GameSave.json");
        
        if (File.Exists(saveFilePath))
        {
            try
            {
                FileInfo fileInfo = new FileInfo(saveFilePath);
                EditorGUILayout.LabelField("Save File Status:", "Found");
                EditorGUILayout.LabelField("File Size:", $"{fileInfo.Length} bytes");
                EditorGUILayout.LabelField("Last Modified:", fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"));
                EditorGUILayout.LabelField("Path:", saveFilePath);
                
                // Try to read save count from file
                try
                {
                    string jsonData = File.ReadAllText(saveFilePath);
                    var saveData = JsonUtility.FromJson<GameSaveData>(jsonData);
                    if (saveData != null)
                    {
                        EditorGUILayout.LabelField("Save Count:", saveData.saveCount.ToString());
                        System.DateTime saveTime = System.DateTime.FromBinary(saveData.saveTimestamp);
                        EditorGUILayout.LabelField("Save Time:", saveTime.ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                }
                catch
                {
                    EditorGUILayout.LabelField("Save Data:", "Could not parse");
                }
            }
            catch (System.Exception e)
            {
                EditorGUILayout.LabelField("Error:", e.Message);
            }
        }
        else
        {
            EditorGUILayout.LabelField("Save File Status:", "Not Found");
            EditorGUILayout.LabelField("Expected Path:", saveFilePath);
        }
    }

    private void DisplayRAMSaveInfo()
    {
        if (Application.isPlaying && GameSaveLoad.Instance != null)
        {
            bool ramDataExists = GameSaveLoad.Instance.SaveExists(GameSaveLoad.OperationMode.RAM_OP);
            EditorGUILayout.LabelField("RAM Save Status:", ramDataExists ? "Data Found" : "No Data");
            
            if (ramDataExists)
            {
                string ramInfo = GameSaveLoad.Instance.GetSaveInfo(GameSaveLoad.OperationMode.RAM_OP);
                EditorGUILayout.LabelField("RAM Info:", ramInfo);
            }
        }
        else
        {
            EditorGUILayout.LabelField("RAM Save Status:", "Not Available (Play Mode Required)");
        }
    }

    private static void DeleteSaveFile()
    {
        try
        {
            if (Application.isPlaying && GameSaveLoad.Instance != null)
            {
                // Use the GameSaveLoad system if available
                GameSaveLoad.Instance.DeleteSave(GameSaveLoad.OperationMode.DISK_OP);
                Debug.Log("Save file deleted via GameSaveLoad system");
            }
            else
            {
                // Manually delete the file
                string saveFilePath = Path.Combine(Application.persistentDataPath, "GameSave.json");
                if (File.Exists(saveFilePath))
                {
                    File.Delete(saveFilePath);
                    Debug.Log($"Save file deleted manually: {saveFilePath}");
                }
                else
                {
                    Debug.Log("No save file found to delete");
                }
            }

            EditorUtility.DisplayDialog("Success", "Save file deleted successfully!", "OK");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to delete save file: {e.Message}");
            EditorUtility.DisplayDialog("Error", $"Failed to delete save file:\n{e.Message}", "OK");
        }
    }

    private static void ClearPlayerPreferences()
    {
        try
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("Player Preferences cleared successfully");
            EditorUtility.DisplayDialog("Success", "Player Preferences cleared successfully!", "OK");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to clear Player Preferences: {e.Message}");
            EditorUtility.DisplayDialog("Error", $"Failed to clear Player Preferences:\n{e.Message}", "OK");
        }
    }

    private static void ClearRAMSaveData()
    {
        try
        {
            if (Application.isPlaying && GameSaveLoad.Instance != null)
            {
                GameSaveLoad.Instance.DeleteSave(GameSaveLoad.OperationMode.RAM_OP);
                Debug.Log("RAM save data cleared successfully");
                EditorUtility.DisplayDialog("Success", "RAM save data cleared successfully!", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Warning", "RAM save data can only be cleared during play mode when GameSaveLoad is active.", "OK");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to clear RAM save data: {e.Message}");
            EditorUtility.DisplayDialog("Error", $"Failed to clear RAM save data:\n{e.Message}", "OK");
        }
    }

    private static void DeleteAllGameData()
    {
        try
        {
            // Delete save file
            DeleteSaveFile();
            
            // Clear player preferences
            ClearPlayerPreferences();
            
            // Clear RAM data if available
            if (Application.isPlaying && GameSaveLoad.Instance != null)
            {
                GameSaveLoad.Instance.DeleteSave(GameSaveLoad.OperationMode.RAM_OP);
            }
            
            Debug.Log("All game data deleted successfully");
            EditorUtility.DisplayDialog("Success", "All game data deleted successfully!\n• Save Files\n• Player Preferences\n• RAM Save Data", "OK");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to delete all game data: {e.Message}");
            EditorUtility.DisplayDialog("Error", $"Failed to delete all game data:\n{e.Message}", "OK");
        }
    }

    // Auto-refresh when window gains focus
    private void OnFocus()
    {
        Repaint();
    }*/
}
