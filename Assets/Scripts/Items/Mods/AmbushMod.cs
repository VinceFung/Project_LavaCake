using UnityEngine;
using UnityEngine.Events;

public class AmbushMod : MonoBehaviour
{
    public MeleeWeaponMod mod;
    public float damageBonus = 0.25f;
    public float healthPercentRequirement = 0.9f;
    public bool checkForLessThan = false;

    private void Start()
    {
        mod.stats.ConditionalBonuses.AddListener(Check);
    }

    private void OnDestroy()
    {
        mod.stats.ConditionalBonuses.RemoveListener(Check);
    }

    void Check(Entity reciever, DamageInstance dmg, bool heavy)
    {
        if (!checkForLessThan)
        {
            if (reciever.Health >= reciever.MaxHealth * healthPercentRequirement)
            {
                mod.stats.DamageMultiplier += damageBonus * mod.stacks;
                mod.stats.StaggerDamageMultiplier += damageBonus * mod.stacks;
                mod.stats.SeverenceDamageMultiplier += damageBonus * mod.stacks;
            }
        }
        else
        {
            if (reciever.Health <= reciever.MaxHealth * healthPercentRequirement)
            {
                mod.stats.DamageMultiplier += damageBonus * mod.stacks;
                mod.stats.StaggerDamageMultiplier += damageBonus * mod.stacks;
                mod.stats.SeverenceDamageMultiplier += damageBonus * mod.stacks;
            }
        }
    }
}
