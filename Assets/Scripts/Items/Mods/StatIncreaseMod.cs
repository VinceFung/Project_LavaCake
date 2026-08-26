using UnityEngine;

public class StatIncreaseMod : MonoBehaviour
{
    public MeleeWeaponMod mod;

    public float DamageBonus = 0f;
    public float StaggerDamageBonus = 0f;
    public float SeverenceDamageBonus = 0f;
    [Space]
    public float FinalSeverenceBonus = 0f;
    [Space]
    public float LightAttackSpeedBonus = 0f;
    public float HeavyAttackSpeedBonus = 0f;
    [Space]
    public float lightStaminaEfficiencyBonus = 0f;
    public float heavyStaminaEfficiencyBonus = 0f;
    [Space(5)]
    public float gunChargeBonus = 0f;
    public float rangeBonus = 0f;
    public float knockbackBonus = 0f;

    private void Update()
    {
        bool hasBuff = false;
        foreach (MeleeWeaponStats.Buff buff in mod.stats.activeBuffs)
        {
            if (buff.Name == mod.modID)
            {
                hasBuff = true;
                buff.DamageBonus = DamageBonus * mod.stacks;
                buff.StaggerDamageBonus = StaggerDamageBonus * mod.stacks;
                buff.SeverenceDamageBonus = SeverenceDamageBonus * mod.stacks;

                buff.finalSeverenceBonus = FinalSeverenceBonus * mod.stacks;

                buff.LightAttackSpeedBonus = LightAttackSpeedBonus * mod.stacks;
                buff.HeavyAttackSpeedBonus = HeavyAttackSpeedBonus * mod.stacks;

                buff.lightStaminaEfficiencyBonus = lightStaminaEfficiencyBonus * mod.stacks;
                buff.heavyStaminaEfficiencyBonus = heavyStaminaEfficiencyBonus * mod.stacks;

                buff.gunChargeBonus = gunChargeBonus * mod.stacks;
                buff.rangeBonus = rangeBonus * mod.stacks;
                buff.knockbackBonus = knockbackBonus * mod.stacks;
            }
        }

        if (!hasBuff)
        {
            MeleeWeaponStats.Buff newBuff = new MeleeWeaponStats.Buff();
            newBuff.Name = mod.modID;
            newBuff.DamageBonus = DamageBonus * mod.stacks;
            newBuff.DamageBonus = DamageBonus * mod.stacks;
            newBuff.StaggerDamageBonus = StaggerDamageBonus * mod.stacks;
            newBuff.SeverenceDamageBonus = SeverenceDamageBonus * mod.stacks;

            newBuff.finalSeverenceBonus = FinalSeverenceBonus * mod.stacks;

            newBuff.LightAttackSpeedBonus = LightAttackSpeedBonus * mod.stacks;
            newBuff.HeavyAttackSpeedBonus = HeavyAttackSpeedBonus * mod.stacks;

            newBuff.lightStaminaEfficiencyBonus = lightStaminaEfficiencyBonus * mod.stacks;
            newBuff.heavyStaminaEfficiencyBonus = heavyStaminaEfficiencyBonus * mod.stacks;

            newBuff.gunChargeBonus = gunChargeBonus * mod.stacks;
            newBuff.rangeBonus = rangeBonus * mod.stacks;
            newBuff.knockbackBonus = knockbackBonus * mod.stacks;

            mod.stats.activeBuffs.Add(newBuff);
        }
    }

    private void OnDestroy()
    {
        bool hasBuff = false;
        MeleeWeaponStats.Buff buffToRemove = null;
        foreach (MeleeWeaponStats.Buff buff in mod.stats.activeBuffs)
        {
            if (buff.Name == mod.modID)
            {
                buffToRemove = buff;
                hasBuff = true;
            }
        }

        if (hasBuff)
        {
            mod.stats.activeBuffs.Remove(buffToRemove);
        }
    }
}
