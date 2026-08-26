using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnDamageFieldOnDash : MonoBehaviour
{
    public Entity Owner;
    public float cooldown = 2f;
    public float nextTimeToUse = 0f;
    public float healthPercentThreshold = 0.6f;
    public float explosionRadius = 7f;
    public float explosionRadiusNear = 3f;
    public float explosionRadiusFar = 5f;
    public DamageInstance maxDamage;
    public float damageFallOff = 0.067f;

    public DebuffPreset debuffToApply;

    public GameObject spawnEffect;
    public float spawnDelay;
    public GameObject initialExplosionEffect;
    public GameObject fieldEffect;
    public GameObject tickEffect;
    public float FieldDamage;
    public float FieldStaggerDamage;
    public float Duration;
    public float tickRate;
    float nextTimeToTick;

    [Space(10)]
    public Transform[] spawnPoints;

    [System.Serializable]
    public class DamageField
    {
        public Vector3 pos;
        public float duration;
        public float nextTimeToTick;
        public ParticleSystem effect;
    }
    public List<DamageField> DamageFields = new List<DamageField>();

    public void OnDash()
    {
        if (Time.time < nextTimeToUse)
        {
            return;
        }
        if (((float)Owner.Health / (float)Owner.MaxHealth) < healthPercentThreshold)
        {
            foreach (Transform spawnPoint in spawnPoints)
            {
                StartCoroutine(SpawnField(spawnPoint.position));
            }
            nextTimeToUse = Time.time + cooldown;
        }
    }

    IEnumerator SpawnField(Vector3 spawnPos)
    {
        Instantiate(spawnEffect, spawnPos, Quaternion.identity);
        yield return new WaitForSeconds(spawnDelay);

        DamageInstance dmg = new DamageInstance(maxDamage);

        Instantiate(initialExplosionEffect, spawnPos, Quaternion.identity).transform.localScale = Vector3.one * explosionRadius;

        GameManager.SpawnExplosion(spawnPos, explosionRadius, explosionRadiusNear, explosionRadiusFar, damageFallOff, dmg, debuffToApply, Owner);

        DamageField spawnedField = new DamageField();
        spawnedField.pos = spawnPos;
        spawnedField.duration = Duration;
        spawnedField.nextTimeToTick = Time.time + tickRate;
        spawnedField.effect = Instantiate(fieldEffect, spawnPos, Quaternion.identity).GetComponent<ParticleSystem>();
        spawnedField.effect.transform.localScale = Vector3.one * explosionRadius;
        DamageFields.Add(spawnedField);
    }

    private void Update()
    {
        foreach (DamageField field in DamageFields.ToArray())
        {
            if (field.duration > 0f)
            {
                field.duration -= Time.deltaTime;

                if (Time.time >= field.nextTimeToTick)
                {
                    field.nextTimeToTick = Time.time + tickRate;

                    Instantiate(tickEffect, field.pos, Quaternion.identity).transform.localScale = Vector3.one * explosionRadius;

                    DamageInstance dmg = new DamageInstance(maxDamage);
                    dmg.HealthDamage = FieldDamage;
                    dmg.StaggerDamage = FieldStaggerDamage;
                    GameManager.SpawnExplosion(field.pos, explosionRadius, explosionRadius, explosionRadius, 1f, dmg, debuffToApply, Owner);
                }
            }
            else
            {
                field.effect.Stop();
                DamageFields.Remove(field);
            }
        }
    }
}
