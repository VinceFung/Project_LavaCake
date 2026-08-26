using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HemorrhageAbility : MonoBehaviour
{
    public Ability ability;
    public float energyRegenSpeed;
    public Entity Owner;
    public Animator anim;
    public string CastAnimationName = "AbilityCastRadial";
    public float targettingRadius;
    public LayerMask targettingMask;
    public GameObject ProjectilePrefab;
    public float spread = 15f;
    public Transform[] firePoints;
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

    public float seekingAmount;
    public float seekingChangeRate;
    public float seekingAmountMax;
    public float seekingDisableRadius = 0f;
    public bool horizontalSeekRotation;

    public DebuffPreset debuffToApply;

    List<Entity> targets = new List<Entity>();
    List<Entity> targetsToRemove = new List<Entity>();

    private void Update()
    {
        bool IsCoveredInBlood = false;
        Debuff coveredInBloodDebuff = null;
        foreach (Debuff debuff in Owner.activeDebuffs)
        {
            if (debuff.DebuffName == "Covered In Blood")
            {
                IsCoveredInBlood = true;
                coveredInBloodDebuff = debuff;
                break;
            }
        }

        if (IsCoveredInBlood)
        {
            ability.AbilityRegenSpeed = energyRegenSpeed;
        }
        else
        {
            ability.AbilityRegenSpeed = 0f;
        }
    }

    public void PlayCastAnimation()
    {
        targets.Clear();
        Collider[] entitiesCaught = Physics.OverlapSphere(Owner.Body.position, targettingRadius, targettingMask);

        // Gather and sort by distance
        List<Entity> foundEntities = new List<Entity>();
        foreach (Collider entityCaught in entitiesCaught)
        {
            if (entityCaught != null)
            {
                Entity entity = entityCaught.GetComponent<Entity>();
                if (entity != null && entity.Team != Owner.Team && !foundEntities.Contains(entity))
                {
                    foundEntities.Add(entity);
                }
            }
        }

        foundEntities.Sort((a, b) =>
        {
            // Use horizontal distance for ability targeting
            Vector3 distA = Owner.Body.position - a.Body.position;
            Vector3 distB = Owner.Body.position - b.Body.position;
            distA.y = 0;
            distB.y = 0;
            return distA.magnitude.CompareTo(distB.magnitude);
        });

        targets.AddRange(foundEntities);

        if (targets.Count > 0)
        {
            ability.currentAbilityEnergy = 0f;
            anim.CrossFade(CastAnimationName, 0.1f);
        }
    }

    public void EmitProjectile()
    {
        StartCoroutine(Burst());
    }

    IEnumerator Burst()
    {
        int t = 0;

        for (int i = 0; i < burstCount; i++)
        {
            foreach (Entity target in targets)
            {
                if (target == null) targetsToRemove.Add(target);
            }

            foreach (Entity target in targetsToRemove)
            {
                targets.Remove(target);
            }

            if (t >= targets.Count) t = 0;

            if(targets.Count == 0)
            {
                yield break; // No targets left, exit the coroutine
            }

            if (emitAudio != null)
            {
                SoundFXManager.Instance.PlaySoundClip(emitAudio, transform.position, emitAudioVolume, Random.Range(0.95f, 1.05f));
            }
            if (EmitParticles != null) EmitParticles.Play();

            for (int f = 0; f < firePoints.Length; f++)
            {
                GameObject InstantiatedProjectile = Instantiate(ProjectilePrefab, firePoints[f].position, firePoints[f].rotation);

                InstantiatedProjectile.transform.Rotate(Random.Range(-spread, spread), Random.Range(-spread, spread), 0f);

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

                proj.seeking = true;
                proj.seekingAmount = seekingAmount;
                proj.seekingChangeRate = seekingChangeRate;
                proj.seekingAmountMax = seekingAmountMax;
                proj.seekingDisableRadius = seekingDisableRadius;
                proj.horizontalSeekRotation = horizontalSeekRotation;
                proj.seekingTarg = targets[t].Body;

                proj.debuffToApply = debuffToApply;
            }

            t++;
            if(t >= targets.Count) t = 0;

            yield return new WaitForSeconds(timeBtwBurst);
        }
    }
}
