using UnityEngine;

public class BuffOnHealRelic : MonoBehaviour
{
    public EntityRelic relic;

    public float grantRadius = 7f;
    public LayerMask entityMask;
    public DebuffPreset debuffToApply;

    private void Start()
    {
        relic.owner.transform.GetComponent<PlayerHealthPotion>().OnHeal.AddListener(GrantBuff);
    }

    private void OnDestroy()
    {
        relic.owner.transform.GetComponent<PlayerHealthPotion>().OnHeal.RemoveListener(GrantBuff);
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
