using System.Collections.Generic;
using UnityEngine;

public class ReanimateAbility : MonoBehaviour
{
    public SoulChargeHandler soulChargeHandler;
    public float soulChargeCost = 15f;

    public Ability ability;

    public float captureRadius = 2f;

    public GameObject spawnEffect;
    public GameObject NpcToSpawn;
    public float NpcHealthMultiplier = 0.5f;
    public float npcHealthDrain;
    float healthDrain;

    public List<NpcCombatAI> spawnedNpcs = new List<NpcCombatAI>();

    public void Cast(Vector3 pos)
    {
        List<Entity> soulTouchedCorpses = new List<Entity>();
        List<Entity> otherCorpses = new List<Entity>();

        Collider[] enemiesHit = Physics.OverlapSphere(pos, captureRadius);
        foreach (Collider hit in enemiesHit)
        {
            Entity hitEntity = hit.GetComponent<Entity>();
            if (hitEntity != null && hitEntity.EntityType == Entity.EntityTypes.Corpse)
            {
                bool isSoulTouched = false;
                foreach (Debuff debuff in hitEntity.activeDebuffs)
                {
                    if (debuff.DebuffName == "Soul Touched")
                    {
                        isSoulTouched = true;
                        break;
                    }
                }

                if (!hitEntity.Impaled)
                {
                    if (isSoulTouched)
                    {
                        soulTouchedCorpses.Add(hitEntity);
                    }
                    else
                    {
                        otherCorpses.Add(hitEntity);
                    }
                }
            }
        }

        Entity closestEntity = null;
        float closest = float.MaxValue;

        // Prioritize soul touched corpses
        List<Entity> prioritizedList = soulTouchedCorpses.Count > 0 ? soulTouchedCorpses : otherCorpses;

        foreach (Entity item in prioritizedList)
        {
            float dist = Vector3.Distance(item.transform.position, pos);
            if (dist < closest)
            {
                closest = dist;
                closestEntity = item;
            }
        }

        if (closestEntity != null)
        {
            bool isSoulTouched = soulTouchedCorpses.Contains(closestEntity);
            ReanimateCorpse(closestEntity, isSoulTouched);
        }
    }

    private void Update()
    {
        if(spawnedNpcs.Count > 0)
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

        foreach (NpcCombatAI spawnedNpc in spawnedNpcs)
        {
            spawnedNpc.spawnPos = ability.Owner.Body.position;
        }
    }

    void ReanimateCorpse(Entity capturedCorpse, bool IsSoulTouched)
    {
        if(soulChargeHandler != null)
        {
            if(!IsSoulTouched)
            {
                if (soulChargeHandler.SoulCharge < soulChargeCost)
                {
                    return;
                }
                soulChargeHandler.SoulCharge -= soulChargeCost;
            }
        }
        else
        {
            if (!IsSoulTouched)
            {
                return;
            }
        }

        if (spawnEffect != null)
        {
            Instantiate(spawnEffect, capturedCorpse.Body.position + new Vector3(0f, 0f, 0f), Quaternion.identity);
        }
        Entity instantiatedEntity = Instantiate(NpcToSpawn, capturedCorpse.Body.position + new Vector3(0f, 0.5f, 0f), Quaternion.identity).GetComponent<Entity>();
        if (instantiatedEntity != null) 
        {
            instantiatedEntity.Team = ability.Owner.Team;
            instantiatedEntity.MaxHealth = (int)(capturedCorpse.MaxHealth * NpcHealthMultiplier);
            instantiatedEntity.Health = (int)(capturedCorpse.Health * NpcHealthMultiplier);
            spawnedNpcs.Add(instantiatedEntity.GetComponent<NpcCombatAI>());
        }
        Destroy(capturedCorpse.gameObject);
    }
}
