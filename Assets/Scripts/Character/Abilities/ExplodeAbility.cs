using UnityEngine;

public class ExplodeAbility : MonoBehaviour
{
    public Ability ability;
    public bool consumeEnergy = true;
    public bool triggerCastEvent = true;
    public GameObject explosionEffect;
    public float explosionRadius = 7f;
    public float explosionRadiusNear = 3f;
    public float explosionRadiusFar = 5f;
    public DamageInstance maxDamage;
    public float damageFallOff = 0.067f;

    public DebuffPreset debuffToApply;

    public void Explode()
    {
        if (consumeEnergy) ability.currentAbilityEnergy = 0f;

        DamageInstance dmg = new DamageInstance(maxDamage);

        Instantiate(explosionEffect, ability.Owner.Body.position, Quaternion.identity).transform.localScale = Vector3.one * explosionRadius;

        GameManager.SpawnExplosion(ability.Owner.Body.position, explosionRadius, explosionRadiusNear, explosionRadiusFar, damageFallOff, dmg, debuffToApply, ability.Owner);

        if(triggerCastEvent) ability.Owner.OnAbilityCast.Invoke();
    }
}
