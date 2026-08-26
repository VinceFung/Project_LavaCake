using UnityEngine;
using UnityEngine.UIElements;

public class ShockToothRelic : MonoBehaviour
{
    public EntityRelic relic;
    public DebuffPreset debuffToApply;
    public BuffDisplay.DisplayTypes buffDisplayType = BuffDisplay.DisplayTypes.AttackSpeedBonus;
    public GameObject lightningEffect;
    public float lightningRadius = 4f;
    public float lightningDamageMultiplier = 1.5f;
    public DamageInstance lightningDamage;

    bool strikeReady;
    float strikeCooldownTime;

    MeleeWeapon lastMelee;

    private void Start()
    {
        lastMelee = relic.owner.meleeWeapon;
        relic.owner.meleeWeapon.stats.OnDirectDamageDealt.AddListener(LightningStrikeOnHit);
        relic.owner.OnStatusApplied.AddListener(OnElectricStatus);
    }

    private void OnDestroy()
    {
        relic.owner.meleeWeapon.stats.OnDirectDamageDealt.RemoveListener(LightningStrikeOnHit);
        relic.owner.OnStatusApplied.RemoveListener(OnElectricStatus);
    }

    private void Update()
    {
        if (relic.owner.meleeWeapon != lastMelee)
        {
            relic.owner.meleeWeapon.stats.OnDirectDamageDealt.AddListener(LightningStrikeOnHit);
        }
        lastMelee = relic.owner.meleeWeapon;
    }

    void OnElectricStatus(Entity reciever, Debuff appliedStatus)
    {
        bool hasElectric = false;
        if(appliedStatus != null)
        {
            if (appliedStatus.debuffType == Debuff.DebuffTypes.Electric)
            {
                hasElectric = true;
            }

            if (hasElectric)
            {
                if (!strikeReady)
                {
                    strikeCooldownTime = Time.time + 0.1f;
                    strikeReady = true;
                }

                relic.owner.ApplyDebuff(debuffToApply, relic.owner);
            }
        }
    }

    void LightningStrikeOnHit(Entity reciever, DamageInstance dmg, bool heavy)
    {
        if (strikeReady && Time.time >= strikeCooldownTime)
        {
            Instantiate(lightningEffect, reciever.Body.position, Quaternion.identity).transform.localScale = Vector3.one * lightningRadius;
            lightningDamage.HealthDamage = relic.owner.meleeWeapon.lightDamage.HealthDamage * lightningDamageMultiplier * relic.owner.meleeWeapon.stats.DamageMultiplier * relic.owner.DamageMultiplier;
            lightningDamage.StaggerDamage = relic.owner.meleeWeapon.lightDamage.HealthDamage * lightningDamageMultiplier * relic.owner.meleeWeapon.stats.StaggerDamageMultiplier * relic.owner.DamageMultiplier;
            GameManager.SpawnExplosion(reciever.Body.position, lightningRadius, lightningRadius, lightningRadius, 0f, lightningDamage, null, relic.owner);
            strikeReady = false;
        }
    }
}
