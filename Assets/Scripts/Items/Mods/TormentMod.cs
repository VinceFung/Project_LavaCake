using UnityEngine;

public class TormentMod : MonoBehaviour
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
        foreach (Debuff debuff in reciever.activeDebuffs)
        {
            mod.stats.DamageMultiplier += damageBonus * mod.stacks;
        }
    }
}
