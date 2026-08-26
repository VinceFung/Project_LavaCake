using System.Collections.Generic;
using UnityEngine;

public class NpcSpawner : MonoBehaviour
{
    public Entity summonOwner;
    public float SpawnDistRequirement = 20f;
    [HideInInspector]
    public bool spawned = false;

    [System.Serializable]
    public class NpcSpawn
    {
        public Transform SpawnPoint;
        public GameObject[] Npcs;
    }

    public NpcSpawn[] NpcSpawns;

    public float spawnerDanger = 0f;
    public float dangerBonusHealth = 2.5f;
    public float dangerBonusHealthCap = 2.5f;
    public float initialBonusHealth = 0f;

    [Space(10)]
    public float npcHealthDrain;
    float healthDrain;

    public List<NpcCombatAI> spawnedNpcs = new List<NpcCombatAI>();

    private void Start()
    {

    }

    private void Update()
    {
        if (SpawnDistRequirement > 0f)
        {
            if (UnitManager.Instance.playerObj != null)
            {
                // Use horizontal distance for spawn triggering
                Vector3 horizontalDistance = UnitManager.Instance.playerObj.transform.position - transform.position;
                horizontalDistance.y = 0;
                if (horizontalDistance.magnitude <= SpawnDistRequirement && spawned == false)
                {
                    spawned = true;
                    Generate();
                }
            }
        }

        if (spawnedNpcs.Count > 0)
        {
            healthDrain += Time.deltaTime * npcHealthDrain;
            if (healthDrain >= 1f)
            {
                foreach (NpcCombatAI spawnedNpc in spawnedNpcs)
                {
                    spawnedNpc.entity.Health -= (int)healthDrain;
                }
                healthDrain = 0f;
            }
        }

        if(summonOwner != null)
        {
            foreach (NpcCombatAI spawnedNpc in spawnedNpcs)
            {
                spawnedNpc.spawnPos = summonOwner.Body.position;
            }
        }
    }

    public void Generate()
    {
        foreach (NpcSpawn strucSpawn in NpcSpawns)
        {
            GameObject instantiatedEntity = Instantiate(strucSpawn.Npcs[Random.Range(0, strucSpawn.Npcs.Length)], strucSpawn.SpawnPoint.position, Quaternion.identity);
            if (summonOwner == null) instantiatedEntity.transform.parent = this.transform;

            Entity entity = instantiatedEntity.GetComponent<Entity>();
            if (entity != null)
            {
                if (summonOwner != null) entity.Team = summonOwner.Team;

                entity.Body.rotation = strucSpawn.SpawnPoint.rotation;

                float healthMultiplier = (dangerBonusHealth * spawnerDanger) + initialBonusHealth;
                healthMultiplier = Mathf.Clamp(healthMultiplier, 0f, dangerBonusHealthCap);

                entity.MaxHealth = (int)(entity.MaxHealth * (1f + healthMultiplier));
                entity.severenceDamageRequirement = entity.severenceDamageRequirement * (1f + (healthMultiplier / 2f));

                if (entity.ImpalementImmune)
                {
                    entity.StaggerRequirement = entity.StaggerRequirement * (1f + (healthMultiplier * 2f));
                    entity.MaxHealth = (int)(entity.MaxHealth * (1f + (healthMultiplier * 2f)));
                }
                else
                {
                    entity.MaxHealth = (int)(entity.MaxHealth * (1f + healthMultiplier));
                }

                entity.Health = entity.MaxHealth;

                spawnedNpcs.Add(instantiatedEntity.GetComponent<NpcCombatAI>());
            }
        }
    }
}
