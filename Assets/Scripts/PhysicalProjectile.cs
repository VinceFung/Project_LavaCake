using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalProjectile : MonoBehaviour
{
    public Entity Owner;
    public float speedThreshhold;
    public DamageInstance Damage;
    public Rigidbody rb;

    public List<Entity> entitiesHit = new List<Entity>();

    public bool destroyOnSpeedLoss;

    private void Start()
    {
        if(rb == null) rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        //Debug.Log($"Speed: {rb.linearVelocity.magnitude} Threshhold: {speedThreshhold}");
        if (rb.linearVelocity.magnitude <= 0.5f)
        {
            if (destroyOnSpeedLoss)
            {
                Destroy(this);
            }
            entitiesHit.Clear();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (rb.linearVelocity.magnitude >= speedThreshhold)
        {
            Entity hitEntity = collision.gameObject.GetComponent<Entity>();
            if (hitEntity != null)
            {
                if (!entitiesHit.Contains(hitEntity))
                {
                    DamageInstance scaledDamage = new DamageInstance(Damage)
                    {
                        HealthDamage = Damage.HealthDamage * rb.linearVelocity.magnitude/10f,
                        StaggerDamage = Damage.StaggerDamage * rb.linearVelocity.magnitude/10f,
                    };
                    hitEntity.TakeDamage(scaledDamage, Owner, false);
                    entitiesHit.Add(hitEntity);
                }
            }
        }
    }
}
