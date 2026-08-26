using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MeleeWeapon : MonoBehaviour
{
    public Item weaponItem;
    public MeleeWeaponStats stats;
    public Entity Wielder;
    public AttackAnimationController animController;
    public AnimatorOverrideController overrideController;
    [System.Serializable]
    public enum WeaponTypes
    {
        Sword, Axe
    }
    public WeaponTypes WeaponType;
    public bool IsAttacking = false;
    public int comboIndex;

    [Header("Offhand GFX")]
    public GameObject offhandGFX;
    GameObject offhandTrails;

    [Header("Effects")]
    public GameObject Trails;
    public ParticleSystemPlayer particlePlayer;

    [Header("Light Attack")]
    public float lightStaminaCost;
    public DamageInstance lightDamage;
    public float lightWindUpSpeed = 0.5f;
    public float lightAttackSpeed = 1f;
    public Transform lightAttackPoint;
    public Vector3 lightHitboxSize;
    public GameObject hitEffect;
    public AudioClip lightAttackSound;
    public float lightAttackSoundVolume = 1f;
    List<Entity> enemiesHitByLight = new List<Entity>();

    [Header("Heavy Attack")]
    public float heavyStaminaCost;
    public DamageInstance heavyDamage;
    public float heavyWindUpSpeed = 0.75f;
    public float heavyAttackSpeed = 1.25f;
    public float heavyHitboxOffset;
    public Vector3 heavyHitboxSize;
    public ParticleSystem heavyAttackIndicator;

    [Header("Events")]
    public UnityEvent<Entity> OnTargetSevered;
    public UnityEvent<Vector3> OnTargetSplitInHalf;

    [Header("Impalement")]
    public bool animatedImpalementPos = false;
    public Transform impalementPos;
    public float impalementOffset = 0.25f;
    public string impalementAnim = "ImpaleThrow";
    public UnityEvent[] impalementEvents;
    public List<Entity> impaledEntities = new List<Entity>();
    public DebuffPreset[] impalementDebuffs;

    [HideInInspector]
    public List<GameObject> impalementEffects = new List<GameObject>();

    [System.Serializable]
    public class WeaponStatusBuildUp
    {
        public float buildUp;

        public float weaponToStatusDmgMultiplier;
        public DebuffPreset debuffToApply;
        public System.Action<Debuff> modifyInstance;
    }
    public List<WeaponStatusBuildUp> statusBuildUps = new List<WeaponStatusBuildUp>();
    List<WeaponStatusBuildUp> statusBuildUpsToRemove = new List<WeaponStatusBuildUp>();

    List<Entity> entitiesToRemove = new List<Entity>();

    private void Start()
    {
        Wielder.meleeWeapon = this;
        animController.selectedWeapon = this;

        if (offhandGFX != null)
        {
            GameObject spawnedGfx = Instantiate(offhandGFX, animController.offhandWeaponHolder);
            spawnedGfx.transform.localPosition = offhandGFX.transform.localPosition;
            spawnedGfx.transform.localRotation = offhandGFX.transform.localRotation;
            Vector3 scale = offhandGFX.transform.localScale;
            scale.x = -1f;
            spawnedGfx.transform.localScale = scale;

            Transform trailsChild = spawnedGfx.transform.Find(Trails.name);
            if (trailsChild != null)
            {
                offhandTrails = trailsChild.gameObject;
            }

            offhandGFX = spawnedGfx;
        }

        if (animatedImpalementPos)
        {
            impalementPos = animController.animatedImpalementPos;
        }
    }

    private void Update()
    {
        Wielder.meleeWeapon = this;
        animController.selectedWeapon = this;

        animController.anim.runtimeAnimatorController = overrideController;

        animController.anim.SetInteger("WeaponIndex", (int)WeaponType);
        animController.anim.SetInteger("WeaponComboIndex", comboIndex);

        animController.anim.SetFloat("LightWindUpSpeed", (1f / lightWindUpSpeed) * Wielder.AttackSpeedMultiplier * Wielder.GetMeleeStatMultiplier(Entity.MeleeStatType.AttackSpeed) * stats.LightAttackSpeedMultiplier);
        animController.anim.SetFloat("LightAttackSpeed", (1f / lightAttackSpeed) * Wielder.AttackSpeedMultiplier * Wielder.GetMeleeStatMultiplier(Entity.MeleeStatType.AttackSpeed) * stats.LightAttackSpeedMultiplier);

        animController.anim.SetFloat("HeavyWindUpSpeed", (1f / heavyWindUpSpeed) * Wielder.AttackSpeedMultiplier * Wielder.GetMeleeStatMultiplier(Entity.MeleeStatType.AttackSpeed) * stats.HeavyAttackSpeedMultiplier);
        animController.anim.SetFloat("HeavyAttackSpeed", (1f / heavyAttackSpeed) * Wielder.AttackSpeedMultiplier * Wielder.GetMeleeStatMultiplier(Entity.MeleeStatType.AttackSpeed) * stats.HeavyAttackSpeedMultiplier);

        entitiesToRemove.Clear();

        foreach (var item in impaledEntities)
        {
            if (item == null)
            {
                entitiesToRemove.Add(item);
            }
            else
            {
                item.Impaled = true;
                item.transform.position = impalementPos.position;
                if (item.charMovement != null)
                {
                    item.Body.rotation = impalementPos.rotation;
                }
            }
        }

        foreach (var item in entitiesToRemove)
        {
            impaledEntities.Remove(item);
        }

        if (animController.meleeHitboxActive)
        {
            DealLightDamage();
        }

        if (animController.OffhandMeleeHitboxActive)
        {
            DealLightDamageOffhand();
        }

        if (!animController.meleeHitboxActive && !animController.OffhandMeleeHitboxActive)
        {
            enemiesHitByLight.Clear();
        }

        if (Trails != null && offhandTrails != null)
        {
            offhandTrails.SetActive(Trails.activeSelf);
        }
        
        foreach (GameObject effect in impalementEffects)
        {
            if (effect != null)
            {
                effect.transform.position = impalementPos.position;
                effect.transform.rotation = impalementPos.rotation;
            }
        }
    }

    void DealLightDamage()
    {
        Collider[] enemiesHit = Physics.OverlapBox(
            lightAttackPoint.position + (lightAttackPoint.forward * stats.RangeBonus / 2f),
            lightHitboxSize + new Vector3(0f, 0f, stats.RangeBonus),
            lightAttackPoint.rotation);

        foreach (Collider item in enemiesHit)
        {
            Entity itemEntity = item.GetComponent<Entity>();
            if (itemEntity != null)
            {
                // Only hit if not same team, or if friendly fire is enabled
                bool isSameTeam = itemEntity.Team == Wielder.Team;
                bool allowHit = !isSameTeam || lightDamage.FriendlyFire != 0;

                if (allowHit && !enemiesHitByLight.Contains(itemEntity) && itemEntity != Wielder)
                {
                    Vector3 knockDir = new Vector3(
                        itemEntity.rigidBody.transform.position.x - Wielder.transform.position.x,
                        0f,
                        itemEntity.rigidBody.transform.position.z - Wielder.transform.position.z
                    ).normalized;

                    stats.UpdateConditionalBonus(itemEntity, lightDamage, false);
                    DamageInstance dmgInstance = new DamageInstance(lightDamage)
                    {
                        knockbackDir = knockDir,
                        knockbackAmount = lightDamage.knockbackAmount + stats.KnockbackBonus,
                        gunChargeMultiplier = lightDamage.gunChargeMultiplier + stats.GunChargeBonus,
                        FinalSeverenceDamageMultiplier = lightDamage.FinalSeverenceDamageMultiplier + stats.FinalSeverenceMultiplier,

                        HealthDamage = lightDamage.HealthDamage * stats.DamageMultiplier * Wielder.DamageMultiplier * Wielder.GetMeleeStatMultiplier(Entity.MeleeStatType.PhysicalDamage),
                        StaggerDamage = lightDamage.StaggerDamage * stats.StaggerDamageMultiplier * Wielder.DamageMultiplier * Wielder.GetMeleeStatMultiplier(Entity.MeleeStatType.StaggerDamage),
                        SeverenceDamage = lightDamage.SeverenceDamage * stats.SeverenceDamageMultiplier * Wielder.DamageMultiplier * Wielder.GetMeleeStatMultiplier(Entity.MeleeStatType.SeverenceDamage),
                    };

                    if ((itemEntity.EntityType == Entity.EntityTypes.Character && (itemEntity.Health - dmgInstance.HealthDamage) <= 0))
                    {
                        dmgInstance.knockbackAmount = (dmgInstance.knockbackAmount + 10f);
                    }
                    else if (itemEntity.EntityType == Entity.EntityTypes.Corpse)
                    {
                        dmgInstance.knockbackDir = (knockDir + new Vector3(0f, 0.5f, 0f)).normalized;
                        dmgInstance.knockbackAmount = (dmgInstance.knockbackAmount + 5f);
                    }

                    itemEntity.TakeDamage(dmgInstance, Wielder, false);
                    if (!itemEntity.GetImmunityStatus())
                    {
                        stats.CallOnDirectDamageDealt(itemEntity, dmgInstance, false);
                        ApplyBuiltUpStatuses(itemEntity, dmgInstance, false);
                        if (itemEntity.EntityType != Entity.EntityTypes.Corpse)
                            Instantiate(hitEffect, itemEntity.Body.transform.position, Quaternion.identity);
                    }

                    enemiesHitByLight.Add(itemEntity);
                }
            }
        }
    }

    void DealLightDamageOffhand()
    {
        if (animController == null || animController.weaponHolder == null || animController.offhandWeaponHolder == null || lightAttackPoint == null)
            return;

        Vector3 localOffset = animController.weaponHolder.InverseTransformPoint(lightAttackPoint.position);
        Quaternion localRotOffset = Quaternion.Inverse(animController.weaponHolder.rotation) * lightAttackPoint.rotation;

        Vector3 offhandWorldPos = animController.offhandWeaponHolder.TransformPoint(localOffset);
        Quaternion offhandWorldRot = animController.offhandWeaponHolder.rotation * localRotOffset;

        Collider[] enemiesHit = Physics.OverlapBox(
            offhandWorldPos + (offhandWorldRot * Vector3.forward) * (stats.RangeBonus / 2f),
            lightHitboxSize + new Vector3(0f, 0f, stats.RangeBonus),
            offhandWorldRot);

        foreach (Collider item in enemiesHit)
        {
            Entity itemEntity = item.GetComponent<Entity>();
            if (itemEntity != null)
            {
                // Only hit if not same team, or if friendly fire is enabled
                bool isSameTeam = itemEntity.Team == Wielder.Team;
                bool allowHit = !isSameTeam || lightDamage.FriendlyFire != 0;

                if (allowHit && !enemiesHitByLight.Contains(itemEntity) && itemEntity != Wielder)
                {
                    Vector3 knockDir = new Vector3(
                        itemEntity.rigidBody.transform.position.x - Wielder.transform.position.x,
                        0f,
                        itemEntity.rigidBody.transform.position.z - Wielder.transform.position.z
                    ).normalized;

                    stats.UpdateConditionalBonus(itemEntity, lightDamage, false);
                    DamageInstance dmgInstance = new DamageInstance(lightDamage)
                    {
                        knockbackDir = knockDir,
                        knockbackAmount = lightDamage.knockbackAmount + stats.KnockbackBonus,
                        gunChargeMultiplier = lightDamage.gunChargeMultiplier + stats.GunChargeBonus,
                        FinalSeverenceDamageMultiplier = lightDamage.FinalSeverenceDamageMultiplier + stats.FinalSeverenceMultiplier,

                        HealthDamage = lightDamage.HealthDamage * stats.DamageMultiplier * Wielder.DamageMultiplier * Wielder.GetMeleeStatMultiplier(Entity.MeleeStatType.PhysicalDamage),
                        StaggerDamage = lightDamage.StaggerDamage * stats.StaggerDamageMultiplier * Wielder.DamageMultiplier * Wielder.GetMeleeStatMultiplier(Entity.MeleeStatType.StaggerDamage),
                        SeverenceDamage = lightDamage.SeverenceDamage * stats.SeverenceDamageMultiplier * Wielder.DamageMultiplier * Wielder.GetMeleeStatMultiplier(Entity.MeleeStatType.SeverenceDamage),
                    };

                    if ((itemEntity.EntityType == Entity.EntityTypes.Character && (itemEntity.Health - dmgInstance.HealthDamage) <= 0))
                    {
                        dmgInstance.knockbackAmount = (dmgInstance.knockbackAmount + 10f);
                    }
                    else if (itemEntity.EntityType == Entity.EntityTypes.Corpse)
                    {
                        dmgInstance.knockbackDir = (knockDir + new Vector3(0f, 0.5f, 0f)).normalized;
                        dmgInstance.knockbackAmount = (dmgInstance.knockbackAmount + 5f);
                    }

                    itemEntity.TakeDamage(dmgInstance, Wielder, false);
                    if (!itemEntity.GetImmunityStatus())
                    {
                        stats.CallOnDirectDamageDealt(itemEntity, dmgInstance, false);
                        if (itemEntity.EntityType != Entity.EntityTypes.Corpse)
                            Instantiate(hitEffect, itemEntity.Body.transform.position, Quaternion.identity);
                    }

                    enemiesHitByLight.Add(itemEntity);
                }
            }
        }
    }

    public void LightAttack()
    {
        if (!IsAttacking && Wielder.Stamina > 0 && Wielder.Staggered == false)
        {
            IsAttacking = true;
            Wielder.UseStamina((lightStaminaCost / stats.lightStaminaEfficiency) / Wielder.StaminaEfficiencyMultiplier);
            animController.anim.SetTrigger("Attack");
            comboIndex++;
            if(comboIndex > 1)
            {
                comboIndex = 0;
            }
        }
    }

    public void HeavyAttack()
    {
        if (!IsAttacking && Wielder.Stamina >= ((heavyStaminaCost / stats.heavyStaminaEfficiency) / Wielder.StaminaEfficiencyMultiplier) && Wielder.Staggered == false)
        {
            IsAttacking = true;
            Wielder.UseStamina((heavyStaminaCost / stats.heavyStaminaEfficiency) / Wielder.StaminaEfficiencyMultiplier);
            animController.anim.SetTrigger("HeavyAttack");
            if(heavyAttackIndicator != null) heavyAttackIndicator.Play();
        }
    }

    public void DealHeavyDamage()
    {
        List<Entity> entitiesToRemove = new List<Entity>();
        bool ImpalementImmuneEnemyHit = false;
        bool ImpalableTargetHit = false;
        List<Entity> hitEntities = new List<Entity>();
        Collider[] targetsHit = Physics.OverlapBox(animController.targeting.objectToRotate.position + animController.targeting.objectToRotate.forward.normalized * (heavyHitboxOffset + stats.RangeBonus/2f), heavyHitboxSize + (Vector3.one * stats.RangeBonus), animController.targeting.objectToRotate.rotation);
        foreach (Collider target in targetsHit)
        {
            Entity targetEntity = target.GetComponent<Entity>();
            if(targetEntity != null)
            {
                if (Wielder.Team != targetEntity.Team && !hitEntities.Contains(targetEntity) && targetEntity.EntityType == Entity.EntityTypes.Character)
                {
                    hitEntities.Add(targetEntity);
                    if (targetEntity.GetImmunityStatus())
                    {
                        entitiesToRemove.Add(targetEntity);
                    }
                    if (targetEntity.ImpalementImmune && !targetEntity.Staggered)
                    {
                        ImpalementImmuneEnemyHit = true;
                    }
                    else
                    {
                        ImpalableTargetHit = true;
                    }
                }
            }
        }

        foreach (Entity hitEntity in hitEntities)
        {
            // Skip unimpalable targets - don't damage them at all
            if (hitEntity.ImpalementImmune && !hitEntity.Staggered)
            {
                continue;
            }

            Vector3 knockDir = new Vector3(hitEntity.rigidBody.transform.position.x - Wielder.transform.position.x, 0f, hitEntity.rigidBody.transform.position.z - Wielder.transform.position.z).normalized;
            stats.UpdateConditionalBonus(hitEntity, heavyDamage, true);
            DamageInstance dmgInstance = new DamageInstance(heavyDamage)
            {
                knockbackDir = knockDir,
                gunChargeMultiplier = heavyDamage.gunChargeMultiplier + stats.GunChargeBonus,
                FinalSeverenceDamageMultiplier = heavyDamage.FinalSeverenceDamageMultiplier + stats.FinalSeverenceMultiplier,

                HealthDamage = heavyDamage.HealthDamage * stats.DamageMultiplier * Wielder.DamageMultiplier * Wielder.GetMeleeStatMultiplier(Entity.MeleeStatType.PhysicalDamage),
                StaggerDamage = heavyDamage.StaggerDamage * stats.StaggerDamageMultiplier * Wielder.DamageMultiplier * Wielder.GetMeleeStatMultiplier(Entity.MeleeStatType.StaggerDamage),
                SeverenceDamage = heavyDamage.SeverenceDamage * stats.SeverenceDamageMultiplier * Wielder.DamageMultiplier * Wielder.GetMeleeStatMultiplier(Entity.MeleeStatType.SeverenceDamage),
            };

            if (!hitEntity.GetImmunityStatus())
            {
                foreach (DebuffPreset debuff in impalementDebuffs)
                {
                    hitEntity.ApplyDebuff(debuff, Wielder);
                }

                hitEntity.Impaled = true;
                if(hitEntity.EntityType != Entity.EntityTypes.Character) hitEntity.transform.parent = impalementPos;
                hitEntity.rigidBody.isKinematic = true;
                if (!impaledEntities.Contains(hitEntity))
                    impaledEntities.Add(hitEntity);
            }

            hitEntity.TakeDamage(dmgInstance, Wielder, true);

            if (!hitEntity.GetImmunityStatus())
            {
                stats.CallOnDirectDamageDealt(hitEntity, dmgInstance, true);
                ApplyBuiltUpStatuses(hitEntity, dmgInstance, true);
                Instantiate(hitEffect, hitEntity.Body.transform.position, Quaternion.identity);
            }
        }

        foreach (var item in entitiesToRemove)
        {
            hitEntities.Remove(item);
        }

        // Remove unimpalable targets from the hit list for animation/effects
        List<Entity> impaledTargets = new List<Entity>();
        foreach (Entity hitEntity in hitEntities)
        {
            if (!(hitEntity.ImpalementImmune && !hitEntity.Staggered))
            {
                impaledTargets.Add(hitEntity);
            }
        }

        if (impaledTargets.Count > 0)
        {
            animController.CrossFadeAnimation(impalementAnim);

            foreach (Entity hitEntity in impaledTargets)
            {
                if (hitEntity.impalementEffect != null)
                {
                    impalementEffects.Add(Instantiate(hitEntity.impalementEffect, hitEntity.Body.position, impalementPos.rotation));
                }
            }

            if (Wielder.Name == "Player")
            {
                StaggerPulse();
            }
        }
        else if (ImpalementImmuneEnemyHit && !ImpalableTargetHit)
        {
            Wielder.Stagger();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (lightAttackPoint != null)
        {
            Gizmos.color = Color.red;
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(lightAttackPoint.position, lightAttackPoint.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.forward * (stats != null ? stats.RangeBonus / 2f : 0f), lightHitboxSize + new Vector3(0f, 0f, stats != null ? stats.RangeBonus : 0f));
            Gizmos.matrix = oldMatrix;
        }

        // Draw offhand light attack hitbox
        if (animController != null && animController.weaponHolder != null && animController.offhandWeaponHolder != null && lightAttackPoint != null)
        {
            // Calculate local offset and rotation
            Vector3 localOffset = animController.weaponHolder.InverseTransformPoint(lightAttackPoint.position);
            Quaternion localRotOffset = Quaternion.Inverse(animController.weaponHolder.rotation) * lightAttackPoint.rotation;

            // Apply to offhand
            Vector3 offhandWorldPos = animController.offhandWeaponHolder.TransformPoint(localOffset);
            Quaternion offhandWorldRot = animController.offhandWeaponHolder.rotation * localRotOffset;

            Gizmos.color = Color.cyan;
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(offhandWorldPos, offhandWorldRot, Vector3.one);
            Gizmos.DrawWireCube(Vector3.forward * (stats != null ? stats.RangeBonus / 2f : 0f), lightHitboxSize + new Vector3(0f, 0f, stats != null ? stats.RangeBonus : 0f));
            Gizmos.matrix = oldMatrix;
        }

        if (animController != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(
                animController.targeting.objectToRotate.position + animController.targeting.objectToRotate.forward.normalized * heavyHitboxOffset,
                heavyHitboxSize
            );
        }
    }

    public void DropCorpses()
    {
        foreach (Entity impaledEntity in impaledEntities)
        {
            if (impaledEntity != null)
            {
                impaledEntity.Impaled = false;
                impaledEntity.transform.parent = null;
                impaledEntity.rigidBody.isKinematic = false;
                if(impaledEntity.charMovement != null)
                {
                    impaledEntity.Body.rotation = Quaternion.Euler(0f, impaledEntity.Body.rotation.y, 0f);
                }
            }
        }

        impaledEntities.Clear();
    }

    public void AddStatusBuildUp(DebuffPreset debuffPreset, float wpnToDmgMultiplier, float buildUp, System.Action<Debuff> modifyInstance = null)
    {
        bool hasMatchingBuildUp = false;
        foreach (WeaponStatusBuildUp wpnStatusBuildUp in statusBuildUps)
        {
            if (wpnStatusBuildUp.debuffToApply.DebuffName == debuffPreset.DebuffName)
            {
                hasMatchingBuildUp = true;
                wpnStatusBuildUp.buildUp += buildUp;
                wpnStatusBuildUp.modifyInstance = modifyInstance;
                break;
            }
        }

        if (!hasMatchingBuildUp)
        {
            WeaponStatusBuildUp newWpnStatusBuildUp = new WeaponStatusBuildUp
            {
                debuffToApply = debuffPreset,
                weaponToStatusDmgMultiplier = wpnToDmgMultiplier,
                buildUp = buildUp,
                modifyInstance = modifyInstance
            };
            statusBuildUps.Add(newWpnStatusBuildUp);
        }
    }

    void ApplyBuiltUpStatuses(Entity reciever, DamageInstance dmg, bool heavy)
    {
        if (reciever.EntityType == Entity.EntityTypes.Character)
        {
            foreach (WeaponStatusBuildUp wpnStatusBuildUp in statusBuildUps)
            {
                if (reciever.Health > 0 && wpnStatusBuildUp.buildUp >= 100f)
                {
                    reciever.ApplyDebuff(wpnStatusBuildUp.debuffToApply, Wielder, wpnStatusBuildUp.modifyInstance);
                    wpnStatusBuildUp.buildUp -= 100f;
                }
            }
        }

        statusBuildUpsToRemove.Clear();

        foreach (WeaponStatusBuildUp wpnStatusBuildUp in statusBuildUps)
        {
            if(wpnStatusBuildUp.buildUp <= 0f)
            {
                statusBuildUpsToRemove.Add(wpnStatusBuildUp);
            }
        }

        foreach (WeaponStatusBuildUp wpnStatusBuildUp in statusBuildUpsToRemove)
        {
            statusBuildUps.Remove(wpnStatusBuildUp);
        }
    }

    public void StaggerPulse()
    {
        Instantiate(Resources.Load("Effects/ImpalementStaggerPulse") as GameObject, Wielder.Body.position, Quaternion.identity).transform.localScale = new Vector3(10f, 0f, 10f);
        Collider[] staggerPulse = Physics.OverlapSphere(Wielder.Body.position, 10f);
        foreach (Collider pulseTarget in staggerPulse)
        {
            Entity pulseEntity = pulseTarget.GetComponent<Entity>();
            if (pulseEntity != null && pulseEntity.EntityType == Entity.EntityTypes.Character && pulseEntity.Team != Wielder.Team && !pulseEntity.ImpalementImmune)
            {
                pulseEntity.Stagger();
            }
        }
    }

    private void OnDestroy()
    {
        if (offhandGFX != null) Destroy(offhandGFX.gameObject);
    }
}
