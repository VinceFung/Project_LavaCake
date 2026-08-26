using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.Events;

public class MeleeWeaponStats : MonoBehaviour
{
    public MeleeWeapon weapon;
    public ItemInstanceData data;
    public Transform modHolder;

    [Header("Base Stats")]

    public float baseDamageMultiplier = 1f;
    public float baseStaggerDamageMultiplier = 1f;
    public float baseSeverenceDamageMultiplier = 1f;
    [Space(5)]
    public float baseFinalSeverenceMultiplier = 0f;
    [Space(5)]
    public float baseLightAttackSpeedMultiplier = 1f;
    public float baseHeavyAttackSpeedMultiplier = 1f;
    [Space(5)]
    public float baselightStaminaEfficiency = 1f;
    public float baseheavyStaminaEfficiency = 1f;
    [Space(5)]
    public float baseGunChargeBonus = 0f;
    public float baseRangeBonus = 0f;
    public float baseKnockbackBonus = 0f;

    [Header("Stats")]

    public float DamageMultiplier = 1f;
    public float StaggerDamageMultiplier = 1f;
    public float SeverenceDamageMultiplier = 1f;
    [Space(5)]
    public float FinalSeverenceMultiplier = 0f;
    [Space(5)]
    public float LightAttackSpeedMultiplier = 1f;
    public float HeavyAttackSpeedMultiplier = 1f;
    [Space(5)]
    public float lightStaminaEfficiency = 1f;
    public float heavyStaminaEfficiency = 1f;
    [Space(5)]
    public float GunChargeBonus = 0f;
    public float RangeBonus = 0f;
    public float KnockbackBonus = 0f;

    public class Buff
    {
        public string Name;
        public float DamageBonus = 0f;
        public float StaggerDamageBonus = 0f;
        public float SeverenceDamageBonus = 0f;
        [Space]
        public float finalSeverenceBonus = 0f;
        [Space]
        public float LightAttackSpeedBonus = 0f;
        public float HeavyAttackSpeedBonus = 0f;
        [Space]
        public float lightStaminaEfficiencyBonus = 0f;
        public float heavyStaminaEfficiencyBonus = 0f;
        [Space]
        public float gunChargeBonus = 0f;
        public float rangeBonus = 0f;
        public float knockbackBonus = 0f;
    }

    public List<Buff> activeBuffs = new List<Buff>();

    public UnityEvent<Entity, DamageInstance, bool> ConditionalBonuses;
    public UnityEvent<Entity, DamageInstance, bool> OnDirectDamageDealt;

    List<ModToInstantiate> modsToInstantiate = new List<ModToInstantiate>();
    List<GameObject> modsToDestroy = new List<GameObject>();
    List<MeleeWeaponMod> instantiatedMods = new List<MeleeWeaponMod>();

    public class ModToInstantiate
    {
        public string ModID;
        public int stacks = 1;
    }

    private void Update()
    {
        baseDamageMultiplier = 1f;
        baseStaggerDamageMultiplier = 1f;
        baseSeverenceDamageMultiplier = 1f;

        baseFinalSeverenceMultiplier = 0f;

        baseLightAttackSpeedMultiplier = 1f;
        baseHeavyAttackSpeedMultiplier = 1f;

        baselightStaminaEfficiency = 1f;
        baseheavyStaminaEfficiency = 1f;

        baseGunChargeBonus = 0f;
        baseRangeBonus = 0f;
        baseKnockbackBonus = 0f;

        foreach (Buff activeBuff in activeBuffs)
        {
            baseDamageMultiplier += activeBuff.DamageBonus;
            baseStaggerDamageMultiplier += activeBuff.StaggerDamageBonus;
            baseSeverenceDamageMultiplier += activeBuff.SeverenceDamageBonus;

            baseFinalSeverenceMultiplier += activeBuff.finalSeverenceBonus;

            baseLightAttackSpeedMultiplier += activeBuff.LightAttackSpeedBonus;
            baseHeavyAttackSpeedMultiplier += activeBuff.HeavyAttackSpeedBonus;

            baselightStaminaEfficiency += activeBuff.lightStaminaEfficiencyBonus;
            baseheavyStaminaEfficiency += activeBuff.heavyStaminaEfficiencyBonus;

            baseGunChargeBonus += activeBuff.gunChargeBonus;
            baseRangeBonus = activeBuff.rangeBonus;
            baseKnockbackBonus = activeBuff.knockbackBonus;
        }

        DamageMultiplier = baseDamageMultiplier;
        StaggerDamageMultiplier = baseStaggerDamageMultiplier;
        SeverenceDamageMultiplier = baseSeverenceDamageMultiplier;
        FinalSeverenceMultiplier = baseFinalSeverenceMultiplier;
        LightAttackSpeedMultiplier = baseLightAttackSpeedMultiplier;
        HeavyAttackSpeedMultiplier = baseHeavyAttackSpeedMultiplier;
        lightStaminaEfficiency = baselightStaminaEfficiency;
        heavyStaminaEfficiency = baseheavyStaminaEfficiency;
        GunChargeBonus = baseGunChargeBonus;
        RangeBonus = baseRangeBonus;
        KnockbackBonus = baseKnockbackBonus;

        UpdateModGameObjects();
    }

    private void UpdateModGameObjects()
    {
        if (data == null || data.ModSlots == null) return;

        modsToInstantiate.Clear();
        foreach (ItemInstanceData.ModSlot slot in data.ModSlots)
        {
            if (slot.modItem != null)
            {
                bool increaseStack = false;
                ModToInstantiate increaseStackFor = null;
                foreach (ModToInstantiate mod in modsToInstantiate)
                {
                    if (mod.ModID == slot.modItem.itemID)
                    {
                        increaseStackFor = mod;
                        increaseStack = true;
                    }
                }

                if (increaseStack)
                {
                    increaseStackFor.stacks++;
                }
                else
                {
                    ModToInstantiate newMod = new ModToInstantiate();
                    newMod.ModID = slot.modItem.itemID;
                    newMod.stacks = 1;
                    modsToInstantiate.Add(newMod);
                }
            }
        }

        if (instantiatedMods.Count != modsToInstantiate.Count)
        {
            InstantiateMods();
        }
        else
        {
            bool changeMods = false;
            for (int i = 0; i < modsToInstantiate.Count; i++)
            {
                if (instantiatedMods[i].name != modsToInstantiate[i].ModID)
                {
                    changeMods = true;
                    break;
                }
                else
                {
                    instantiatedMods[i].stacks = modsToInstantiate[i].stacks;
                }
            }

            if (changeMods)
            {
                InstantiateMods();
            }
        }
    }

    void InstantiateMods()
    {
        modsToDestroy.Clear();
        foreach (Transform item in modHolder)
        {
            modsToDestroy.Add(item.gameObject);
        }
        foreach (GameObject item in modsToDestroy)
        {
            Destroy(item);
        }

        instantiatedMods.Clear();
        modsToInstantiate.Clear();
        foreach (ItemInstanceData.ModSlot slot in data.ModSlots)
        {
            if (slot.modItem != null)
            {
                bool increaseStack = false;
                ModToInstantiate increaseStackFor = null;
                foreach (ModToInstantiate mod in modsToInstantiate)
                {
                    if (slot.modItem != null)
                    {
                        if (mod.ModID == slot.modItem.itemID)
                        {
                            increaseStackFor = mod;
                            increaseStack = true;
                        }
                    }
                }

                if (increaseStack)
                {
                    increaseStackFor.stacks++;
                }
                else
                {
                    ModToInstantiate newMod = new ModToInstantiate();
                    newMod.ModID = slot.modItem.itemID;
                    newMod.stacks = 1;
                    modsToInstantiate.Add(newMod);
                }
            }
        }

        foreach (ModToInstantiate mod in modsToInstantiate)
        {
            GameObject modObj = null;
            foreach (ItemInstanceData.ModSlot item in data.ModSlots)
            {
                if (item.modItem != null)
                {
                    if (item.modItem.itemID == mod.ModID)
                    {
                        modObj = item.modItem.itemObject;
                    }
                }
            }
            GameObject instantiatedModObj = Instantiate(modObj, modHolder);
            instantiatedModObj.name = mod.ModID;
            MeleeWeaponMod meleeWeaponMod = instantiatedModObj.GetComponent<MeleeWeaponMod>();
            meleeWeaponMod.stats = this;
            instantiatedMods.Add(meleeWeaponMod);
        }
    }

    public void UpdateConditionalBonus(Entity reciever, DamageInstance dmg, bool heavy)
    {
        DamageMultiplier = baseDamageMultiplier;
        StaggerDamageMultiplier = baseStaggerDamageMultiplier;
        SeverenceDamageMultiplier = baseSeverenceDamageMultiplier;
        FinalSeverenceMultiplier = baseFinalSeverenceMultiplier;
        LightAttackSpeedMultiplier = baseLightAttackSpeedMultiplier;
        HeavyAttackSpeedMultiplier = baseHeavyAttackSpeedMultiplier;
        lightStaminaEfficiency = baselightStaminaEfficiency;
        heavyStaminaEfficiency = baseheavyStaminaEfficiency;
        GunChargeBonus = baseGunChargeBonus;
        RangeBonus = baseRangeBonus;
        KnockbackBonus = baseKnockbackBonus;

        ConditionalBonuses.Invoke(reciever, dmg, heavy);
    }
    
    public void CallOnDirectDamageDealt(Entity reciever, DamageInstance dmg, bool heavy)
    {
        OnDirectDamageDealt.Invoke(reciever, dmg, heavy);
    }
}
