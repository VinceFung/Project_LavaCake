using UnityEngine;
using static UnityEngine.Rendering.BoolParameter;

public class BloodEngineRelic : MonoBehaviour
{
    public EntityRelic relic;
    public float grantRadius;
    public LayerMask entityMask;
    public BuffDisplay.DisplayTypes displayType;
    public DebuffPreset debuffToApply;

    MeleeWeapon lastMelee;

    private void Start()
    {
        lastMelee = relic.owner.meleeWeapon;
        relic.owner.OnKill.AddListener(GrantBuff);
        relic.owner.OnDamageTaken.AddListener(GrantBuff);
    }

    private void OnDestroy()
    {
        relic.owner.OnKill.RemoveListener(GrantBuff);
        relic.owner.OnDamageTaken.RemoveListener(GrantBuff);
        relic.owner.meleeWeapon.OnTargetSevered.RemoveListener(GrantBuff);

    }

    private void Update()
    {
        if (relic.owner.meleeWeapon != lastMelee)
        {
            relic.owner.meleeWeapon.OnTargetSevered.AddListener(GrantBuff);
        }
        lastMelee = relic.owner.meleeWeapon;
    }

    void GrantBuff(Entity reciever)
    {
        Collider[] colliders = Physics.OverlapSphere(relic.owner.Body.position, grantRadius, entityMask);
        foreach (Collider entityCaught in colliders)
        {
            Entity entityScript = entityCaught.GetComponent<Entity>();
            if (entityScript != null)
            {
                if (entityScript.Team == relic.owner.Team)
                {
                    entityScript.ApplyDebuff(debuffToApply, relic.owner);
                }
            }
        }
    }

    void GrantBuff()
    {
        Collider[] colliders = Physics.OverlapSphere(relic.owner.Body.position, grantRadius, entityMask);
        foreach (Collider entityCaught in colliders)
        {
            Entity entityScript = entityCaught.GetComponent<Entity>();
            if (entityScript != null)
            {
                if (entityScript.Team == relic.owner.Team)
                {
                    entityScript.ApplyDebuff(debuffToApply, relic.owner);
                }
            }
        }
    }
}
