using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    public LootPool lootPool;
    public Transform[] itemSpawnPos;

    public bool chestOpened;
    public Transform ChestLid;
    public float chestLidOpenAngle;
    public float lidOpenSpeed;

    float lidRot = 0f;

    public bool Locked;
    [HideInInspector]
    public bool wardensSpawned;
    bool interactionlocked;

    public ParticleSystem lockedParticles;
    public ParticleSystem unlockParticles;
    public List<GameObject> Wardens = new List<GameObject>();
    List<GameObject> WardensToRemove = new List<GameObject>();

    public void OnOpen(Entity entity)
    {
        if (interactionlocked) return;
        if (chestOpened) return;
        chestOpened = true;

        for (int i = 0; i < itemSpawnPos.Length; i++)
        {
            Item itemToSpawn = lootPool.GetItem();
            if (itemToSpawn != null)
            {
                ItemInstanceData itemData = new ItemInstanceData();
                if (itemToSpawn.itemType == Item.ItemTypes.Weapon)
                {
                    int slotCount = Random.Range(1, 4);
                    for (int j = 0; j < slotCount; j++)
                    {
                        ItemInstanceData.ModSlot modSlot = new ItemInstanceData.ModSlot();
                        // Set random mod slot type
                        System.Array modTypeValues = System.Enum.GetValues(typeof(Item.ModType));
                        modSlot.modSlotType = (Item.ModType)modTypeValues.GetValue(Random.Range(0, modTypeValues.Length));
                        itemData.ModSlots.Add(modSlot);
                    }
                }

                GameManager.Instance.SpawnItemPickUp(itemToSpawn, itemData, itemSpawnPos[i].position);
            }
        }
    }

    private void Update()
    {
        if (chestOpened)
        {
            ChestLid.localRotation = Quaternion.Euler(0, 0, lidRot);
            lidRot = Mathf.Lerp(lidRot, chestLidOpenAngle, Time.deltaTime * lidOpenSpeed);
        }

        if (Wardens.Count > 0)
        {
            wardensSpawned = true;
        }

        if (Locked && wardensSpawned)
        {
            if (Wardens.Count > 0)
            {
                interactionlocked = true;
            }
            else
            {
                interactionlocked = false;
                lockedParticles.Stop();
                unlockParticles.Play();
                Locked = false;
            }

            WardensToRemove.Clear();
            foreach (GameObject warden in Wardens)
            {
                if (warden.gameObject == null)
                {
                    WardensToRemove.Add(warden.gameObject);
                }
            }

            foreach (GameObject warden in WardensToRemove)
            {
                Wardens.Remove(warden);
            }
        }
    }
}
