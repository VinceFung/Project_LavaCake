using UnityEngine;

public class BuffOnKillRelic : MonoBehaviour
{
    public EntityRelic relic;
    public DebuffPreset debuffToApply;

    private void Start()
    {
        relic.owner.OnKill.AddListener(IncreaseStack);
        relic.owner.OnAbilityCast.AddListener(IncreaseStack);
    }

    private void OnDestroy()
    {
        relic.owner.OnKill.RemoveListener(IncreaseStack);
        relic.owner.OnAbilityCast.RemoveListener(IncreaseStack);
    }

    public void IncreaseStack(Entity d)
    {
        relic.owner.ApplyDebuff(debuffToApply, relic.owner);
    }

    public void IncreaseStack()
    {
        relic.owner.ApplyDebuff(debuffToApply, relic.owner);
    }
}
