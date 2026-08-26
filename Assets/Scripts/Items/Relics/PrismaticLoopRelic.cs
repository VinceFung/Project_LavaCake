using UnityEngine;

public class PrismaticLoopRelic : MonoBehaviour
{
    public EntityRelic relic;
    public DebuffPreset debuffToApply;

    private void Start()
    {
        relic.owner.OnStatusApplied.AddListener(IncreaseStack);
    }

    private void OnDestroy()
    {
        relic.owner.OnStatusApplied.RemoveListener(IncreaseStack);
    }

    void IncreaseStack(Entity reciever, Debuff appliedStatus)
    {
        relic.owner.ApplyDebuff(debuffToApply, relic.owner);
    }
}
