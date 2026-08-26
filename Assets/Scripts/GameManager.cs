using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject itemPickUpObj;
    public GameObject draggableItemObj;

    public string selectedPlayerClass = "player_classes/Player";

    [Header("Resurrection System")]
    public int maxResurrections = 2;
    public int currentResurrections = 1; // Start with 1 revive

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SpawnItemPickUp(Item item, ItemInstanceData itemData, Vector3 pos)
    {
        GameObject SpawnedPickUp = Instantiate(itemPickUpObj, pos, Quaternion.identity);
        ItemPickUp pickUpScript = SpawnedPickUp.GetComponent<ItemPickUp>();
        pickUpScript.item = item;
        if (itemData != null)
        {
            pickUpScript.itemData = itemData;
        }
        else
        {
            pickUpScript.itemData = new ItemInstanceData();
        }
    }

    public void SpawnItemPickUp(ItemInstance itemInstance, Vector3 pos)
    {
        if (itemInstance == null || itemInstance.item == null)
        {
            Debug.LogWarning("Cannot spawn item pickup: ItemInstance or Item is null");
            return;
        }

        SpawnItemPickUp(itemInstance.item, itemInstance.itemData, pos);
    }

    public void SpawnItemPickUp(Item item, Vector3 pos)
    {
        SpawnItemPickUp(item, null, pos);
    }

    public void SpawnItemPickUpFromSlot(PlayerInventory.ItemSlot slot, Vector3 pos)
    {
        if (slot?.itemInstance?.item != null)
        {
            SpawnItemPickUp(slot.itemInstance, pos);
            
            slot.itemInstance = new ItemInstance();
        }
        else
        {
            Debug.LogWarning("Cannot spawn item pickup: Slot is empty or invalid");
        }
    }

    public void AddItemToSlot(ItemInstance itemInstance, PlayerInventory.ItemSlot slot, PlayerInventory inventory)
    {
        slot.itemInstance = itemInstance;

        if (slot.slotImage != null && itemInstance != null && itemInstance.item != null)
        {
            slot.slotImage.sprite = itemInstance.item.itemIcon;
            slot.slotImage.gameObject.SetActive(true);
        }
        else if (slot.slotImage != null)
        {
            slot.slotImage.gameObject.SetActive(false);
        }

        if (inventory != null)
        {
            inventory.RenderItems();
        }
    }

    public static void SpawnExplosion(Vector3 explosionPos, float Radius, float nearRadius, float farRadius, float fallOff, DamageInstance damageInstance, DebuffPreset debuffToApply, Entity owner, bool impalementAttack = false)
    {
        Collider[] enemiesHit = Physics.OverlapSphere(explosionPos, Radius);
        foreach (Collider hit in enemiesHit)
        {
            Entity hitEntity = hit.GetComponent<Entity>();
            if (hitEntity != null)
            {
                float dist = Vector3.Distance(explosionPos, hitEntity.Body.position);
                float damageMultiplier = 1f;

                if (farRadius > nearRadius)
                {
                    if (dist <= nearRadius)
                    {
                        damageMultiplier = 1f;
                    }
                    else if (dist >= farRadius)
                    {
                        damageMultiplier = Mathf.Clamp01(1f - fallOff);
                    }
                    else
                    {
                        float t = (dist - nearRadius) / (farRadius - nearRadius);
                        damageMultiplier = Mathf.Lerp(1f, Mathf.Clamp01(1f - fallOff), t);
                    }
                }
                else
                {
                    damageMultiplier = 1f;
                }

                DamageInstance explosionDamage = new DamageInstance(damageInstance)
                {
                    HealthDamage = damageInstance.HealthDamage * owner.DamageMultiplier,
                    StaggerDamage = damageInstance.StaggerDamage * owner.DamageMultiplier,
                    SeverenceDamage = damageInstance.SeverenceDamage * owner.DamageMultiplier,
                };
                explosionDamage.Multiplier = damageMultiplier;

                Vector3 flatknockDir = new Vector3(
                    hitEntity.Body.position.x - explosionPos.x,
                    0f,
                    hitEntity.Body.position.z - explosionPos.z
                );

                explosionDamage.knockbackDir = flatknockDir.normalized;
                hitEntity.TakeDamage(explosionDamage, owner, impalementAttack);

                if (debuffToApply != null)
                {
                    if (string.IsNullOrEmpty(debuffToApply.DebuffName))
                        continue;

                    if (!debuffToApply.transfersToCorpse && hitEntity.EntityType == Entity.EntityTypes.Corpse)
                        continue;

                    if (owner != null && hitEntity.Team == owner.Team)
                        continue;

                    bool hasMatchingDebuff = false;
                    Debuff matchingDebuff = null;
                    foreach (Debuff activeDebuffs in hitEntity.activeDebuffs)
                    {
                        if (activeDebuffs.DebuffName == debuffToApply.DebuffName)
                        {
                            hasMatchingDebuff = true;
                            matchingDebuff = activeDebuffs;
                            break;
                        }
                    }

                    if (hasMatchingDebuff)
                    {
                        if (debuffToApply.Duration > matchingDebuff.Duration)
                            matchingDebuff.Duration = debuffToApply.Duration;
                        owner.OnStatusApplied.Invoke(hitEntity, matchingDebuff);
                        continue;
                    }

                    Debuff newDebuff = new Debuff(debuffToApply, owner);
                    newDebuff.Applier = owner;
                    hitEntity.activeDebuffs.Add(newDebuff);
                    owner.OnStatusApplied.Invoke(hitEntity, newDebuff);
                }
            }
            /*else
            {
                Rigidbody hitRb = hit.GetComponent<Rigidbody>();
                if (hitRb != null)
                {
                    Vector3 knockDIr = (hitRb.transform.position - capturedCorpse.Body.position).normalized;
                    hitRb.AddExplosionForce(explosionForce, capturedCorpse.Body.position, explosionRadius);
                }
            }*/
        }
    }

    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName);
    }

    /// <summary>
    /// Grant a resurrection token to the player
    /// </summary>
    public void GrantResurrection()
    {
        if (currentResurrections < maxResurrections)
        {
            currentResurrections++;
            Debug.Log($"Resurrection token granted! Total: {currentResurrections}/{maxResurrections}");
            
            // Auto-save resurrection data
            SaveResurrections();
        }
        else
        {
            Debug.Log($"Cannot grant resurrection: Already at maximum ({maxResurrections})");
        }
    }

    /// <summary>
    /// Get the current number of resurrections available
    /// </summary>
    /// <returns>Number of resurrections remaining</returns>
    public int GetCurrentResurrections()
    {
        return currentResurrections;
    }

    /// <summary>
    /// Check if player can resurrect
    /// </summary>
    /// <returns>True if player has resurrections available</returns>
    public bool CanResurrect()
    {
        return currentResurrections > 0;
    }

    /// <summary>
    /// Consume a resurrection token
    /// </summary>
    /// <returns>True if resurrection was consumed, false if none available</returns>
    public bool ConsumeResurrection()
    {
        if (currentResurrections <= 0)
        {
            Debug.Log("Cannot consume resurrection: No resurrections remaining!");
            return false;
        }
        
        currentResurrections--;
        Debug.Log($"Resurrection consumed. Resurrections remaining: {currentResurrections}");
        
        // Auto-save resurrection data
        SaveResurrections();
        return true;
    }

    /// <summary>
    /// Reset resurrections to starting amount (for new runs)
    /// </summary>
    public void ResetResurrections()
    {
        currentResurrections = 1; // Start with 1 revive
        Debug.Log($"Resurrections reset to starting amount: {currentResurrections}");
    }

    /// <summary>
    /// Save resurrection data to disk
    /// </summary>
    private void SaveResurrections()
    {
        if (GameSaveLoad.Instance != null)
        {
            // Save only GameManager data (which includes resurrections)
            GameSaveLoad.Instance.SaveGame(GameSaveLoad.OperationMode.DISK_OP);
            Debug.Log("Resurrection data saved");
        }
    }
}
