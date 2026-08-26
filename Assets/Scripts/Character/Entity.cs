using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Entity : MonoBehaviour
{
    #region Variables
    public enum EntityTypes
    {
        Character, Corpse
    }
    public EntityTypes EntityType = EntityTypes.Character;
    public string Team = "Enemy";
    public string Name;
    public float XpReward = 0f;
    public int MaxHealth = 100;
    public int Health = 100;
    public float damageCooldown;
    float dmgCooldownTime;

    public float MaxStamina = 100f;
    public float Stamina = 100f;
    public float staminaRegenSpeed = 50;
    public float staminaRegenDelay = 2f;
    [HideInInspector]
    public float staminaDelayTime;

    public float StaggerRequirement = 20f;
    public float staggerDamageTaken = 0f;
    public float staggerCooldown = 1f;
    public float staggerDuration = 1f;
    float staggerCooldownTime;
    float autoUnstaggerTimeStamp;
    public bool ImpalementImmune;
    public bool Staggered;
    public bool Impaled;
    public bool DashImmunity;
    [Space(10f)]

    public float severenceDamageRequirement = 250f;
    public float severenceDamageTaken;

    [Header("References")]
    public CharacterMovement charMovement;
    public NpcCombatAI charCombatAI;
    public MeleeWeapon meleeWeapon;
    public Gun entityGun;
    public GameObject CorpseObject;
    public float corpseHealth = 1000f;
    public GameObject CorpseTopObject;
    public float corpseTopHealth = 600;
    public GameObject CorpseBottomObject;
    public float corpseBottemHealth = 750f;
    public GameObject SeveredChunkObject;
    public float severedChunkHealth = 600f;
    public Transform Body;
    public Rigidbody rigidBody;
    public GameObject[] hitEffects;
    public GameObject impalementEffect;

    [Header("UI")]
    public TextMeshProUGUI nameText;
    public Slider InstantHealthBar;
    public Slider DelayedHealthBar;
    public Slider InstantStaminaBar;
    public Slider DelayedStaminaBar;
    public Transform BuffBar;
    public bool displayBasicDebuffs;
    public GameObject healthBarDeathEffect;

    [Header("Events")]
    public UnityEvent OnDeath;
    public UnityEvent OnDamageTaken;
    public UnityEvent OnAttackDodged;
    public UnityEvent OnStagger;
    public UnityEvent OnSevered;
    public UnityEvent<Entity> OnKill;
    public UnityEvent<Entity> OnGunDamageDealt;
    public UnityEvent<Entity, Debuff> OnStatusApplied;
    public UnityEvent OnAbilityCast;

    bool corpseSpawned = false;

    [Header("Stats")]
    public int Strength = 0; //Scales Melee Physical Damage upto 40%, Scales Stagger Damage up to 20%
    public int Dexterity = 0; //Scales Attack Speed up to 20%, Scales Severence up to 20%
    public int Agility = 0; //Scales Movement Speed up to 20%, Scales Stamina Regen up and Efficiency to 20%
    public int Hunger = 0; //Scales Gun Ammo Charge up to 20%, Scales Resource Gain by 20%
    public int Lucidity = 0; //Scales Ability Damage by 20%, Scales Status Damage by 20%

    [Header("Multipliers")]

    public float baseAbilityDamageMultiplier = 1.0f;
    public float baseResourceGainMultiplier = 1.0f;

    public float baseDamageInputMultiplier = 1.0f;
    public float baseDamageMultiplier = 1.0f;
    public float baseHealingMultiplier = 1.0f;

    public float baseAttackSpeedMultiplier = 1.0f;

    public float baseMovementSpeedMultiplier = 1.0f;

    public float baseStatusBuildUpMultiplier = 1.0f;
    public float baseStatusDamageMultiplier = 1.0f;

    public float baseGunChargeMultiplier = 1.0f;

    public float baseStaminaEfficiencyMultiplier = 1.0f;

    [Space(10)]
    public float DamageInputMultiplier = 1.0f;
    public float DamageMultiplier = 1.0f;

    public float AbilityDamageMultiplier = 1.0f;
    public float ResourceGainMultiplier = 1.0f;

    public float HealingMultiplier = 1.0f;
    public float AttackSpeedMultiplier = 1.0f;
    public float MovementSpeedMultiplier = 1.0f;
    public float StatusBuildUpMultiplier = 1.0f;
    public float StatusDamageMultiplier = 1.0f;

    public float GunChargeMultiplier = 1.0f;

    public float StaminaEfficiencyMultiplier = 1.0f;

    public List<Debuff> activeDebuffs = new List<Debuff>();

    public float timeSinceDirectDamageTaken;

    #endregion

    #region Health

    void Update()
    {
        timeSinceDirectDamageTaken += Time.deltaTime;

        if (Health > MaxHealth)
        {
            Health = MaxHealth;
        }

        if (Stamina > MaxStamina)
        {
            Stamina = MaxStamina;
        }
        else if (Stamina < 0)
        {
            Stamina = 0;
        }

        if (Stamina < MaxStamina && Time.time >= staminaDelayTime)
        {
            Stamina += staminaRegenSpeed * Time.deltaTime;
        }

        if (Health <= 0)
        {
            Die();
        }

        if (EntityType == EntityTypes.Character)
        {
            if (transform.position.y <= -10f)
            {
                SpawnCorpse(CorpseObject, Vector3.down, null, false, corpseHealth);
                Die();
            }
        }
        else if (EntityType == EntityTypes.Corpse)
        {
            if (transform.position.y <= -20f)
            {
                Die();
            }
        }


        if (staggerDamageTaken >= StaggerRequirement)
        {
            Stagger();
        }

        HandleDebuffs();
        HandleUI();

        if (Time.time >= autoUnstaggerTimeStamp && !Impaled)
        {
            setStaggerState(0);
        }
    }

    void HandleUI()
    {
        if (BuffBar != null) UpdateBuffDisplays(activeDebuffs);

        if (InstantHealthBar != null)
        {
            InstantHealthBar.maxValue = MaxHealth;
            InstantHealthBar.value = Health;
        }

        if (DelayedHealthBar != null)
        {
            DelayedHealthBar.maxValue = MaxHealth;
            if (Health < DelayedHealthBar.value)
            {
                DelayedHealthBar.value -= Time.deltaTime * MaxHealth / 1.5f;
            }
            else
            {
                DelayedHealthBar.value = Health;
            }
        }

        if (InstantStaminaBar != null)
        {
            InstantStaminaBar.maxValue = MaxStamina;
            InstantStaminaBar.value = Stamina;
        }

        if (DelayedStaminaBar != null)
        {
            DelayedStaminaBar.maxValue = MaxStamina;
            if (Stamina < DelayedStaminaBar.value)
            {
                DelayedStaminaBar.value -= Time.deltaTime * MaxStamina / 1.5f;
            }
            else
            {
                DelayedStaminaBar.value = Stamina;
            }
        }

        if (nameText != null)
        {
            nameText.text = Name;
        }
    }

    public void TakeDamage(DamageInstance dmg, Entity Attacker, bool impalementAttack)
    {
        if (Attacker != null)
        {
            if (DashImmunity && Attacker.Team != Team)
            {
                OnAttackDodged.Invoke();
            }
        }
        else
        {
            if (DashImmunity)
            {
                OnAttackDodged.Invoke();
            }
        }

        if (GetImmunityStatus()) return;

        dmgCooldownTime = Time.time + damageCooldown;

        if (Attacker != null)
        {
            if (Attacker.Team != Team)
            {
                InflictDamage(dmg, false);
                if (EntityType == EntityTypes.Character && Attacker.entityGun != null)
                {
                    Attacker.entityGun.AmmoCharge += dmg.HealthDamage * dmg.gunChargeMultiplier * Attacker.GunChargeMultiplier;
                }
            }
            else if (dmg.FriendlyFire != 0)
            {
                InflictDamage(dmg, true);
            }
        }
        else
        {
            InflictDamage(dmg, false);
        }


        if (severenceDamageTaken >= severenceDamageRequirement && Health > 0)
        {
            if (EntityType == EntityTypes.Character)
            {
                SpawnCorpse(SeveredChunkObject, dmg.knockbackDir * dmg.knockbackAmount, Attacker, impalementAttack, severedChunkHealth);
            }

            OnSevered.Invoke();
            if (Attacker.meleeWeapon != null) Attacker.meleeWeapon.OnTargetSevered.Invoke(this);
            severenceDamageTaken = 0f;
        }

        if (Attacker != null && ImpalementImmune)
        {
            if (staggerDamageTaken >= StaggerRequirement)
            {
                Attacker.Stamina += (Attacker.MaxStamina * 0.3f) + 30f;
                Attacker.staminaDelayTime = 0f;
            }
        }

        if (Health <= 0)
        {
            if (Attacker != null)
            {
                Attacker.OnKill.Invoke(this);
            }
            severenceDamageTaken += dmg.SeverenceDamage * dmg.FinalSeverenceDamageMultiplier;
            bool splitInHalf = false;
            if (severenceDamageTaken >= severenceDamageRequirement) splitInHalf = true;

            if (corpseSpawned == false && EntityType == EntityTypes.Character)
            {
                corpseSpawned = true;
                if (!splitInHalf)
                {
                    SpawnCorpse(CorpseObject, dmg.knockbackDir * dmg.knockbackAmount, Attacker, impalementAttack, corpseHealth);
                }
                else
                {
                    OnSevered.Invoke();
                    if (Attacker.meleeWeapon != null) Attacker.meleeWeapon.OnTargetSplitInHalf.Invoke(Body.position);
                    int randDir = (2 * Random.Range(0, 2)) - 1;
                    Vector3 knockDirA = Quaternion.Euler(0f, 37.5f * randDir, 0f) * dmg.knockbackDir;
                    Vector3 knockDirB = Quaternion.Euler(0f, -37.5f * randDir, 0f) * dmg.knockbackDir;
                    SpawnCorpse(CorpseTopObject, knockDirA * dmg.knockbackAmount, Attacker, impalementAttack, corpseTopHealth);
                    SpawnCorpse(CorpseBottomObject, knockDirB * dmg.knockbackAmount, Attacker, impalementAttack, corpseBottemHealth);
                }
            }
        }
        else
        {
            if (EntityType == EntityTypes.Character)
            {
                // Impalement logic removed from here. Now handled only in MeleeWeapon.DealHeavyDamage.
            }
        }
    }

    void InflictDamage(DamageInstance dmg, bool friendlyFireActive)
    {
        float damageMultiplier = dmg.Multiplier;
        if (friendlyFireActive)
        {
            damageMultiplier *= dmg.FriendlyFire;
        }

        if (charMovement != null)
        {
            charMovement.appliedForces.Add(dmg.knockbackDir * dmg.knockbackAmount * damageMultiplier / 4f);
        }
        else if (rigidBody != null)
        {
            rigidBody.AddForce(dmg.knockbackDir * dmg.knockbackAmount * damageMultiplier, ForceMode.Impulse);
        }

        int finalDamage = Mathf.RoundToInt(dmg.HealthDamage * damageMultiplier * DamageInputMultiplier);
        Health -= finalDamage;
        foreach (Debuff debuff in activeDebuffs)
        {
            debuff.debuffDamageTaken += finalDamage;
        }

        if (EntityType == EntityTypes.Character)
        {
            Vector3 randPos = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            Instantiate(Resources.Load("DamageText") as GameObject, (Vector3)transform.position + randPos, Quaternion.identity).GetComponent<TextMeshPro>().text = finalDamage.ToString();
        }

        severenceDamageTaken += dmg.SeverenceDamage * damageMultiplier * DamageInputMultiplier;

        if (Time.time >= staggerCooldownTime) staggerDamageTaken += dmg.StaggerDamage * damageMultiplier * DamageInputMultiplier;

        if (dmg.DamageType == DamageInstance.DamageTypes.DirectDamage) timeSinceDirectDamageTaken = 0f;

        OnDamageTaken.Invoke();

        foreach (GameObject effect in hitEffects)
        {
            Instantiate(effect, Body.position, Quaternion.identity);
        }
    }

    public void UseStamina(float staminaUsed)
    {
        Stamina -= staminaUsed;
        staminaDelayTime = Time.time + staminaRegenDelay;
    }

    void SpawnCorpse(GameObject Corpse, Vector3 knockbackDir, Entity Attacker, bool impalementAttack, float corpseHealth)
    {
        if (Corpse == null) return;
        GameObject corpseObj = Instantiate(Corpse, Body.position, Quaternion.Euler(0f, Body.rotation.y, Body.rotation.z));
        Entity corpseEntity = corpseObj.GetComponent<Entity>();
        corpseEntity.MaxHealth = (int)corpseHealth;
        corpseEntity.Health = corpseEntity.MaxHealth;

        foreach (Debuff activeDebuff in activeDebuffs)
        {
            if (activeDebuff.transfersToCorpse)
            {
                Debuff debuff = new Debuff(activeDebuff.instancePreset, this);
                debuff.oneTimeEffectActivated = false;
                debuff.instantiatedEffect = null;
                corpseEntity.activeDebuffs.Add(debuff);
            }
        }

        if (!impalementAttack)
        {
            float power = knockbackDir.magnitude;
            if (corpseEntity != null) corpseEntity.rigidBody.AddForce((knockbackDir + new Vector3(0f, 0.5f, 0f)).normalized * power, ForceMode.Impulse);
        }
        else
        {
            if (Attacker != null)
            {
                Attacker.meleeWeapon.impaledEntities.Add(corpseEntity);
                corpseEntity.rigidBody.isKinematic = true;
                corpseObj.transform.parent = Attacker.meleeWeapon.impalementPos;
                corpseObj.transform.localPosition = new Vector3(0, 0, (Attacker.meleeWeapon.impaledEntities.Count - 1) * Attacker.meleeWeapon.impalementOffset);
                corpseEntity.transform.localRotation = Quaternion.identity;
                corpseEntity.transform.Rotate(Attacker.meleeWeapon.impalementPos.forward, Random.Range(-10f, 10f));
                Collider[] colliders = corpseEntity.GetComponentsInChildren<Collider>();
            }
        }
    }

    public void Die()
    {
        OnDeath.Invoke();

        if (meleeWeapon != null) meleeWeapon.DropCorpses();

        if (InstantHealthBar != null && healthBarDeathEffect != null)
        {
            Instantiate(healthBarDeathEffect, InstantHealthBar.transform.position, Quaternion.identity);
        }

        activeDisplays.Clear();

        UnitManager.Instance.playerLevel.GainXp(XpReward);

        TriggerDebuffDeathEffects();
        Destroy(gameObject);
    }

    public void Stagger()
    {
        autoUnstaggerTimeStamp = Time.time + staggerDuration + 0.9f;
        if (meleeWeapon != null) meleeWeapon.DropCorpses();
        setStaggerState(1);
        OnStagger.Invoke();
        staggerDamageTaken = 0f;
        staggerCooldownTime = Time.time + staggerCooldown;
    }

    public void setStaggerState(int state)
    {
        if (state == 1)
        {
            Staggered = true;
        }
        else
        {
            Staggered = false;
        }
    }

    public bool GetImmunityStatus()
    {
        if (DashImmunity || (damageCooldown > 0f && Time.time <= dmgCooldownTime))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void SpawnObjectOnBody(GameObject obj)
    {
        Instantiate(obj, Body.position, Quaternion.identity);
    }
    #endregion

    #region Multipliers
    // Place multiplier/stat-related functions here for new stats system
    public enum MeleeStatType {
        Strength,
        Dexterity,
        PhysicalDamage,
        StaggerDamage,
        SeverenceDamage,
        AttackSpeed
    }

    /// <summary>
    /// Returns the stat multiplier for a given melee stat type.
    /// </summary>
    public float GetMeleeStatMultiplier(MeleeStatType type)
    {
        switch (type)
        {
            case MeleeStatType.PhysicalDamage:
                // Strength scales physical (health) damage
                return GetStatBonus(Strength, 0.4f, 0.5f, 0.55f, 0.05f);
            case MeleeStatType.StaggerDamage:
                // Strength scales stagger damage
                return GetStatBonus(Strength, 0.2f, 0.3f, 0.35f, 0.05f);
            case MeleeStatType.SeverenceDamage:
                // Dexterity scales severence damage
                return GetStatBonus(Dexterity, 0.2f, 0.3f, 0.35f, 0.05f);
            case MeleeStatType.AttackSpeed:
                // Dexterity scales attack speed
                return GetStatBonus(Dexterity, 0.2f, 0.3f, 0.35f, 0.05f);
            default:
                return 1f;
        }
    }

    /// <summary>
    // Removed GetMeleeDamageMultiplier and GetMeleeAttackSpeedMultiplier in favor of GetMeleeStatMultiplier
    float GetStatBonus(int stat, float bonus1, float bonus2, float bonus3, float beyondPer100)
    {
        if (stat <= 100)
            return 1f + bonus1 * (stat / 100f);
        else if (stat <= 200)
            return 1f + bonus1 + (bonus2 - bonus1) * ((stat - 100) / 100f);
        else if (stat <= 300)
            return 1f + bonus2 + (bonus3 - bonus2) * ((stat - 200) / 100f);
        else
            return 1f + bonus3 + beyondPer100 * ((stat - 300) / 100f);
    }

    /// <summary>
    /// Applies all stat multipliers. Call this at the start of HandleDebuffs().
    /// </summary>
    void ApplyStatMultipliers()
    {
        // Reset to base values
        DamageInputMultiplier = baseDamageInputMultiplier;
        DamageMultiplier = baseDamageMultiplier;
        HealingMultiplier = baseHealingMultiplier;
        AttackSpeedMultiplier = baseAttackSpeedMultiplier;
        MovementSpeedMultiplier = baseMovementSpeedMultiplier;
        StatusBuildUpMultiplier = baseStatusBuildUpMultiplier;
        StatusDamageMultiplier = baseStatusDamageMultiplier;
        GunChargeMultiplier = baseGunChargeMultiplier;
        StaminaEfficiencyMultiplier = baseStaminaEfficiencyMultiplier;
        AbilityDamageMultiplier = baseAbilityDamageMultiplier;
        ResourceGainMultiplier = baseResourceGainMultiplier;

        MovementSpeedMultiplier *= GetStatBonus(Agility, 0.2f, 0.3f, 0.35f, 0.05f);
        StaminaEfficiencyMultiplier *= GetStatBonus(Agility, 0.2f, 0.30f, 0.35f, 0.05f);

        GunChargeMultiplier *= GetStatBonus(Hunger, 0.2f, 0.3f, 0.35f, 0.05f);
        ResourceGainMultiplier *= GetStatBonus(Hunger, 0.2f, 0.3f, 0.35f, 0.05f);

        AbilityDamageMultiplier *= GetStatBonus(Lucidity, 0.2f, 0.3f, 0.35f, 0.05f);
        StatusDamageMultiplier *= GetStatBonus(Lucidity, 0.2f, 0.3f, 0.35f, 0.05f);
    }
    #endregion

    #region Debuff
    public void ApplyDebuff(DebuffPreset preset, Entity applier, System.Action<Debuff> modifyInstance = null)
    {
        if (preset == null) return;

        bool hasMatchingDebuff = false;
        Debuff matchingDebuff = null;
        foreach (Debuff debuff in activeDebuffs.ToArray())
        {
            if (debuff.DebuffName == preset.DebuffName)
            {
                hasMatchingDebuff = true;
                matchingDebuff = debuff;
                break;
            }
        }

        if (hasMatchingDebuff)
        {
            switch (preset.stackType)
            {
                case Debuff.StackTypes.DoesNotStack:
                    if (modifyInstance != null) modifyInstance(matchingDebuff);
                    break;

                case Debuff.StackTypes.RefreshDuration:
                    matchingDebuff.Duration = preset.Duration;
                    if (modifyInstance != null) modifyInstance(matchingDebuff);
                    break;

                case Debuff.StackTypes.IncreaseDuration:
                    matchingDebuff.Duration += preset.Duration;
                    if (modifyInstance != null) modifyInstance(matchingDebuff);
                    break;

                case Debuff.StackTypes.IncreasePowerAndRefresh:
                    matchingDebuff.Duration = preset.Duration;
                    if (matchingDebuff.Stacks < preset.MaxStacks)
                    {
                        matchingDebuff.Stacks += 1;
                        matchingDebuff.debuffDamageInputMultiplier += preset.debuffDamageInputMultiplier;
                        matchingDebuff.debuffDamageMultiplier += preset.debuffDamageMultiplier;
                        matchingDebuff.debuffHealingMultiplier += preset.debuffHealingMultiplier;
                        matchingDebuff.debuffAttackSpeedMultiplier += preset.debuffAttackSpeedMultiplier;
                        matchingDebuff.debuffMovementSpeedMultiplier += preset.debuffMovementSpeedMultiplier;
                        matchingDebuff.debuffStatusBuildUpMultiplier += preset.debuffStatusBuildUpMultiplier;
                        matchingDebuff.debuffStatusDamageMultiplier += preset.debuffStatusDamageMultiplier;
                        matchingDebuff.debuffGunChargeMultiplier += preset.debuffGunChargeMultiplier;
                        matchingDebuff.debuffStaminaEfficiencyMultiplier += preset.debuffStaminaEfficiencyMultiplier;
                    }
                    else
                    {
                        matchingDebuff.Stacks = preset.MaxStacks;
                    }
                    break;

                case Debuff.StackTypes.IncreasePowerAndDuration:
                    matchingDebuff.Duration += preset.Duration;
                    if (matchingDebuff.Stacks < preset.MaxStacks)
                    {
                        matchingDebuff.Stacks += 1;
                        matchingDebuff.debuffDamageInputMultiplier += preset.debuffDamageInputMultiplier;
                        matchingDebuff.debuffDamageMultiplier += preset.debuffDamageMultiplier;
                        matchingDebuff.debuffHealingMultiplier += preset.debuffHealingMultiplier;
                        matchingDebuff.debuffAttackSpeedMultiplier += preset.debuffAttackSpeedMultiplier;
                        matchingDebuff.debuffMovementSpeedMultiplier += preset.debuffMovementSpeedMultiplier;
                        matchingDebuff.debuffStatusBuildUpMultiplier += preset.debuffStatusBuildUpMultiplier;
                        matchingDebuff.debuffStatusDamageMultiplier += preset.debuffStatusDamageMultiplier;
                        matchingDebuff.debuffGunChargeMultiplier += preset.debuffGunChargeMultiplier;
                        matchingDebuff.debuffStaminaEfficiencyMultiplier += preset.debuffStaminaEfficiencyMultiplier;
                    }
                    else
                    {
                        matchingDebuff.Stacks = preset.MaxStacks;
                    }
                    break;
            }

            if (applier != this) applier.OnStatusApplied.Invoke(this, matchingDebuff);
        }
        else
        {
            Debuff newDebuff = new Debuff(preset, applier);
            if (modifyInstance != null) modifyInstance(newDebuff);
            activeDebuffs.Add(newDebuff);

            if (applier != this) applier.OnStatusApplied.Invoke(this, newDebuff);
        }
    }

    void HandleDebuffs()
    {
    ApplyStatMultipliers();


        foreach (Debuff debuff in activeDebuffs.ToArray())
        {
            DamageInputMultiplier += debuff.debuffDamageInputMultiplier;
            DamageMultiplier += debuff.debuffDamageMultiplier;
            HealingMultiplier += debuff.debuffHealingMultiplier;
            AttackSpeedMultiplier += debuff.debuffAttackSpeedMultiplier;
            MovementSpeedMultiplier += debuff.debuffMovementSpeedMultiplier;
            StatusBuildUpMultiplier += debuff.debuffStatusBuildUpMultiplier;
            StatusDamageMultiplier += debuff.debuffStatusDamageMultiplier;
            GunChargeMultiplier += debuff.debuffGunChargeMultiplier;
            StaminaEfficiencyMultiplier += debuff.debuffStaminaEfficiencyMultiplier;
            AbilityDamageMultiplier += debuff.debuffAbilityDamageMultiplier;
            ResourceGainMultiplier += debuff.debuffResourceGainMultiplier;
        }

        foreach (Debuff debuff in activeDebuffs.ToArray())
        {
            switch (debuff.debuffType)
            {
                case Debuff.DebuffTypes.None:

                    if (debuff.instantiatedEffect == null && debuff.debuffEffect != null)
                    {
                        debuff.instantiatedEffect = Instantiate(debuff.debuffEffect, transform.position, Quaternion.identity);
                        debuff.instantiatedEffect.transform.SetParent(transform);
                    }

                    debuff.Duration -= Time.deltaTime;

                    if (debuff.Duration <= 0)
                    {
                        if (debuff.instantiatedEffect != null)
                        {
                            debuff.instantiatedEffect.transform.SetParent(null, true);
                            debuff.instantiatedEffect.transform.localScale = new Vector3(1, 1, 1);
                            debuff.instantiatedEffect.GetComponent<ParticleSystem>().Stop();
                        }

                        activeDebuffs.Remove(debuff);
                    }

                    if (debuff.removedOnRequirementMet && debuff.debuffDamageTaken >= debuff.damageRequirement)
                    {
                        if (debuff.instantiatedEffect != null)
                        {
                            debuff.instantiatedEffect.transform.SetParent(null, true);
                            debuff.instantiatedEffect.transform.localScale = new Vector3(1, 1, 1);
                            debuff.instantiatedEffect.GetComponent<ParticleSystem>().Stop();
                        }

                        activeDebuffs.Remove(debuff);
                    }
                    break;


                case Debuff.DebuffTypes.Fire:

                    if (debuff.instantiatedEffect == null)
                    {
                        debuff.instantiatedEffect = Instantiate(Resources.Load("Effects/FireEffect") as GameObject, transform.position, Quaternion.identity);
                        debuff.instantiatedEffect.transform.SetParent(transform);
                    }

                    if (!debuff.oneTimeEffectActivated)
                    {
                        debuff.oneTimeEffectActivated = true;

                        debuff.nextTickTime = Time.time + debuff.TickRate;
                    }

                    if (Time.time >= debuff.nextTickTime)
                    {
                        debuff.nextTickTime = Time.time + debuff.TickRate;
                        TakeDamage(debuff.Damage, debuff.Applier, false);
                    }

                    debuff.Duration -= Time.deltaTime;

                    if (debuff.Duration <= 0)
                    {
                        debuff.instantiatedEffect.transform.SetParent(null, true);
                        debuff.instantiatedEffect.transform.localScale = new Vector3(1, 1, 1);
                        debuff.instantiatedEffect.GetComponent<ParticleSystem>().Stop();

                        activeDebuffs.Remove(debuff);
                    }
                    break;

                case Debuff.DebuffTypes.Poison:

                    if (debuff.instantiatedEffect == null)
                    {
                        debuff.instantiatedEffect = Instantiate(Resources.Load("Effects/PoisonEffect") as GameObject, transform.position, Quaternion.identity);
                        debuff.instantiatedEffect.transform.SetParent(transform);
                    }

                    if (!debuff.oneTimeEffectActivated)
                    {
                        debuff.oneTimeEffectActivated = true;

                        debuff.nextTickTime = Time.time + debuff.TickRate;
                    }

                    if (Time.time >= debuff.nextTickTime)
                    {
                        debuff.nextTickTime = Time.time + debuff.TickRate;
                        TakeDamage(debuff.Damage, debuff.Applier, false);
                    }

                    debuff.Duration -= Time.deltaTime;

                    if (debuff.Duration <= 0)
                    {
                        debuff.instantiatedEffect.transform.SetParent(null, true);
                        debuff.instantiatedEffect.transform.localScale = new Vector3(1, 1, 1);
                        debuff.instantiatedEffect.GetComponent<ParticleSystem>().Stop();

                        activeDebuffs.Remove(debuff);
                    }
                    break;

                case Debuff.DebuffTypes.Electric:

                    if (debuff.instantiatedEffect == null)
                    {
                        debuff.instantiatedEffect = Instantiate(Resources.Load("Effects/ElectricityEffect") as GameObject, transform.position, Quaternion.identity);
                        debuff.instantiatedEffect.transform.SetParent(transform);
                    }

                    if (!debuff.oneTimeEffectActivated)
                    {
                        debuff.oneTimeEffectActivated = true;

                        debuff.nextTickTime = Time.time + debuff.TickRate;
                    }

                    if (Time.time >= debuff.nextTickTime)
                    {
                        debuff.nextTickTime = Time.time + debuff.TickRate;
                        TakeDamage(debuff.Damage, debuff.Applier, false);
                    }

                    debuff.Duration -= Time.deltaTime;

                    if (debuff.Duration <= 0)
                    {
                        debuff.instantiatedEffect.transform.SetParent(null, true);
                        debuff.instantiatedEffect.transform.localScale = new Vector3(1, 1, 1);
                        debuff.instantiatedEffect.GetComponent<ParticleSystem>().Stop();

                        activeDebuffs.Remove(debuff);
                    }
                    break;

                case Debuff.DebuffTypes.Bleed:

                    if (debuff.instantiatedEffect == null)
                    {
                        debuff.instantiatedEffect = Instantiate(Resources.Load("Effects/BleedEffect") as GameObject, transform.position, Quaternion.identity);
                        debuff.instantiatedEffect.transform.SetParent(transform);
                    }

                    if (!debuff.oneTimeEffectActivated)
                    {
                        debuff.oneTimeEffectActivated = true;

                        debuff.nextTickTime = Time.time + debuff.TickRate;
                    }

                    if (Time.time >= debuff.nextTickTime)
                    {
                        debuff.nextTickTime = Time.time + debuff.TickRate;
                        TakeDamage(debuff.Damage, debuff.Applier, false);
                    }

                    debuff.Duration -= Time.deltaTime;

                    if (debuff.Duration <= 0)
                    {
                        debuff.instantiatedEffect.transform.SetParent(null, true);
                        debuff.instantiatedEffect.transform.localScale = new Vector3(1, 1, 1);
                        debuff.instantiatedEffect.GetComponent<ParticleSystem>().Stop();

                        activeDebuffs.Remove(debuff);
                    }
                    break;

                case Debuff.DebuffTypes.Unstable:
                    if (debuff.instantiatedEffect == null)
                    {
                        debuff.instantiatedEffect = Instantiate(Resources.Load("Effects/UnstableEffect") as GameObject, transform.position, Quaternion.identity);
                        debuff.instantiatedEffect.transform.SetParent(transform);
                    }

                    if (!debuff.oneTimeEffectActivated)
                    {
                        debuff.oneTimeEffectActivated = true;

                        debuff.nextTickTime = Time.time + debuff.TickRate;
                    }

                    debuff.Duration -= Time.deltaTime;

                    if (debuff.Duration <= 0)
                    {
                        debuff.instantiatedEffect.transform.SetParent(null, true);
                        debuff.instantiatedEffect.transform.localScale = new Vector3(1, 1, 1);
                        debuff.instantiatedEffect.GetComponent<ParticleSystem>().Stop();

                        activeDebuffs.Remove(debuff);
                    }
                    break;
            }
        }
    }

    void TriggerDebuffDeathEffects()
    {
        foreach (Debuff debuff in activeDebuffs.ToArray())
        {
            switch (debuff.debuffType)
            {
                case Debuff.DebuffTypes.Unstable:
                    Instantiate(Resources.Load("Effects/UnstableExplosion") as GameObject, transform.position, Quaternion.identity).transform.localScale = Vector3.one * 4f;
                    GameManager.SpawnExplosion(Body.position, 4f, 3f, 3.5f, 0f, debuff.Damage, null, debuff.Applier);
                    break;
            }
        }
    }

    private Dictionary<Debuff, BuffDisplay> activeDisplays = new Dictionary<Debuff, BuffDisplay>();

    public void UpdateBuffDisplays(List<Debuff> debuffs)
    {
        // Remove displays for debuffs that no longer exist
        var toRemove = new List<Debuff>();
        foreach (var kvp in activeDisplays)
        {
            if (!debuffs.Contains(kvp.Key))
            {
                Destroy(kvp.Value.gameObject);
                toRemove.Add(kvp.Key);
            }
        }
        foreach (var key in toRemove)
            activeDisplays.Remove(key);

        // Add or update displays for current debuffs
        foreach (var debuff in debuffs)
        {
            if (!activeDisplays.ContainsKey(debuff))
            {
                var displayObj = Instantiate(Resources.Load("BuffDisplay") as GameObject, BuffBar);
                var display = displayObj.GetComponent<BuffDisplay>();
                display.SetDebuff(debuff);
                activeDisplays[debuff] = display;
            }
            else
            {
                activeDisplays[debuff].SetDebuff(debuff);
            }
        }
    }

    #endregion

    void OnDestroy()
    {
        if (Name == "Player")
        {
            // Special cleanup for player entity
            //GameSaveLoad.Instance.SaveGame(GameSaveLoad.OperationMode.RAM_OP);
        }   

        List<GameObject> displaysToDestroy = new List<GameObject>();
        if (BuffBar != null)
        {
            foreach (Transform child in BuffBar)
            {
                displaysToDestroy.Add(child.gameObject);
            }
        }

        foreach (GameObject display in displaysToDestroy)
        {
            Destroy(display);
        }
    }
}
