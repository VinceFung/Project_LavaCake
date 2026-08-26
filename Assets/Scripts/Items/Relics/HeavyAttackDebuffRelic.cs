using UnityEngine;

public class HeavyAttackDebuffRelic : MonoBehaviour
{
    public EntityRelic relic;
    public float weaponToStatusDmgMultiplier = 0.4f;
    public DebuffPreset debuffToApply;

    MeleeWeapon lastMelee;

    private void Start()
    {
        lastMelee = relic.owner.meleeWeapon;
        relic.owner.meleeWeapon.stats.OnDirectDamageDealt.AddListener(ApplyDebuff);
    }

    private void OnDestroy()
    {
        relic.owner.meleeWeapon.stats.OnDirectDamageDealt.RemoveListener(ApplyDebuff);
    }

    private void Update()
    {
        if (relic.owner.meleeWeapon != lastMelee)
        {
            relic.owner.meleeWeapon.stats.OnDirectDamageDealt.AddListener(ApplyDebuff);
        }
        lastMelee = relic.owner.meleeWeapon;
    }

    public void ApplyDebuff(Entity reciever, DamageInstance dmg, bool heavy)
    {
        if (heavy)
        {
            reciever.ApplyDebuff(debuffToApply, relic.owner, instance =>
            {
                instance.Damage.HealthDamage = relic.owner.meleeWeapon.lightDamage.HealthDamage * relic.owner.meleeWeapon.stats.DamageMultiplier * relic.owner.DamageMultiplier * weaponToStatusDmgMultiplier;
            });
        }
    }
}
