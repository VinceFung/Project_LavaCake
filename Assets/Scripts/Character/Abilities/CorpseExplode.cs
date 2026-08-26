using System.Collections.Generic;
using UnityEngine;

public class CorpseExplode : MonoBehaviour
{
    public Ability ability;
    public float captureRadius = 2f;
    public GameObject explosionEffect;
    public float explosionRadius = 7f;
    public float explosionRadiusNear = 3f;
    public float explosionRadiusFar = 5f;
    public float maxHealthDamageMultiplier;
    public float baseRadiusMultiplier = 0.5f;
    public float maxHeathRadiusRatio = 800f;
    public DamageInstance maxDamage;
    public float damageFallOff = 0.067f;

    public DebuffPreset debuffToApply;
    public DebuffPreset debuffToApplyToAllies;

    List<Entity> entitiesCaught = new List<Entity>();

    public void Cast(Vector3 pos)
    {
        entitiesCaught.Clear();
        Collider[] enemiesHit = Physics.OverlapSphere(pos, captureRadius);
        foreach (Collider hit in enemiesHit)
        {
            Entity hitEntity = hit.GetComponent<Entity>();
            if (hitEntity != null)
            {
                if (hitEntity.EntityType == Entity.EntityTypes.Corpse && !entitiesCaught.Contains(hitEntity))
                {
                    bool IsSoulTouched = false;
                    foreach (Debuff debuff in hitEntity.activeDebuffs)
                    {
                        if (debuff.DebuffName == "Soul Touched")
                        {
                            IsSoulTouched = true;
                        }
                    }

                    if (IsSoulTouched && !hitEntity.Impaled)
                    {
                        entitiesCaught.Add(hitEntity);
                    }
                }
            }
        }

        Entity closestEntity = null;
        float closest = 99999999f;
        foreach (Entity item in entitiesCaught)
        {
            float dist = Vector3.Distance(item.transform.position, pos);
            if (dist < closest)
            {
                closest = dist;
                closestEntity = item;
            }
        }
        if (closestEntity != null) ExplodeCorpse(closestEntity);
    }

    void ExplodeCorpse(Entity capturedCorpse)
    {
        DamageInstance dmg = new DamageInstance(maxDamage);
        dmg.HealthDamage = capturedCorpse.MaxHealth * maxHealthDamageMultiplier;
        dmg.StaggerDamage = capturedCorpse.MaxHealth * maxHealthDamageMultiplier;
        float corpseHP = capturedCorpse.MaxHealth;

        float radiusMultiplier = baseRadiusMultiplier + (corpseHP / maxHeathRadiusRatio);

        Instantiate(explosionEffect, capturedCorpse.Body.position, Quaternion.identity).transform.localScale = Vector3.one * explosionRadius * radiusMultiplier;

        capturedCorpse.Die();

        GameManager.SpawnExplosion(capturedCorpse.Body.position, explosionRadius * radiusMultiplier, explosionRadiusNear * radiusMultiplier, explosionRadiusFar * radiusMultiplier, damageFallOff, dmg, debuffToApply, ability.Owner);

        Collider[] alliesHit = Physics.OverlapSphere(capturedCorpse.Body.position, explosionRadius * radiusMultiplier);
        foreach (Collider ally in alliesHit)
        {
            Entity allyEntity = ally.GetComponent<Entity>();
            if (allyEntity != null && allyEntity.Team == ability.Owner.Team)
            {
                // Always extend "Covered In Blood" duration by 5 seconds if present
                foreach (Debuff debuff in allyEntity.activeDebuffs)
                {
                    if (debuff.DebuffName == "Blood Rush")
                    {
                        debuff.Duration += 5f;
                        // No break; in case there are multiple, but usually there should be only one
                    }
                }

                allyEntity.ApplyDebuff(debuffToApplyToAllies, ability.Owner);
            }
        }

        ability.Owner.OnAbilityCast.Invoke();
    }
}
