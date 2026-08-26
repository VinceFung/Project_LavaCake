using UnityEngine;
using System.Collections.Generic;

public class RadialDetonateUnstable : MonoBehaviour
{
    public Ability ability;

    public float captureRadius = 6f;
    public LayerMask characterMask;

    public void Detonate()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, captureRadius, characterMask);

        foreach (Collider collider in colliders)
        {
            Entity entity = collider.GetComponent<Entity>();
            if (entity != null && entity != ability.Owner)
            {
                bool hasUnstable = false;
                DamageInstance unstableDamage = null;
                List<Debuff> debuffsToRemove = new List<Debuff>();
                foreach (Debuff debuff in entity.activeDebuffs.ToArray())
                {
                    if(debuff.debuffType == Debuff.DebuffTypes.Unstable)
                    {
                        debuffsToRemove.Add(debuff);
                        unstableDamage = new DamageInstance(debuff.Damage);
                        hasUnstable = true;
                    }
                }

                foreach (Debuff debuff in debuffsToRemove)
                {
                    debuff.instantiatedEffect.transform.SetParent(null, true);
                    debuff.instantiatedEffect.transform.localScale = new Vector3(1, 1, 1);
                    debuff.instantiatedEffect.GetComponent<ParticleSystem>().Stop();

                    entity.activeDebuffs.Remove(debuff);
                }

                if (hasUnstable)
                {
                    Instantiate(Resources.Load("Effects/UnstableExplosion") as GameObject, entity.Body.position, Quaternion.identity).transform.localScale = Vector3.one * 4f;
                    GameManager.SpawnExplosion(entity.Body.position, 4f, 3f, 3.5f, 0f, unstableDamage, null, ability.Owner);
                }
            }
        }
    }
}