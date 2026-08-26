using UnityEngine;

public class TwinfangMedalRelic : MonoBehaviour
{
    public EntityRelic relic;
    MeleeWeapon lastMelee;

    public float grantRadius = 7f;
    public LayerMask entityMask;
    public DebuffPreset debuffToApply;

    private void Start()
    {
        lastMelee = relic.owner.meleeWeapon;
        relic.owner.meleeWeapon.OnTargetSevered.AddListener(GrantBuff);
        relic.owner.meleeWeapon.OnTargetSplitInHalf.AddListener(GrantBuff);
    }

    private void OnDestroy()
    {
        relic.owner.meleeWeapon.OnTargetSevered.RemoveListener(GrantBuff);
        relic.owner.meleeWeapon.OnTargetSplitInHalf.RemoveListener(GrantBuff);
    }

    private void Update()
    {
        if (relic.owner.meleeWeapon != lastMelee)
        {
            relic.owner.meleeWeapon.OnTargetSevered.AddListener(GrantBuff);
            relic.owner.meleeWeapon.OnTargetSplitInHalf.AddListener(GrantBuff);
        }
        lastMelee = relic.owner.meleeWeapon;
    }

    void GrantBuff(Entity reciever)
    {
        Collider[] colliders = Physics.OverlapSphere(relic.owner.Body.position, grantRadius, entityMask);
        foreach (Collider entityCaught in colliders)
        {
            Entity entityScript = entityCaught.GetComponent<Entity>();
            if(entityScript != null)
            {
                if(entityScript.Team == relic.owner.Team)
                {
                    entityScript.ApplyDebuff(debuffToApply, relic.owner);
                }
            }
        }
    }

    void GrantBuff(Vector3 pos)
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
