using UnityEngine;
using UnityEngine.Events;

public class RippersCleaverRelic : MonoBehaviour
{
    public EntityRelic relic;
    public float explosionRadius = 5f;
    public GameObject explosionEffect;
    public DamageInstance explosionDamage;

    public DebuffPreset debuffToApply;

    MeleeWeapon lastMelee;

    private void Start()
    {
        lastMelee = relic.owner.meleeWeapon;
        relic.owner.meleeWeapon.OnTargetSevered.AddListener(AddStatus);
        relic.owner.meleeWeapon.OnTargetSplitInHalf.AddListener(StatusExplosion);
    }

    private void OnDestroy()
    {
        relic.owner.meleeWeapon.OnTargetSevered.RemoveListener(AddStatus);
        relic.owner.meleeWeapon.OnTargetSplitInHalf.RemoveListener(StatusExplosion);
    }

    private void Update()
    {
        if (relic.owner.meleeWeapon != lastMelee)
        {
            relic.owner.meleeWeapon.OnTargetSevered.AddListener(AddStatus);
            relic.owner.meleeWeapon.OnTargetSplitInHalf.AddListener(StatusExplosion);
        }
        lastMelee = relic.owner.meleeWeapon;
    }

    void AddStatus(Entity reciever)
    {
        reciever.ApplyDebuff(debuffToApply, relic.owner);
    }

    void StatusExplosion(Vector3 position)
    {
        Instantiate(explosionEffect, position, Quaternion.identity).transform.localScale = Vector3.one * explosionRadius;
        GameManager.SpawnExplosion(position, explosionRadius, explosionRadius, explosionRadius, 0f, explosionDamage, debuffToApply, relic.owner);
    }
}
