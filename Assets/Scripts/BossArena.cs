using UnityEngine;

/// <summary>
/// Boss Arena script that handles trigger detection, multiple boss enemies, and arena management
/// </summary>
public class BossArena : MonoBehaviour
{
    [Header("Arena Configuration")]
    public Vector3 arenaExitPosition = Vector3.zero; // Position where player should respawn if they die
    public bool autoRegisterWithUnitManager = true;
    
    [Header("Boss Enemies")]
    public GameObject[] bossEnemyPrefabs; // Prefabs to spawn when fight starts
    public Transform[] bossSpawnPoints; // Where to spawn the bosses
    
    [Header("Arena Objects")]
    public GameObject[] arenaObjects; // Objects to enable/disable with this boss fight
    public GameObject[] arenaTriggers;
    public GameObject[] arenaBarriers; // Barriers that should be disabled when leaving this arena
    
    private bool hasStartedFight = false; // Track if this arena's boss fight has started
    private bool isArenaActive = false; // Track if arena is currently active
    private BossForgiveness[] spawnedBosses; // Track spawned boss instances
    private int aliveBossCount = 0; // Track how many bosses are still alive

    void Start()
    {
        // Auto-register with UnitManager if enabled
        if (autoRegisterWithUnitManager && UnitManager.Instance != null)
        {
            // Set a default exit position if none specified (behind the arena center)
            if (arenaExitPosition == Vector3.zero)
            {
                arenaExitPosition = transform.position + Vector3.back * 15f;
                arenaExitPosition.y = transform.position.y; // Keep same Y level
            }
        }
        
        // Initialize spawned bosses array
        spawnedBosses = new BossForgiveness[bossEnemyPrefabs.Length];
    }

    private void Update()
    {
        // Check if all bosses are defeated (only if fight has started)
        if (hasStartedFight && aliveBossCount <= 0)
        {
            OnBossArenaCleared();
        }
    }

    /// <summary>
    /// Called when a boss in this arena dies
    /// </summary>
    public void OnBossDied()
    {
        aliveBossCount--;
        Debug.Log($"Boss died in arena {gameObject.name}. Remaining bosses: {aliveBossCount}");
    }

    /// <summary>
    /// Reset all bosses in this arena
    /// </summary>
    public void ResetAllBosses()
    {
        // Destroy any existing spawned bosses
        for (int i = 0; i < spawnedBosses.Length; i++)
        {
            if (spawnedBosses[i] != null && spawnedBosses[i].entity != null)
            {
                Destroy(spawnedBosses[i].entity.gameObject);
            }
        }
        
        // Clear the array
        spawnedBosses = new BossForgiveness[bossEnemyPrefabs.Length];
        aliveBossCount = 0;
        
        Debug.Log($"All bosses in arena {gameObject.name} have been reset");
    }

    /// <summary>
    /// Call this when the boss fight begins (e.g., when player enters arena or any boss is alerted)
    /// </summary>
    public void StartBossFight()
    {
        if (hasStartedFight) return; // Prevent multiple starts
        
        hasStartedFight = true;
        isArenaActive = true;
        
        if (UnitManager.Instance != null && autoRegisterWithUnitManager)
        {
            UnitManager.Instance.EnterBossArena(this, arenaExitPosition);
        }
        
        // Spawn all boss enemies
        aliveBossCount = 0;
        for (int i = 0; i < bossEnemyPrefabs.Length; i++)
        {
            if (bossEnemyPrefabs[i] != null)
            {
                // Determine spawn position
                Vector3 spawnPos = (bossSpawnPoints != null && i < bossSpawnPoints.Length && bossSpawnPoints[i] != null) 
                    ? bossSpawnPoints[i].position 
                    : transform.position + Vector3.forward * (i * 3f); // Default spacing
                
                // Spawn the boss
                GameObject spawnedBoss = Instantiate(bossEnemyPrefabs[i], spawnPos, Quaternion.identity);
                BossForgiveness bossScript = spawnedBoss.GetComponent<BossForgiveness>();
                
                if (bossScript != null)
                {
                    bossScript.SetBossArena(this);
                    spawnedBosses[i] = bossScript;
                    
                    // Subscribe to death event
                    if (bossScript.entity != null)
                    {
                        bossScript.entity.OnDeath.AddListener(OnBossDied);
                        aliveBossCount++;
                    }
                }
            }
        }
        
        // Call function to enable arena objects (instead of managing object arrays directly)
        OnArenaActivated();
        
        Debug.Log($"Boss fight started in arena {gameObject.name} - ARENA LOCKED! Spawned {aliveBossCount} bosses.");
    }
    
    /// <summary>
    /// Function called when arena is activated - override this or use events to customize behavior
    /// </summary>
    public virtual void OnArenaActivated()
    {
        // Enable arena objects
        foreach (GameObject obj in arenaObjects)
        {
            if (obj != null) obj.SetActive(true);
        }

        // Disable arena triggers - no escape once the fight begins!
        foreach (GameObject trigger in arenaTriggers)
        {
            if (trigger != null) trigger.SetActive(false);
        }
        
        // Enable arena barriers - lock the player in
        foreach (GameObject barrier in arenaBarriers)
        {
            if (barrier != null) barrier.SetActive(true);
        }
    }

    /// <summary>
    /// Function called when arena is cleared - override this or use events to customize behavior
    /// </summary>
    public virtual void OnArenaCleared()
    {
        // Disable arena barriers to allow passage
        foreach (GameObject barrier in arenaBarriers)
        {
            if (barrier != null) barrier.SetActive(false);
        }

        foreach (GameObject trigger in arenaTriggers)
        {
            if (trigger != null) trigger.SetActive(true);
        }
        
        // Optionally disable arena objects
        foreach (GameObject obj in arenaObjects)
        {
            if (obj != null) obj.SetActive(false);
        }
    }

    /// <summary>
    /// Function called when arena is exited (player resurrection) - override this or use events to customize behavior
    /// </summary>
    public virtual void OnArenaExited()
    {
        // Disable arena barriers to allow passage
        foreach (GameObject barrier in arenaBarriers)
        {
            if (barrier != null) barrier.SetActive(false);
        }
        
        // Re-enable arena triggers so player can enter again
        foreach (GameObject trigger in arenaTriggers)
        {
            if (trigger != null) trigger.SetActive(true);
        }
        
        // Optionally disable arena objects
        foreach (GameObject obj in arenaObjects)
        {
            if (obj != null) obj.SetActive(false);
        }
    }

    /// <summary>
    /// Call this when all bosses are defeated
    /// </summary>
    public void OnBossArenaCleared()
    {
        if (!hasStartedFight) return; // Only process if fight was active
        
        hasStartedFight = false;
        isArenaActive = false;
        
        if (UnitManager.Instance != null)
        {
            UnitManager.Instance.BossDefeated();
        }
        
        // Call function instead of managing objects directly
        OnArenaCleared();
        
        Debug.Log($"Boss arena {gameObject.name} cleared - all enemies defeated");
    }
    
    /// <summary>
    /// Manually exit this boss arena (called on player resurrection)
    /// </summary>
    public void ExitArena()
    {
        hasStartedFight = false;
        isArenaActive = false;
        
        // Call function instead of managing objects directly
        OnArenaExited();
        
        Debug.Log($"Exited boss arena {gameObject.name} - arena reset for re-entry");
    }

    /// <summary>
    /// Get the arena exit position for player resurrection
    /// </summary>
    /// <returns>Position where player should be resurrected</returns>
    public Vector3 GetExitPosition()
    {
        return arenaExitPosition;
    }

    /// <summary>
    /// Check if this arena's boss fight has started
    /// </summary>
    /// <returns>True if boss fight is active</returns>
    public bool HasStartedFight()
    {
        return hasStartedFight;
    }

    /// <summary>
    /// Check if this arena is currently active
    /// </summary>
    /// <returns>True if arena is active</returns>
    public bool IsArenaActive()
    {
        return isArenaActive;
    }
    
    private void OnDrawGizmos()
    {
        // Draw the trigger area (always red since it's entrance-only)
        Gizmos.color = Color.red;
        Gizmos.matrix = transform.localToWorldMatrix;
        
        Collider col = GetComponent<Collider>();
        if (col is BoxCollider boxCol)
        {
            Gizmos.DrawWireCube(boxCol.center, boxCol.size);
        }
        else if (col is SphereCollider sphereCol)
        {
            Gizmos.DrawWireSphere(sphereCol.center, sphereCol.radius);
        }
        
        // Draw exit position
        if (arenaExitPosition != Vector3.zero)
        {
            Gizmos.color = Color.blue;
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.DrawWireSphere(arenaExitPosition, 1f);
            Gizmos.DrawLine(transform.position, arenaExitPosition);
        }
        
        // Draw connections to boss spawn points
        Gizmos.color = Color.red;
        if (bossSpawnPoints != null)
        {
            foreach (Transform spawnPoint in bossSpawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawLine(transform.position, spawnPoint.position);
                    Gizmos.DrawWireSphere(spawnPoint.position, 2f);
                }
            }
        }
        
        // Draw boss prefab indicators
        Gizmos.color = Color.magenta;
        for (int i = 0; i < bossEnemyPrefabs.Length; i++)
        {
            if (bossEnemyPrefabs[i] != null)
            {
                Vector3 indicatorPos = (bossSpawnPoints != null && i < bossSpawnPoints.Length && bossSpawnPoints[i] != null) 
                    ? bossSpawnPoints[i].position + Vector3.up * 3f
                    : transform.position + Vector3.forward * (i * 3f) + Vector3.up * 3f;
                
                Gizmos.DrawWireCube(indicatorPos, Vector3.one * 0.5f);
            }
        }
        
        // Draw arena objects connections
        Gizmos.color = Color.yellow;
        foreach (GameObject obj in arenaObjects)
        {
            if (obj != null)
            {
                Gizmos.DrawLine(transform.position, obj.transform.position);
                Gizmos.DrawWireCube(obj.transform.position, Vector3.one * 0.5f);
            }
        }
        
        // Draw arena barriers connections
        Gizmos.color = new Color(1f, 0.5f, 0f); // Orange color
        foreach (GameObject barrier in arenaBarriers)
        {
            if (barrier != null)
            {
                Gizmos.DrawLine(transform.position, barrier.transform.position);
                Gizmos.DrawWireCube(barrier.transform.position, Vector3.one * 0.8f);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if player entered the boss arena
        if (other.CompareTag("Player") && UnitManager.Instance != null)
        {
            if (bossEnemyPrefabs.Length > 0 && !hasStartedFight)
            {
                // Player entered boss arena - no way out except victory or death!
                StartBossFight();
                
                Debug.Log($"Player entered boss arena: {gameObject.name} - Fight to the death!");
            }
        }
    }
}
