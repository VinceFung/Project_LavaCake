using UnityEngine;

public class ExecutionMod : MonoBehaviour
{
    public MeleeWeaponMod mod;
    public float damageBonus = 0.12f;

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
        if(heavy)
        {
            if (reciever.ImpalementImmune)
            {
                mod.stats.DamageMultiplier += damageBonus * mod.stacks * 2f;
            }
            else
            {
                mod.stats.DamageMultiplier += damageBonus * mod.stacks;
            }
        }
    }
}
