using UnityEngine;

public class FinalSurgeRelic : MonoBehaviour
{
    public EntityRelic relic;
    public DebuffPreset debuffToApply;

    void Update()
    {
        if(relic.owner.entityGun.Ammo >= 2)
        {
            relic.owner.ApplyDebuff(debuffToApply, relic.owner);
        }
    }
}
