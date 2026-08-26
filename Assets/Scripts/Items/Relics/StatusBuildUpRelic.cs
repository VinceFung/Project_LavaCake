using UnityEngine;

public class StatusBuildUpRelic : MonoBehaviour
{
    public EntityRelic relic;

    public float buildUpOnHit;
    float buildUp;

    public float weaponToStatusDmgMultiplier;
    public DebuffPreset debuffToApply;

    MeleeWeapon lastMelee;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lastMelee = relic.owner.meleeWeapon;
        relic.owner.meleeWeapon.stats.OnDirectDamageDealt.AddListener(OnHit);
    }

    private void OnDestroy()
    {
        relic.owner.meleeWeapon.stats.OnDirectDamageDealt.RemoveListener(OnHit);
    }

    // Update is called once per frame
    void Update()
    {
        if (relic.owner.meleeWeapon != lastMelee)
        {
            relic.owner.meleeWeapon.stats.OnDirectDamageDealt.AddListener(OnHit);
        }
        lastMelee = relic.owner.meleeWeapon;
    }

    void OnHit(Entity reciever, DamageInstance dmg, bool heavy)
    {
        if (reciever.EntityType == Entity.EntityTypes.Character)
        {
            relic.owner.meleeWeapon.AddStatusBuildUp(debuffToApply, weaponToStatusDmgMultiplier, buildUpOnHit, instance =>
            {
                instance.Damage.HealthDamage = relic.owner.meleeWeapon.lightDamage.HealthDamage * relic.owner.meleeWeapon.stats.DamageMultiplier * relic.owner.meleeWeapon.Wielder.DamageMultiplier * weaponToStatusDmgMultiplier;
            });
        }
    }
}
