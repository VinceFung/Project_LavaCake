using UnityEngine;

public class EmberCharmRelic : MonoBehaviour
{
    public EntityRelic relic;
    public GameObject explosionEffect;
    public float statusToDamageMultiplier = 2.5f;
    public float explosionRadius;
    public DamageInstance explosionDamage;

    MeleeWeapon lastMelee;

    float cooldownTime;

    private void Start()
    {
        relic.owner.OnKill.AddListener(Explode);
        relic.owner.meleeWeapon.OnTargetSevered.AddListener(Explode);
    }

    private void OnDestroy()
    {
        relic.owner.OnKill.RemoveListener(Explode);
        relic.owner.meleeWeapon.OnTargetSevered.RemoveListener(Explode);
    }

    private void Update()
    {
        if (relic.owner.meleeWeapon != lastMelee)
        {
            relic.owner.meleeWeapon.OnTargetSevered.AddListener(Explode);
        }
        lastMelee = relic.owner.meleeWeapon;
    }

    void Explode(Entity target)
    {
        if (Time.time >= cooldownTime)
        {
            cooldownTime = Time.time + 0.5f;
            Vector3 pos = target.Body.position;

            bool spawnExplosion = false;
            float damage = 0f;
            foreach (Debuff debuff in target.activeDebuffs)
            {
                if (debuff.DebuffName == "Fire")
                {
                    spawnExplosion = true;
                    damage = debuff.Damage.HealthDamage * statusToDamageMultiplier;
                }
            }

            if (spawnExplosion)
            {
                explosionDamage.HealthDamage = damage;
                explosionDamage.StaggerDamage = damage;
                GameManager.SpawnExplosion(pos, explosionRadius, explosionRadius, explosionRadius, 0f, explosionDamage, null, relic.owner);

                Instantiate(explosionEffect, pos, Quaternion.identity).transform.localScale = Vector3.one * explosionRadius;
            }
        }
    }
}
