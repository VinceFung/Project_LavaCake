using UnityEngine;

public class MeleeWeaponStatIncreaseRelic : MonoBehaviour
{
    public EntityRelic relic;

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
        foreach (MeleeWeaponStats.Buff buff in relic.owner.meleeWeapon.stats.activeBuffs)
        {
            if (buff.Name == relic.RelicID)
            {
                hasBuff = true;
                buff.DamageBonus = DamageBonus;
                buff.StaggerDamageBonus = StaggerDamageBonus;
                buff.SeverenceDamageBonus = SeverenceDamageBonus;

                buff.finalSeverenceBonus = FinalSeverenceBonus;

                buff.LightAttackSpeedBonus = LightAttackSpeedBonus;
                buff.HeavyAttackSpeedBonus = HeavyAttackSpeedBonus;

                buff.lightStaminaEfficiencyBonus = lightStaminaEfficiencyBonus;
                buff.heavyStaminaEfficiencyBonus = heavyStaminaEfficiencyBonus;

                buff.gunChargeBonus = gunChargeBonus;
                buff.rangeBonus = rangeBonus;
                buff.knockbackBonus = knockbackBonus;
            }
        }

        if (!hasBuff)
        {
            MeleeWeaponStats.Buff newBuff = new MeleeWeaponStats.Buff();
            newBuff.Name = relic.RelicID;
            newBuff.DamageBonus = DamageBonus;
            newBuff.DamageBonus = DamageBonus;
            newBuff.StaggerDamageBonus = StaggerDamageBonus;
            newBuff.SeverenceDamageBonus = SeverenceDamageBonus;

            newBuff.finalSeverenceBonus = FinalSeverenceBonus;

            newBuff.LightAttackSpeedBonus = LightAttackSpeedBonus;
            newBuff.HeavyAttackSpeedBonus = HeavyAttackSpeedBonus;

            newBuff.lightStaminaEfficiencyBonus = lightStaminaEfficiencyBonus;
            newBuff.heavyStaminaEfficiencyBonus = heavyStaminaEfficiencyBonus;

            newBuff.gunChargeBonus = gunChargeBonus;
            newBuff.rangeBonus = rangeBonus;
            newBuff.knockbackBonus = knockbackBonus;

            relic.owner.meleeWeapon.stats.activeBuffs.Add(newBuff);
        }
    }

    private void OnDestroy()
    {
        bool hasBuff = false;
        MeleeWeaponStats.Buff buffToRemove = null;
        foreach (MeleeWeaponStats.Buff buff in relic.owner.meleeWeapon.stats.activeBuffs)
        {
            if (buff.Name == relic.RelicID)
            {
                buffToRemove = buff;
                hasBuff = true;
            }
        }

        if (hasBuff)
        {
            relic.owner.meleeWeapon.stats.activeBuffs.Remove(buffToRemove);
        }
    }
}
