using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChestSpawner : MonoBehaviour
{
    public NpcSpawner NpcSpawner;
    public int SpawnFactor;
    public List<Transform> SpawnPoints = new List<Transform>();

    public GameObject[] ChestObjects;

    public List<Chest> spawnedChests = new List<Chest>();

    private void Start()
    {
        for (int i = 0; i < SpawnFactor; i++)
        {
            UnitManager.Instance.ChestSpawnFactor++;
            if (UnitManager.Instance.ChestSpawnFactor >= UnitManager.Instance.ChestSpawnFactorRequirement)
            {
                UnitManager.Instance.ChestSpawnFactor -= UnitManager.Instance.ChestSpawnFactorRequirement;
                if (SpawnPoints.Count > 0)
                {
                    int randSpawn = Random.Range(0, SpawnPoints.Count);
                    Chest spawnedChest = Instantiate(ChestObjects[Random.Range(0, ChestObjects.Length)], SpawnPoints[randSpawn].position, SpawnPoints[randSpawn].rotation, transform.parent).GetComponent<Chest>();
                    spawnedChests.Add(spawnedChest);
                    SpawnPoints.Remove(SpawnPoints[randSpawn]);
                }
            }
        }
    }

    void Update()
    {
        if (NpcSpawner.spawned)
        {
            foreach (Chest spawnedChest in spawnedChests)
            {
                if (!spawnedChest.wardensSpawned) spawnedChest.Wardens.AddRange(NpcSpawner.spawnedNpcs.Select(npc => npc.gameObject));
            }
        }
    }
}
