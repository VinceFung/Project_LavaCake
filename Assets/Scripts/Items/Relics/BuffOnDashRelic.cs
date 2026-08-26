using UnityEngine;

public class BuffOnDashRelic : MonoBehaviour
{
    public EntityRelic relic;
    public float grantRadius;
    public LayerMask entityMask;
    public BuffDisplay.DisplayTypes displayType;
    public DebuffPreset debuffToApply;

    bool dashActivated;

    private void Update()
    {
        if (relic.owner.charMovement.dashDur > 0 && dashActivated == false)
        {
            dashActivated = true;
        }

        if (dashActivated && relic.owner.charMovement.dashDur <= 0)
        {
            dashActivated = false;
            OnDashEnd();
        }
    }

    void OnDashEnd()
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
