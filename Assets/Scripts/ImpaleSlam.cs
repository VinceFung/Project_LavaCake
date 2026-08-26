using UnityEngine;

public class ImpaleSlam : MonoBehaviour
{
    public MeleeWeapon weapon;
    public ParticleSystem explosionEffect;
    public Transform explosionPosition;
    public float explosionRadius;
    public float explosionRadiusNear;
    public float explosionRadiusFar;
    public DamageInstance maxDamage;
    public float  damageFallOff;
    public float explosionForce;
    public bool dropCorpseOnSlam = true;

    public DebuffPreset debuffToApply;

    public void SlamCorpse()
    {
        if (weapon.Wielder.Name == "Player")
        {
            weapon.StaggerPulse();
        }

        foreach (Entity impaledEntity in weapon.impaledEntities)
        {
            if (impaledEntity != null)
            {
                impaledEntity.transform.parent = null;
                impaledEntity.rigidBody.isKinematic = false;

                if (impaledEntity.impalementEffect != null)
                {
                    weapon.impalementEffects.Add(Instantiate(impaledEntity.impalementEffect, impaledEntity.Body.position, impaledEntity.Body.rotation * Quaternion.Euler(180f, 0f, 0f)));
                }
            }
        }

        if (dropCorpseOnSlam) weapon.DropCorpses();

        if(explosionEffect != null) explosionEffect.Play();
        GameManager.SpawnExplosion(explosionPosition.position, explosionRadius, explosionRadiusNear, explosionRadiusFar, damageFallOff, maxDamage, debuffToApply, weapon.Wielder, !dropCorpseOnSlam);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(explosionPosition.position, explosionRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(explosionPosition.position, explosionRadiusNear);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(explosionPosition.position, explosionRadiusFar);
    }
}
