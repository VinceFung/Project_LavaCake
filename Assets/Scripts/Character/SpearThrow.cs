using UnityEngine;
using System.Collections;

public class SpearThrow : MonoBehaviour
{
    public MeleeWeapon weapon;
    public GameObject ProjectilePrefab;

    public int projectileCount = 1;

    public ParticleSystem EmitParticles;
    public AudioClip emitAudio;
    public float emitAudioVolume = 1f;

    public float collisionRadius;

    public int burstCount = 1;
    public float timeBtwBurst = 0.1f;
    public DamageInstance DamageInstance;
    public float Speed = 5f;
    public float LifeTime = 5f;
    public float Range = 20f;

    public bool Explosive;
    public float ExplosionRadius;
    public DamageInstance ExplosionDamageInstance;

    public bool piercing;
    public bool goesThroughWalls;

    public DebuffPreset debuffToApply;

    public void EmitProjectile()
    {
        StartCoroutine(Burst());
    }

    IEnumerator Burst()
    {
        for (int i = 0; i < burstCount; i++)
        {
            if(emitAudio != null)
            {
                SoundFXManager.Instance.PlaySoundClip(emitAudio, transform.position, emitAudioVolume, Random.Range(0.95f, 1.05f));
            }
            if (EmitParticles != null) EmitParticles.Play();

            for (int f = 0; f < projectileCount; f++)
            {
                Vector3 firePoint = weapon.Wielder.Body.position;
                firePoint.y = transform.position.y;

                Quaternion rot = Quaternion.LookRotation(weapon.Wielder.Body.forward, Vector3.up);
                if(weapon.Wielder.charCombatAI != null && weapon.Wielder.charCombatAI.Target != null)
                {
                    Vector3 targetDir = (weapon.Wielder.charCombatAI.Target.entity.Body.position - weapon.Wielder.Body.position).normalized;
                    Vector3 horizontalDir = weapon.Wielder.Body.forward;
                    horizontalDir.y = 0f;
                    horizontalDir = horizontalDir.normalized;
                    
                    Vector3 finalDir = new Vector3(horizontalDir.x, targetDir.y, horizontalDir.z).normalized;
                    rot = Quaternion.LookRotation(finalDir, Vector3.up);
                }
                GameObject InstantiatedProjectile = Instantiate(ProjectilePrefab, firePoint, rot);
                Projectile proj = InstantiatedProjectile.GetComponent<Projectile>();

                proj.collisionRadius = collisionRadius;
                proj.Owner = weapon.Wielder;
                proj.DamageInstance = new DamageInstance(DamageInstance);
                proj.Speed = Speed;
                proj.LifeTime = LifeTime;
                proj.Range = Range;

                proj.Explosive = Explosive;
                proj.ExplosionRadius = ExplosionRadius;
                proj.ExplosionDamageInstance = ExplosionDamageInstance;

                proj.piercing = piercing;
                proj.goesThroughWalls = goesThroughWalls;

                proj.debuffToApply = debuffToApply;
            }
            yield return new WaitForSeconds(timeBtwBurst);
        }
    }
}
