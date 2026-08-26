using System.Collections;
using UnityEngine;

public class NpcProjectileWeapon : MonoBehaviour
{
    public GameObject ProjectilePrefab;
    public Transform[] firePoints;
    public ParticleSystem EmitParticles;
    public AudioClip emitAudio;
    public float emitAudioVolume = 1f;

    public float collisionRadius;

    public Entity Owner;

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

            for (int f = 0; f < firePoints.Length; f++)
            {
                GameObject InstantiatedProjectile = Instantiate(ProjectilePrefab, firePoints[f].position, firePoints[f].rotation);
                Projectile proj = InstantiatedProjectile.GetComponent<Projectile>();

                proj.collisionRadius = collisionRadius;
                proj.Owner = Owner;
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
