using UnityEngine;
using UnityEngine.UI;

public class RiposteMod : MonoBehaviour
{
    public MeleeWeaponMod mod;

    public bool riposteActive = false;

    public float buildUpOnHit;
    float buildUp;

    BuffDisplay instantiatedDisplay;
    public Sprite buffSprite;

    public BuffDisplay.DisplayTypes buffDisplayStat;

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

    private void Start()
    {
        mod.stats.weapon.Wielder.OnAttackDodged.AddListener(OnDodge);
        mod.stats.OnDirectDamageDealt.AddListener(OnHit);
    }

    private void OnDestroy()
    {
        mod.stats.weapon.Wielder.OnAttackDodged.RemoveListener(OnDodge);
        mod.stats.OnDirectDamageDealt.RemoveListener(OnHit);

        if(instantiatedDisplay != null)
        {
            Destroy(instantiatedDisplay.gameObject);
        }
    }

    private void Update()
    {
        if (instantiatedDisplay != null)
        {
            if (!riposteActive) Destroy(instantiatedDisplay.gameObject);
        }

        if (riposteActive)
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
        else
        {
            MeleeWeaponStats.Buff buffToRemove = null;
            foreach (MeleeWeaponStats.Buff buff in mod.stats.activeBuffs)
            {
                if (buff.Name == mod.modID)
                {
                    buffToRemove = buff;
                    break;
                }
            }

            mod.stats.activeBuffs.Remove(buffToRemove);
        }
    }

    public void OnDodge()
    {
        if (riposteActive) return;
        
        buildUp += buildUpOnHit;

        if (buildUp >= 100f)
        {
            buildUp -= 100f;

            riposteActive = true;

            instantiatedDisplay = Instantiate(Resources.Load("BuffDisplay") as GameObject, mod.stats.weapon.Wielder.BuffBar).GetComponent<BuffDisplay>();
            instantiatedDisplay.BuffIconImage.sprite = buffSprite;
        }
    }

    void OnHit(Entity reciever, DamageInstance dmg, bool heavy)
    {
        if(reciever.EntityType == Entity.EntityTypes.Character) riposteActive = false;
    }
}
