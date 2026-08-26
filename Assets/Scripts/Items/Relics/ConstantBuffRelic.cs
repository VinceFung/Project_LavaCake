using UnityEngine;

public class ConstantBuffRelic : MonoBehaviour
{
    public EntityRelic relic;
    public DebuffPreset debuffToApply;

    void Update()
    {
        relic.owner.ApplyDebuff(debuffToApply, relic.owner);
    }
}
