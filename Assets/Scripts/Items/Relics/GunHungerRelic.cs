using UnityEngine;

public class GunHungerRelic : MonoBehaviour
{
    public EntityRelic relic;
    public DebuffPreset gunHungerDebuff;

    private void Start()
    {
        relic.owner.OnGunDamageDealt.AddListener(ApplyBuff);
    }

    private void OnDestroy()
    {
        relic.owner.OnGunDamageDealt.RemoveListener(ApplyBuff);
    }

    void ApplyBuff(Entity entityHit)
    {
        relic.owner.ApplyDebuff(gunHungerDebuff, relic.owner);
    }
}
