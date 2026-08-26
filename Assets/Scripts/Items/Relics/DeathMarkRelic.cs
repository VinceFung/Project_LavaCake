using UnityEngine;

public class DeathMarkRelic : MonoBehaviour
{
    public EntityRelic relic;
    public float markRadius;
    public DebuffPreset deathMarkDebuff;

    private void Start()
    {
        relic.owner.OnGunDamageDealt.AddListener(MarkForDeath);
    }

    private void OnDestroy()
    {
        relic.owner.OnGunDamageDealt.RemoveListener(MarkForDeath);
    }

    void MarkForDeath(Entity entityHit)
    {
        Collider[] cols = Physics.OverlapSphere(entityHit.transform.position, markRadius);
        foreach (Collider colHit in cols)
        {
            Entity hitEntity = colHit.GetComponent<Entity>();
            if (hitEntity != null) 
            {
                if(hitEntity.Team != relic.owner.Team)
                {
                    hitEntity.ApplyDebuff(deathMarkDebuff, relic.owner);
                }
            }
        }
    }
}
