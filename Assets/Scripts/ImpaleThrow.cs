using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ImpaleThrow : MonoBehaviour
{
    public MeleeWeapon weapon;
    public float ThrowForce;
    public DamageInstance Damage;
    public void ThrowCorpse()
    {
        List<Entity> entitiesToThrow = new List<Entity>(weapon.impaledEntities);
        weapon.DropCorpses();
        foreach (Entity impaledEntity in entitiesToThrow)
        {
            if (impaledEntity != null)
            {
                impaledEntity.rigidBody.AddForce(weapon.Wielder.Body.forward * ThrowForce * 8f, ForceMode.Impulse);

                PhysicalProjectile projectile = impaledEntity.AddComponent<PhysicalProjectile>();
                projectile.rb = impaledEntity.rigidBody;
                projectile.Owner = weapon.Wielder;
                projectile.speedThreshhold = Mathf.Pow(ThrowForce, 1f / 3f) * 0.1f;
                projectile.Damage = Damage;
                projectile.destroyOnSpeedLoss = true;

                if (impaledEntity.charMovement != null)
                {
                    impaledEntity.transform.rotation = Quaternion.identity;
                    Vector3 knockDir = weapon.Wielder.Body.forward * ThrowForce;
                    impaledEntity.charMovement.appliedForces.Add(knockDir);
                    // Let the physics system handle Y positioning naturally
                    // impaledEntity.transform.position = new Vector3(impaledEntity.transform.position.x, 0.5f, impaledEntity.transform.position.z);
                }
                else
                {
                    SphereCollider col = impaledEntity.AddComponent<SphereCollider>();
                    col.radius = 1f;
                    col.excludeLayers = LayerMask.GetMask("Corpse", "Wall", "Ground");
                }
            }
        }

        weapon.impaledEntities.Clear();
    }
}
