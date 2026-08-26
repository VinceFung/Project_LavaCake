using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.IO;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance;
    public Animator SceneTransitionAnimator;

    public bool loadInventoryOnSpawn;
    public bool spawnPlayer = true;

    public Camera mainCamera;
    public AnchorTransform cameraAnchor;

    public GameObject playerObj;
    public Entity playerEntity;
    public PlayerLevel playerLevel;
    public PlayerInventory playerInventory;

    [Header("Debug Teleport Points")]
    public Transform teleporterTpPoint;

    [Header("UI")]
    public EventSystem eventSystem;
    public GameObject textBoxObj;
    public TextMeshProUGUI textBoxText;
    public TextMeshProUGUI playerHealthPotionCount;
    public Transform playerAmmoDisplayParent;
    public Transform BuffBar;
    public GameObject map;
    public GameObject pauseMenu;
    public Slider playerInstantHealthBar;
    public Slider playerDelayedHealthBar;
    public Slider playerInstantStaminaBar;
    public Slider playerDelayedStaminaBar;
    public Slider playerInstantXpBar;
    public Slider playerDelayedXpBar;
    public TextMeshProUGUI playerLevelText;
    public GameObject consoleObject;

    public TextMeshProUGUI ResurrectionTokenCountText;

    public GameObject DeathScreen;
    public GameObject ResurrectScreen;
    public DebuffPreset ressurectionDebuff;
    public HitEffectAnimation playerHitEffect;

    [Header("Scene Progression")]
    public string[] saveCountScenes;

    [Header("UI References")]
    public Button loadSceneBySaveCountButton;
    public TextMeshProUGUI continueButtonText;

    [Header("Input Actions")]
    public InputActionReference mapAction;
    public InputActionReference pauseAction;

    [Header("Boss Arena Management")]
    public bool isPlayerInBossArena = false;
    public Vector3 bossArenaExitPosition = Vector3.zero;
    public BossArena currentBossArena = null;

    bool PlayerSpawned = false;
    bool inventoryLoaded = false;

    public string trackOnLoad = "";

    public int ChestSpawnFactorRequirement = 3;
    public int ChestSpawnFactor;

    Vector3 playerPos;

    private void Start()
    {
        if (trackOnLoad != "" && MusicManager.Instance.GetCurrentTrack() != MusicManager.Instance.musicLibrary.GetClipFromName(trackOnLoad))
        {
            ChangeMusic(trackOnLoad);
        }

        // Disable the button if no save file or save count is 0
        if (loadSceneBySaveCountButton != null)
        {
            bool saveExists = GameSaveLoad.Instance != null && GameSaveLoad.Instance.SaveExists();
            if (saveExists && GameSaveLoad.Instance != null)
            {
                GameSaveLoad.Instance.LoadGame(GameSaveLoad.OperationMode.DISK_OP, GameSaveLoad.LoadComponents.LOADBIT_SAVECOUNT);
            }
            bool enableButton = saveExists && GameSaveLoad.Instance != null && GameSaveLoad.Instance.saveCount > 0;
            loadSceneBySaveCountButton.interactable = enableButton;

            if (continueButtonText != null)
            {
                continueButtonText.color = enableButton ? Color.white : Color.gray;
            }
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        if (mapAction != null)
        {
            mapAction.action.Enable();
            mapAction.action.performed += OnMapToggle;
        }
        if (pauseAction != null)
        {
            pauseAction.action.Enable();
            pauseAction.action.performed += OnPauseToggle;
        }
    }

    private void OnDisable()
    {
        if (mapAction != null)
        {
            mapAction.action.performed -= OnMapToggle;
            mapAction.action.Disable();
        }
        if (pauseAction != null)
        {
            pauseAction.action.performed -= OnPauseToggle;
            pauseAction.action.Disable();
        }
    }

    private void OnDestroy()
    {
        SaveInventoryToRAM();
    }

    private void SaveInventoryToRAM()
    {
        if (playerInventory != null && GameSaveLoad.Instance != null)
        {
            GameSaveLoad.Instance.SaveGame(GameSaveLoad.OperationMode.RAM_OP);
            Debug.Log("Inventory saved to RAM on destroy");
        }
    }

    private void Update()
    {
        if (PlayerSpawned && loadInventoryOnSpawn)
        {
            if (!inventoryLoaded)
            {
                CallLoadGame();
                inventoryLoaded = true;
            }
        }

        if (spawnPlayer)
        {
            if (playerObj == null)
            {
                if (PlayerSpawned)
                {
                    return;
                }

                playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    SetupExistingPlayer();
                }
                else
                {
                    SpawnPlayer(new Vector3(0f, 1f, 0f), false);
                }
            }
            else
            {
                cameraAnchor.AnchorTo = playerObj.transform;
            }

            SetupPlayerUI();
        }

        if (playerEntity != null)
        {
            if (playerEntity.charMovement.IsGrounded) playerPos = playerEntity.transform.position;
        }

        if (ResurrectionTokenCountText != null)
        {
            ResurrectionTokenCountText.text = GameManager.Instance.currentResurrections.ToString();
        }

        if (playerLevelText != null && playerLevel != null)
        {
            playerLevelText.text = $"Level: {playerLevel.Level}";
        }

        if (Input.GetButtonDown("Fire1"))
        {
            textBoxObj.SetActive(false);
        }
    }

    private void SpawnPlayer(Vector3 position, bool isResurrection)
    {
        playerObj = Instantiate(Resources.Load(GameManager.Instance.selectedPlayerClass) as GameObject, position, Quaternion.identity);

        if (playerObj != null)
        {
            SetupPlayerComponents();
            SetupPlayerUI();

            if (isResurrection)
            {
                playerEntity.ApplyDebuff(ressurectionDebuff, playerEntity);
                if (DeathScreen != null)
                    DeathScreen.SetActive(false);

                if (playerInventory != null && GameSaveLoad.Instance != null)
                {
                    GameSaveLoad.Instance.LoadGame(GameSaveLoad.OperationMode.RAM_OP, GameSaveLoad.LoadComponents.LOADBIT_PLAYER_INVENTORY | GameSaveLoad.LoadComponents.LOADBIT_GAME_MANAGER);
                }
            }

            PlayerSpawned = true;
        }
        else
        {
            string context = isResurrection ? "resurrect" : "spawn";
            Debug.LogError($"Failed to {context} player: Could not instantiate player prefab.");
        }
    }

    private void SetupExistingPlayer()
    {
        playerInventory = playerObj.GetComponent<PlayerInventory>();
        playerEntity = playerObj.GetComponent<Entity>();
        playerLevel = playerObj.GetComponent<PlayerLevel>();

        playerEntity.OnDeath.AddListener(ActivateDeathScreen);
        playerEntity.OnDeath.AddListener(CallDeleteSave);
        playerEntity.OnDamageTaken.AddListener(playerHitEffect.PlayAnimation);

        PlayerSpawned = true;
    }

    private void SetupPlayerComponents()
    {
        playerInventory = playerObj.GetComponent<PlayerInventory>();
        playerEntity = playerObj.GetComponent<Entity>();
        playerLevel = playerObj.GetComponent<PlayerLevel>();

        playerEntity.OnDeath.AddListener(ActivateDeathScreen);
        playerEntity.OnDeath.AddListener(CallDeleteSave);
        playerEntity.OnDamageTaken.AddListener(playerHitEffect.PlayAnimation);
    }

    private void SetupPlayerUI()
    {
        if (playerEntity != null)
        {
            playerEntity.InstantHealthBar = playerInstantHealthBar;
            playerEntity.DelayedHealthBar = playerDelayedHealthBar;
            playerEntity.InstantStaminaBar = playerInstantStaminaBar;
            playerEntity.DelayedStaminaBar = playerDelayedStaminaBar;
            playerEntity.entityGun.AmmoDisplayParent = playerAmmoDisplayParent;
            playerEntity.BuffBar = BuffBar;
        }

        if (playerLevel != null)
        {
            playerLevel.XpSlider = playerInstantXpBar;
            playerLevel.DelayedXpSlider = playerDelayedXpBar;
        }
    }

    void ActivateDeathScreen()
    {
        SaveInventoryToRAM();
        DeathScreen.SetActive(true);

        if (!GameManager.Instance.CanResurrect())
        {
            Debug.Log("No resurrections remaining. Game Over.");
        }
        else
        {
            Debug.Log($"Resurrections remaining: {GameManager.Instance.GetCurrentResurrections()}");
        }
    }

    public void RefillPlayerPotions()
    {
        PlayerHealthPotion potionScript = playerObj.GetComponent<PlayerHealthPotion>();
        potionScript.PotionCount = potionScript.PotionMax;
    }

    public void RefillPlayerHealth()
    {
        if (playerEntity != null)
        {
            playerEntity.Health = playerEntity.MaxHealth;
            playerEntity.staggerDamageTaken = 0f;
            playerEntity.severenceDamageTaken = 0f;
        }
    }

    public void ChangeMusic(string trackName)
    {
        MusicManager.Instance.PlayMusic(trackName, 0.5f);
    }

    public void StartRunWithSelectedCharacter(string characterClassName)
    {
        GameManager.Instance.selectedPlayerClass = $"player_classes/{characterClassName}";
        GameManager.Instance.ResetResurrections();
        StartCoroutine(LoadSceneDelayed("World1"));
    }

    public void LoadSceneByName(string sceneName)
    {
        StartCoroutine(LoadSceneDelayed(sceneName));
    }

    public void LoadSceneBySaveCount()
    {
        if (GameSaveLoad.Instance == null)
        {
            Debug.LogWarning("GameSaveLoad instance not found. Scene will not be loaded.");
            return;
        }

        int count = GameSaveLoad.Instance.saveCount;

        if (!GameSaveLoad.Instance.SaveExists() || count == 0)
        {
            Debug.Log("No save file found or save count is 0. Scene will not be loaded.");
            return;
        }

        if (saveCountScenes != null && saveCountScenes.Length > 0)
        {
            int index = Mathf.Clamp(count, 0, saveCountScenes.Length - 1);
            string sceneToLoad = saveCountScenes[index];
            if (!string.IsNullOrEmpty(sceneToLoad))
            {
                StartCoroutine(LoadSceneDelayed(sceneToLoad));
            }
            else
            {
                Debug.LogWarning("Scene name at index " + index + " is empty.");
            }
        }
        else
        {
            Debug.LogWarning("saveCountScenes array is not set up in the Inspector.");
        }
    }

    public IEnumerator LoadSceneDelayed(string sceneName)
    {
        if (SceneTransitionAnimator != null)
        {
            SceneTransitionAnimator.SetTrigger("FadeOut");
        }
        yield return new WaitForSeconds(0.25f);
        SceneManager.LoadSceneAsync(sceneName);
    }

    public void CallSaveGame()
    {
        if (GameSaveLoad.Instance != null)
        {
            GameSaveLoad.Instance.SaveGame();
        }
        else
        {
            Debug.LogWarning("GameSaveLoad instance not found!");
        }
    }

    public void CallLoadGame()
    {
        if (GameSaveLoad.Instance != null)
        {
            GameSaveLoad.Instance.LoadGame();
        }
        else
        {
            Debug.LogWarning("GameSaveLoad instance not found!");
        }
    }

    public void CallDeleteSave()
    {
        Debug.Log("called delete save");
        if (GameSaveLoad.Instance != null)
        {
            GameSaveLoad.Instance.DeleteSave();
        }
        else
        {
            Debug.LogWarning("GameSaveLoad instance not found!");
        }
    }

    public void ResurrectPlayer()
    {
        if (!GameManager.Instance.ConsumeResurrection())
        {
            Debug.Log("Cannot resurrect: No resurrections remaining!");
            return;
        }

        ResurrectScreen.SetActive(true);
        Vector3 resurrectionPosition = playerPos;

        if (isPlayerInBossArena)
        {
            if (bossArenaExitPosition != Vector3.zero)
            {
                resurrectionPosition = bossArenaExitPosition;
            }
            else
            {
                resurrectionPosition = playerPos + Vector3.back * 10f;
            }

            if (currentBossArena != null)
            {
                currentBossArena.ResetAllBosses();
                Debug.Log("Boss arena reset due to player resurrection");
            }

            ExitBossArena();

            if (!string.IsNullOrEmpty(trackOnLoad))
            {
                ChangeMusic(trackOnLoad);
                Debug.Log($"World music restored: {trackOnLoad}");
            }
        }

        SpawnPlayer(resurrectionPosition, true);
    }

    public void EnterBossArena(BossArena bossArena, Vector3 arenaExitPosition)
    {
        isPlayerInBossArena = true;
        currentBossArena = bossArena;
        bossArenaExitPosition = arenaExitPosition;

        Debug.Log($"Player entered boss arena. Exit position set to {arenaExitPosition}");
    }

    public void ExitBossArena()
    {
        if (currentBossArena != null)
        {
            currentBossArena.ExitArena();
        }

        isPlayerInBossArena = false;
        currentBossArena = null;

        Debug.Log("Player exited boss arena");
    }

    public void BossDefeated()
    {
        if (isPlayerInBossArena)
        {
            ExitBossArena();
            Debug.Log("Boss defeated - arena cleared");
        }
    }

    public void GrantResurrection()
    {
        GameManager.Instance.GrantResurrection();
    }

    public int GetCurrentResurrections()
    {
        return GameManager.Instance.GetCurrentResurrections();
    }

    public bool CanResurrect()
    {
        return GameManager.Instance.CanResurrect();
    }

    private void OnMapToggle(InputAction.CallbackContext ctx)
    {
        if (map != null && !consoleObject.activeSelf)
        {
            map.SetActive(!map.activeSelf);
        }
    }

    private void OnPauseToggle(InputAction.CallbackContext ctx)
    {
        if (pauseMenu != null && !consoleObject.activeSelf)
        {
            pauseMenu.SetActive(!pauseMenu.activeSelf);
            StartCoroutine(delayedButtonSelect());
        }
    }

    IEnumerator delayedButtonSelect()
    {
        yield return new WaitForEndOfFrame();
        if (eventSystem != null && pauseMenu != null)
        {
            eventSystem.SetSelectedGameObject(pauseMenu.transform.GetChild(1).gameObject);
        }
    }

    public void EnableTextBox(string content)
    {
        textBoxText.text = content;
        textBoxObj.SetActive(true);
    }
}