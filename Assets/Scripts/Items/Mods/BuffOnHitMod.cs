using UnityEditor;
using UnityEngine;

public class BuffOnHitMod : MonoBehaviour
{
    public MeleeWeaponMod mod;

    public float buildUpOnHit;
    float buildUp;

    public DebuffPreset debuffToApply;

    BuffDisplay instantiatedDisplay;

    private void Start()
    {
        mod.stats.OnDirectDamageDealt.AddListener(OnHit);
    }

    private void OnDestroy()
    {
        mod.stats.OnDirectDamageDealt.RemoveListener(OnHit);
    }

    private void Update()
    {
        if (instantiatedDisplay != null)
        {
            if (instantiatedDisplay.debuff.Duration <= 0)
            {
                Destroy(instantiatedDisplay.gameObject);
            }
        }
    }

    void OnHit(Entity reciever, DamageInstance dmg, bool heavy)
    {
        buildUp += buildUpOnHit;

        if (buildUp >= 100f)
        {
            buildUp -= 100f;

            mod.stats.weapon.Wielder.ApplyDebuff(debuffToApply ,mod.stats.weapon.Wielder, instance =>
            {
                instance.debuffDamageInputMultiplier = debuffToApply.debuffDamageInputMultiplier * mod.stacks;
                instance.debuffDamageMultiplier = debuffToApply.debuffDamageMultiplier * mod.stacks;
                instance.debuffAttackSpeedMultiplier = debuffToApply.debuffAttackSpeedMultiplier * mod.stacks;
                instance.debuffMovementSpeedMultiplier = debuffToApply.debuffMovementSpeedMultiplier * mod.stacks;
            });
        }
    }
}
