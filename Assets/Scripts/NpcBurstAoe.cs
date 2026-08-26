using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NpcBurstAoe : MonoBehaviour
{
    public Entity Wielder;
    public DamageInstance AoeDamage;
    public Transform AttackPoint;
    public Vector3 HitboxSize;
    public GameObject hitEffect;

    [System.Serializable]
    public class Pulse
    {
        public float pulseDamageMultiplier = 1f;
        public float pulseDelay;
    }
    public Pulse[] pulses;

    public void StartBurst()
    {
        for (int i = 0; i < pulses.Length; i++)
        {
            StartCoroutine(Burst(pulses[i].pulseDamageMultiplier, pulses[i].pulseDelay));
        }
    }

    IEnumerator Burst(float dmgMultiplier, float delay)
    {
        yield return new WaitForSeconds(delay);
        Collider[] enemiesHit = Physics.OverlapBox(AttackPoint.position, HitboxSize, AttackPoint.rotation);
        foreach (Collider item in enemiesHit)
        {
            Entity itemEntity = item.GetComponent<Entity>();
            if (itemEntity != null)
            {
                if (itemEntity != Wielder)
                {
                    Vector3 knockDir = new Vector3(itemEntity.rigidBody.transform.position.x - Wielder.transform.position.x, 0f, itemEntity.rigidBody.transform.position.z - Wielder.transform.position.z).normalized;
                    DamageInstance dmgInstance = new DamageInstance(AoeDamage)
                    {
                        knockbackDir = knockDir,
                        knockbackAmount = AoeDamage.knockbackAmount,
                        gunChargeMultiplier = AoeDamage.gunChargeMultiplier,
                        FinalSeverenceDamageMultiplier = AoeDamage.FinalSeverenceDamageMultiplier,

                        HealthDamage = AoeDamage.HealthDamage *  Wielder.DamageMultiplier * dmgMultiplier,
                        StaggerDamage = AoeDamage.StaggerDamage *  Wielder.DamageMultiplier * dmgMultiplier,
                        SeverenceDamage = AoeDamage.SeverenceDamage * Wielder.DamageMultiplier * dmgMultiplier,
                    };

                    if ((itemEntity.EntityType == Entity.EntityTypes.Character && (itemEntity.Health - dmgInstance.HealthDamage) <= 0))
                    {
                        dmgInstance.knockbackAmount = (dmgInstance.knockbackAmount + 10f);
                    }
                    else if (itemEntity.EntityType == Entity.EntityTypes.Corpse)
                    {
                        dmgInstance.knockbackDir = (knockDir + new Vector3(0f, 0.5f, 0f)).normalized;
                        dmgInstance.knockbackAmount = (dmgInstance.knockbackAmount + 5f);
                    }

                    itemEntity.TakeDamage(dmgInstance, Wielder, false);
                    if (!itemEntity.GetImmunityStatus())
                    {
                        if (itemEntity.EntityType != Entity.EntityTypes.Corpse) Instantiate(hitEffect, itemEntity.Body.transform.position, Quaternion.identity);
                    }
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (AttackPoint != null)
        {
            Gizmos.color = Color.red;
            // Save the current Gizmos matrix
            Matrix4x4 oldMatrix = Gizmos.matrix;
            // Set the Gizmos matrix to match the lightAttackPoint's transform
            Gizmos.matrix = Matrix4x4.TRS(AttackPoint.position, AttackPoint.rotation, Vector3.one);
            // Draw the wire cube at the origin (since the matrix handles the transform)
            Gizmos.DrawWireCube(Vector3.zero, HitboxSize);
            // Restore the original Gizmos matrix
            Gizmos.matrix = oldMatrix;
        }
    }
}
