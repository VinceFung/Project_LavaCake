using System.Collections.Generic;
using UnityEngine;

public class PoisonFieldAbility : MonoBehaviour
{
    public Ability ability;
    public float captureRadius = 2f;
    public float explosionRadius = 7f;
    public float explosionRadiusNear = 3f;
    public float explosionRadiusFar = 5f;
    public float baseRadiusMultiplier = 0.5f;
    public float maxHeathRadiusRatio = 800f;
    public float maxHealthDamageMultiplier;
    public DamageInstance maxDamage;
    public float damageFallOff = 0.067f;

    public DebuffPreset debuffToApply;

    public GameObject initialSpawnEffect;
    public GameObject spawnEffect;
    public GameObject tickEffect;
    public float Duration;
    public float tickRate;
    float nextTimeToTick;

    [System.Serializable]
    public class PoisonField
    {
        public Vector3 pos;
        public float corpseHealth;
        public float duration;
        public float nextTimeToTick;
        public ParticleSystem effect;
    }
    public List<PoisonField> PoisonFields = new List<PoisonField>();

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
        if(closestEntity != null) ExplodeCorpse(closestEntity);
    }

    void ExplodeCorpse(Entity capturedCorpse)
    {
        DamageInstance dmg = new DamageInstance(maxDamage);
        dmg.HealthDamage = capturedCorpse.MaxHealth * maxHealthDamageMultiplier;
        dmg.StaggerDamage = capturedCorpse.MaxHealth * maxHealthDamageMultiplier;
        float corpseHP = capturedCorpse.MaxHealth;

        float radiusMultiplier = baseRadiusMultiplier + (corpseHP / maxHeathRadiusRatio);

        Instantiate(initialSpawnEffect, capturedCorpse.Body.position, Quaternion.identity).transform.localScale = Vector3.one * explosionRadius * radiusMultiplier;

        capturedCorpse.Die();

        GameManager.SpawnExplosion(capturedCorpse.Body.position, explosionRadius * radiusMultiplier, explosionRadiusNear * radiusMultiplier, explosionRadiusFar * radiusMultiplier, damageFallOff, dmg, debuffToApply, ability.Owner);

        PoisonField spawnedField = new PoisonField();
        spawnedField.pos = capturedCorpse.Body.position;
        spawnedField.corpseHealth = corpseHP;
        spawnedField.duration = Duration;
        spawnedField.nextTimeToTick = Time.time + tickRate;
        spawnedField.effect = Instantiate(spawnEffect, capturedCorpse.Body.position, Quaternion.identity).GetComponent<ParticleSystem>();
        spawnedField.effect.transform.localScale = Vector3.one * explosionRadius * radiusMultiplier;
        PoisonFields.Add(spawnedField);

        ability.Owner.OnAbilityCast.Invoke();
    }

    private void Update()
    {
        foreach (PoisonField field in PoisonFields.ToArray())
        {
            if(field.duration > 0f)
            {
                field.duration -= Time.deltaTime;

                if(Time.time >= field.nextTimeToTick)
                {
                    field.nextTimeToTick = Time.time + tickRate;

                    float radiusMultiplier = baseRadiusMultiplier + (field.corpseHealth / maxHeathRadiusRatio);

                    Instantiate(tickEffect, field.pos, Quaternion.identity).transform.localScale = Vector3.one * explosionRadius * radiusMultiplier;

                    DamageInstance dmg = new DamageInstance(maxDamage);
                    dmg.HealthDamage = field.corpseHealth * maxHealthDamageMultiplier;
                    dmg.StaggerDamage = 0f;
                    GameManager.SpawnExplosion(field.pos, explosionRadius * radiusMultiplier, explosionRadius, explosionRadius, 0f, dmg, debuffToApply, ability.Owner);
                }
            }
            else
            {
                field.effect.Stop();
                PoisonFields.Remove(field);
            }
        }
    }
}
