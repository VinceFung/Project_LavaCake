using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float collisionRadius;

    public Entity Owner;

    public DamageInstance DamageInstance;
    public float Speed = 5f;
    public float LifeTime = 5f;
    public float Range = 20f;

    public GameObject HitFX;
    public GameObject[] Particles;
    public float particleLifeTimeOnDeath = 1f;

    Vector3 lastPos;
    Vector3 startPos;

    public bool Explosive;
    public float ExplosionRadius;
    public DamageInstance ExplosionDamageInstance;

    public bool countsAsHeavy = false;

    public bool piercing;
    public bool goesThroughWalls;

    public bool seeking;
    public float seekingAmount;
    public float seekingChangeRate;
    public float seekingAmountMax;
    public float seekingDisableRadius = 0f;
    public bool horizontalSeekRotation;
    public Transform seekingTarg;

    RaycastHit hitInfo;

    List<Entity> prevHitEntities = new List<Entity>();

    public DebuffPreset debuffToApply;

    [Header("Impalement Settings")]
    public bool canImpale;
    public GameObject impalementObject;
    public float impalementDuration = 3f;

    private void Update()
    {
        if (seeking && seekingTarg != null)
        {
            Vector3 desiredRotation = (seekingTarg.position - transform.position).normalized;
            if (horizontalSeekRotation)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(desiredRotation, Vector3.right), seekingAmount * Time.deltaTime);
            }
            else
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(desiredRotation, Vector3.up), seekingAmount * Time.deltaTime);
            }

            if (Vector3.Distance(transform.position, seekingTarg.position) <= seekingDisableRadius)
            {
                seeking = false;
            }

            if (seekingAmount < seekingAmountMax)
            {
                seekingAmount += seekingChangeRate * Time.deltaTime;
            }
            else
            {
                seekingAmount = seekingAmountMax;
            }
        }

        lastPos = transform.position;

        transform.Translate(0, 0, Speed * Time.deltaTime);

        Vector3 direction = (transform.position - lastPos).normalized;
        float distance = (transform.position - lastPos).magnitude;

        RaycastHit[] hits = Physics.SphereCastAll(lastPos, collisionRadius, direction, distance);
        foreach (RaycastHit hit in hits)
        {
            Entity hitEntity = hit.transform.GetComponent<Entity>();
            if (hitEntity != null)
            {
                bool entityAlreadyHit = false;
                foreach (Entity item in prevHitEntities)
                {
                    if (item == hitEntity)
                    {
                        entityAlreadyHit = true;
                        break;
                    }
                }

                if (!entityAlreadyHit && hitEntity != Owner)
                {
                    // Check if projectile can impale and entity is immune but not staggered - projectile dies
                    if (canImpale && hitEntity.ImpalementImmune && !hitEntity.Staggered)
                    {
                        hitInfo = hit;
                        Die();
                        return;
                    }

                    prevHitEntities.Add(hitEntity);

                    CharacterHitbox hitBodyPart = hit.collider.GetComponent<CharacterHitbox>();
                    Owner.meleeWeapon.stats.UpdateConditionalBonus(hitEntity, DamageInstance, countsAsHeavy);
                    DamageInstance dmg = new DamageInstance(DamageInstance);
                    if (Owner.meleeWeapon != null)
                    {
                        dmg.HealthDamage = DamageInstance.HealthDamage * Owner.DamageMultiplier * Owner.meleeWeapon.stats.DamageMultiplier;
                        dmg.StaggerDamage = DamageInstance.StaggerDamage * Owner.DamageMultiplier * Owner.meleeWeapon.stats.StaggerDamageMultiplier;
                        dmg.SeverenceDamage = DamageInstance.SeverenceDamage * Owner.DamageMultiplier * Owner.meleeWeapon.stats.SeverenceDamageMultiplier;
                    }
                    else
                    {
                        dmg.HealthDamage = DamageInstance.HealthDamage * Owner.DamageMultiplier;
                        dmg.StaggerDamage = DamageInstance.StaggerDamage * Owner.DamageMultiplier;
                        dmg.SeverenceDamage = DamageInstance.SeverenceDamage * Owner.DamageMultiplier;
                    }
                    hitEntity.TakeDamage(dmg, Owner, false);
                    applyDebuff(hitEntity);

                    if (canImpale && impalementObject != null && !hitEntity.Impaled)
                    {
                        bool canImpaleThroughImmunity = hitEntity.ImpalementImmune && hitEntity.Staggered;
                        bool canImpaleNormal = !hitEntity.ImpalementImmune;
                        
                        if (canImpaleThroughImmunity || canImpaleNormal)
                        {
                            SpawnImpalementObject(hitEntity);
                        }
                    }

                    hitInfo = hit;

                    if (!piercing)
                    {
                        Die();
                        return;
                    }
                }
            }
            else
            {
                if (!goesThroughWalls)
                {
                    hitInfo = hit;
                    Die();
                    return;
                }
            }
        }

        if (LifeTime <= 0)
        {
            Die();
        }
        else
        {
            LifeTime -= Time.deltaTime;
        }
    }

    void SpawnImpalementObject(Entity target)
    {
        GameObject impalementObj = Instantiate(impalementObject, target.Body.position, transform.rotation);
        impalementObj.transform.SetParent(target.Body);
        
        ImpalementEffect impalementEffect = impalementObj.GetComponent<ImpalementEffect>();
        if (impalementEffect == null)
        {
            impalementEffect = impalementObj.AddComponent<ImpalementEffect>();
        }
        
        impalementEffect.Initialize(target, impalementDuration);
    }

    void Die()
    {
        if (HitFX != null)
        {
            if (hitInfo.transform != null)
            {
                Instantiate(HitFX, hitInfo.point, Quaternion.identity);
            }
            else
            {
                Instantiate(HitFX, transform.position, Quaternion.identity);
            }
        }

        Destroy(gameObject);
    }

    void applyDebuff(Entity hitEntity)
    {
        if (hitEntity.Team == Owner.Team) return;

        hitEntity.ApplyDebuff(debuffToApply, Owner);
    }
}

public class ImpalementEffect : MonoBehaviour
{
    private Entity impaledEntity;
    private float duration;
    private float timer;

    public void Initialize(Entity entity, float impalementDuration)
    {
        impaledEntity = entity;
        duration = impalementDuration;
        timer = 0f;
    }

    private void Update()
    {
        if (impaledEntity == null)
        {
            Destroy(gameObject);
            return;
        }

        timer += Time.deltaTime;
        if (timer >= duration)
        {
            RemoveImpalement();
        }
    }

    private void RemoveImpalement()
    {

        Destroy(gameObject);
    }
}
