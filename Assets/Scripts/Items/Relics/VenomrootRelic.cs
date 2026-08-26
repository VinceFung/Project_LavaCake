using System.Collections.Generic;
using UnityEngine;

public class VenomrootRelic : MonoBehaviour
{
    public EntityRelic relic;
    public DebuffPreset debuffToApply;
    public BuffDisplay.DisplayTypes buffDisplayType = BuffDisplay.DisplayTypes.DamageBonus;
    public GameObject poisonFieldEffect;
    public float poisonFieldRadius = 4f;
    public float fieldDuration = 4f;
    public float tickRate = 0.5f;
    public float poisonFieldDamageMultiplier = 1.5f;
    public DamageInstance poisonFieldDamage;

    bool fieldSpawnReady;
    float fieldSpawnCooldownTime;

    MeleeWeapon lastMelee;

    [System.Serializable]
    public class PoisonField
    {
        public Vector3 pos;
        public DamageInstance fieldDamage;
        public float duration;
        public float nextTimeToTick;
        public ParticleSystem effect;
    }
    public List<PoisonField> PoisonFields = new List<PoisonField>();

    private void Start()
    {
        lastMelee = relic.owner.meleeWeapon;
        relic.owner.meleeWeapon.stats.OnDirectDamageDealt.AddListener(SpawnFieldOnHit);
        relic.owner.OnStatusApplied.AddListener(OnPoisonStatus);
    }

    private void OnDestroy()
    {
        relic.owner.meleeWeapon.stats.OnDirectDamageDealt.RemoveListener(SpawnFieldOnHit);
        relic.owner.OnStatusApplied.RemoveListener(OnPoisonStatus);
    }

    private void Update()
    {
        if (relic.owner.meleeWeapon != lastMelee)
        {
            relic.owner.meleeWeapon.stats.OnDirectDamageDealt.AddListener(SpawnFieldOnHit);
        }
        lastMelee = relic.owner.meleeWeapon;

        foreach (PoisonField field in PoisonFields.ToArray())
        {
            if (field.duration > 0f)
            {
                field.duration -= Time.deltaTime;

                if (Time.time >= field.nextTimeToTick)
                {
                    field.nextTimeToTick = Time.time + tickRate;
;
                    GameManager.SpawnExplosion(field.pos, poisonFieldRadius, poisonFieldRadius, poisonFieldRadius, 0f, field.fieldDamage, null, relic.owner);
                }
            }
            else
            {
                field.effect.Stop();
                PoisonFields.Remove(field);
            }
        }
    }

    void OnPoisonStatus(Entity reciever, Debuff appliedStatus)
    {
        bool hasPoison = false;
        if (appliedStatus != null)
        {
            if (appliedStatus.debuffType == Debuff.DebuffTypes.Poison)
            {
                hasPoison = true;
            }

            if (hasPoison)
            {
                if (!fieldSpawnReady)
                {
                    fieldSpawnCooldownTime = Time.time + 0.1f;
                    fieldSpawnReady = true;
                }

                relic.owner.ApplyDebuff(debuffToApply, relic.owner);
            }
        }
    }

    void SpawnFieldOnHit(Entity reciever, DamageInstance dmg, bool heavy)
    {
        if (fieldSpawnReady && Time.time >= fieldSpawnCooldownTime)
        {
            PoisonField spawnedField = new PoisonField();

            spawnedField.fieldDamage = new DamageInstance(poisonFieldDamage)
            {
                HealthDamage = relic.owner.meleeWeapon.lightDamage.HealthDamage * poisonFieldDamageMultiplier * relic.owner.meleeWeapon.stats.DamageMultiplier * relic.owner.DamageMultiplier,
            };

            spawnedField.pos = reciever.Body.position;
            spawnedField.duration = fieldDuration;
            spawnedField.nextTimeToTick = Time.time + tickRate;
            spawnedField.effect = Instantiate(poisonFieldEffect, reciever.Body.position, Quaternion.identity).GetComponent<ParticleSystem>();
            spawnedField.effect.transform.localScale = Vector3.one * poisonFieldRadius;
            PoisonFields.Add(spawnedField);

            fieldSpawnReady = false;
        }
    }
}
