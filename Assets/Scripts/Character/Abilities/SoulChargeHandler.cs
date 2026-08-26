using UnityEngine;
using UnityEngine.UI;

public class SoulChargeHandler : MonoBehaviour
{
    public Entity entity;

    public float maxSoulCharge = 100f;
    public float SoulCharge = 0f;

    public float gainPerDebuff = 10f;

    public Slider soulChargeSlider;
    public GameObject fullIndicator;

    private void Start()
    {
        entity.OnStatusApplied.AddListener(GainSoulCharge);
    }

    private void Update()
    {
        soulChargeSlider.value = SoulCharge;
        soulChargeSlider.maxValue = maxSoulCharge;
        fullIndicator.SetActive(SoulCharge >= maxSoulCharge);
    }

    void GainSoulCharge(Entity entityHit, Debuff debuffApplied)
    {
        if(entityHit.EntityType == Entity.EntityTypes.Character)
        {
            if(debuffApplied != null)
            {
                if (debuffApplied.DebuffName == "Soul Touched")
                {
                    if (SoulCharge < maxSoulCharge)
                    {
                        SoulCharge += gainPerDebuff;
                        if (SoulCharge > maxSoulCharge)
                        {
                            SoulCharge = maxSoulCharge;
                        }
                    }
                }
            }
        }
    }
}
