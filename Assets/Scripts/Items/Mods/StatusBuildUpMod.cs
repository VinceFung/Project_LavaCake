using UnityEngine;

public class StatusBuildUpMod : MonoBehaviour
{
    public MeleeWeaponMod mod;

    public float buildUpOnHit;
    public bool scaleBuildUp = true;
    public bool scalePower = false;
    public bool scaleDamage = false;

    public float weaponToStatusDmgMultiplier;
    public DebuffPreset debuffToApply;

    private void Start()
    {
        mod.stats.OnDirectDamageDealt.AddListener(OnHit);
    }

    private void OnDestroy()
    {
        mod.stats.OnDirectDamageDealt.RemoveListener(OnHit);
    }

    void OnHit(Entity reciever, DamageInstance dmg, bool heavy)
    {
        if(reciever.EntityType == Entity.EntityTypes.Character)
        {
            float buildUp = scaleBuildUp ? buildUpOnHit * mod.stacks : buildUpOnHit;
            mod.stats.weapon.AddStatusBuildUp(debuffToApply, weaponToStatusDmgMultiplier, buildUp, instance =>
            {
                if (scalePower)
                {
                    instance.debuffDamageInputMultiplier = debuffToApply.debuffDamageInputMultiplier * mod.stacks;
                }

                if (scaleDamage)
                {
                    instance.Damage.HealthDamage = mod.stats.weapon.lightDamage.HealthDamage * mod.stats.DamageMultiplier * mod.stats.weapon.Wielder.DamageMultiplier * weaponToStatusDmgMultiplier * mod.stacks;
                }
                else
                {
                    instance.Damage.HealthDamage = mod.stats.weapon.lightDamage.HealthDamage * mod.stats.DamageMultiplier * mod.stats.weapon.Wielder.DamageMultiplier * weaponToStatusDmgMultiplier;
                }
            });
        }
    }
}
