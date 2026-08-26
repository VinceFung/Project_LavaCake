using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Gun : MonoBehaviour
{
    [Header("References")]
    public Transform AmmoDisplayParent;
    public Entity Wielder;
    public GameObject meleeWeaponHolder;
    public GameObject gunHolder;
    public AttackAnimationController animController;
    public Transform FirePoint;
    public ParticleSystem muzzleFlash;
    public LineRenderer bulletTracer;
    public float bulletTracerDuration = 0.05f;
    public GameObject hitEffect;
    public AudioClip shootSound;
    public float shootSoundVolume = 1f;

    public LayerMask NpcMask;
    public LayerMask WallMask;

    public GameObject TargetIndicator;
    private List<GameObject> activeIndicators = new List<GameObject>();

    public bool IsArming;
    public bool Armed;

    [Header("Stats")]
    public int MaxAmmo = 2;
    public int Ammo = 2;
    public float AmmoChargeRequirement = 1000f;
    public float AmmoCharge = 0f;
    public float armSpeed = 0.5f;
    public float bulletRadius = 0.15f;
    public float fireRate = 1f;
    public DamageInstance Damage;
    public float Range;

    public DebuffPreset[] hitDebuffs;

    public List<Slider> spawnedAmmoDisplays = new List<Slider>();

    float nextTimeToFire;

    private void Update()
    {
        if (Ammo < MaxAmmo)
        {
            if (AmmoCharge >= AmmoChargeRequirement)
            {
                AmmoCharge -= AmmoChargeRequirement;
                Ammo++;
            }
        }
        else
        {
            AmmoCharge = 0f;
        }

        Armed = animController.IsArmed;

        if (!Wielder.meleeWeapon.IsAttacking)
        {
            if (IsArming)
            {
                meleeWeaponHolder.SetActive(false);
                gunHolder.SetActive(true);

                AddIndicatorsToNearbyNPCs();
            }
            else
            {
                meleeWeaponHolder.SetActive(true);
                gunHolder.SetActive(false);

                RemoveAllIndicators();
            }
        }
        animController.anim.SetBool("IsArming", IsArming);
        animController.anim.SetFloat("ArmSpeed", 1f / armSpeed);
        animController.anim.SetFloat("ShootAnimSpeed", fireRate);

        if (AmmoDisplayParent != null)
        {
            HandleAmmoDisplay();
        }
    }

    void HandleAmmoDisplay()
    {
        if (MaxAmmo > 0)
        {
            if (spawnedAmmoDisplays.Count != MaxAmmo)
            {
                foreach (Slider item in spawnedAmmoDisplays)
                {
                    Destroy(item.gameObject);
                }
                spawnedAmmoDisplays.Clear();

                for (int i = 0; i < MaxAmmo; i++)
                {
                    spawnedAmmoDisplays.Add(Instantiate(Resources.Load("AmmoSlider") as GameObject, AmmoDisplayParent).GetComponent<Slider>());
                }
            }

            for (int i = 0; i < MaxAmmo; i++)
            {
                if (i < Ammo)
                {
                    spawnedAmmoDisplays[i].maxValue = AmmoChargeRequirement;
                    spawnedAmmoDisplays[i].value = AmmoChargeRequirement;
                }
                else if (i == Ammo)
                {
                    spawnedAmmoDisplays[i].maxValue = AmmoChargeRequirement;
                    spawnedAmmoDisplays[i].value = AmmoCharge;
                }
                else
                {
                    spawnedAmmoDisplays[i].maxValue = AmmoChargeRequirement;
                    spawnedAmmoDisplays[i].value = 0f;
                }
            }
        }
    }

    public void Shoot()
    {
        if (Armed && Ammo > 0)
        {
            if (Time.time >= nextTimeToFire)
            {
                nextTimeToFire = Time.time + fireRate;
            }
            else
            {
                return;
            }

            Ammo--;
            animController.anim.SetTrigger("Shoot");
            if (muzzleFlash != null) muzzleFlash.Play();
            SoundFXManager.Instance.PlaySoundClip(shootSound, transform.position, shootSoundVolume, Random.Range(0.9f, 1.1f));

            RaycastHit hit;
            bulletTracer.SetPosition(0, FirePoint.position);

            if (Physics.SphereCast(FirePoint.position, bulletRadius, animController.targeting.objectToRotate.forward * Range, out hit))
            {
                Entity hitEntity = hit.transform.GetComponent<Entity>();
                if (hitEntity != null)
                {
                    foreach (DebuffPreset debuff in hitDebuffs)
                    {
                        hitEntity.ApplyDebuff(debuff, Wielder);
                    }

                    DamageInstance dmg = new DamageInstance(Damage)
                    {
                        HealthDamage = Damage.HealthDamage * Wielder.DamageMultiplier,
                        SeverenceDamage = Damage.SeverenceDamage * Wielder.DamageMultiplier,
                        StaggerDamage = Damage.StaggerDamage * Wielder.DamageMultiplier,
                        knockbackDir = Wielder.Body.forward.normalized
                    };

                    hitEntity.TakeDamage(dmg, Wielder, false);
                    TryDetonateUnstable(hitEntity);
                    if (hitEntity.Team != Wielder.Team)
                    {
                        Wielder.OnGunDamageDealt.Invoke(hitEntity);
                    }
                }
                bulletTracer.SetPosition(1, hit.point);
                Instantiate(hitEffect, hit.point, Quaternion.identity);
            }
            else
            {
                bulletTracer.SetPosition(1, FirePoint.position + FirePoint.forward * Range);
            }

            StartCoroutine(RenderBulletTracer());
        }
    }

    IEnumerator RenderBulletTracer()
    {
        bulletTracer.gameObject.SetActive(true);
        yield return new WaitForSeconds(bulletTracerDuration);
        bulletTracer.gameObject.SetActive(false);
    }

    private void AddIndicatorsToNearbyNPCs()
    {
        // Iterate through all nearby NPCs
        Collider[] NPCsCaught = Physics.OverlapSphere(Wielder.Body.position, Range, NpcMask);

        // Track which NPCs already have indicators
        List<GameObject> indicatorsToKeep = new List<GameObject>();

        foreach (Collider npc in NPCsCaught)
        {
            Entity entity = npc.GetComponent<Entity>();
            if (entity != null && entity != Wielder && entity.Team != Wielder.Team)
            {
                // Perform a Line of Sight (LOS) check using wall mask
                Vector3 directionToNPC = (entity.Body.position - Wielder.Body.position).normalized;
                float distanceToNPC = Vector3.Distance(Wielder.Body.position, entity.Body.position);
                
                // Check height difference - skip if too far vertically
                Vector3 heightDiff = entity.Body.position - Wielder.Body.position;
                if (Mathf.Abs(heightDiff.y) > 3f) continue; // Skip NPCs too far above/below

                // Only check for walls blocking the line of sight
                if (Physics.Raycast(Wielder.Body.position, directionToNPC, distanceToNPC, WallMask))
                {
                    // If the raycast hits a wall, skip this NPC (line of sight is blocked)
                    continue;
                }

                // Check if this NPC already has an indicator
                GameObject existingIndicator = activeIndicators.Find(indicator =>
                    indicator != null && indicator.transform.parent == entity.Body);

                if (existingIndicator == null)
                {
                    // Instantiate a new indicator and attach it to the NPC
                    if (entity.Body != null) // Ensure the entity's body is valid
                    {
                        GameObject indicator = Instantiate(TargetIndicator, new Vector3(npc.transform.position.x, 0.8f, npc.transform.position.z), Quaternion.identity);
                        indicator.transform.SetParent(entity.Body); // Attach to NPC's body
                        indicator.transform.localScale = Vector3.one;
                        activeIndicators.Add(indicator);
                        indicatorsToKeep.Add(indicator);
                    }
                }
                else
                {
                    // Keep the existing indicator
                    indicatorsToKeep.Add(existingIndicator);
                }
            }
        }

        // Remove indicators for NPCs that are no longer in range or destroyed
        foreach (GameObject indicator in activeIndicators)
        {
            if (indicator == null || !indicatorsToKeep.Contains(indicator) || indicator.transform.parent == null)
            {
                Destroy(indicator);
            }
        }

        // Update the activeIndicators list to only include the indicators we are keeping
        activeIndicators = indicatorsToKeep;
    }

    private void RemoveAllIndicators()
    {
        foreach (GameObject indicator in activeIndicators)
        {
            if (indicator != null)
            {
                Destroy(indicator);
            }
        }
        activeIndicators.Clear();
    }

    void TryDetonateUnstable(Entity entity)
    {
        bool hasUnstable = false;
        DamageInstance unstableDamage = null;
        List<Debuff> debuffsToRemove = new List<Debuff>();
        foreach (Debuff debuff in entity.activeDebuffs.ToArray())
        {
            if (debuff.debuffType == Debuff.DebuffTypes.Unstable)
            {
                debuffsToRemove.Add(debuff);
                unstableDamage = new DamageInstance(debuff.Damage);
                hasUnstable = true;
            }
        }

        foreach (Debuff debuff in debuffsToRemove)
        {
            debuff.instantiatedEffect.transform.SetParent(null, true);
            debuff.instantiatedEffect.transform.localScale = new Vector3(1, 1, 1);
            debuff.instantiatedEffect.GetComponent<ParticleSystem>().Stop();

            entity.activeDebuffs.Remove(debuff);
        }

        if (hasUnstable)
        {
            Instantiate(Resources.Load("Effects/UnstableExplosion") as GameObject, entity.Body.position, Quaternion.identity).transform.localScale = Vector3.one * 4f;
            GameManager.SpawnExplosion(entity.Body.position, 4f, 3f, 3.5f, 0f, unstableDamage, null, Wielder);
        }
    }

    void OnDestroy()
    {
        RemoveAllIndicators();
        foreach (Slider item in spawnedAmmoDisplays)
        {
            if(item != null) Destroy(item.gameObject);
        }
    }
}
