using UnityEngine;

public class MaskOfViolenceRelic : MonoBehaviour
{
    public EntityRelic relic;
    public DebuffPreset debuffToApply;

    void Update()
    {
        if (relic.owner.timeSinceDirectDamageTaken >= 10f)
        {
            relic.owner.ApplyDebuff(debuffToApply, relic.owner);
        }
    }
}
