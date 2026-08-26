using UnityEngine;

public class AwakenAbility : MonoBehaviour
{
    public Ability ability;
    public AbilitySliderController abilitySliderController;
    public SoulChargeHandler soulChargeHandler;
    public Animator anim;
    public string AwakenAnimName = "KnightSpawnClone";

    private void Update()
    {
        if (soulChargeHandler.SoulCharge >= soulChargeHandler.maxSoulCharge)
        {
            abilitySliderController.abilitySlider.value = 1f;
            abilitySliderController.abilitySlider.maxValue = 1f;
        }
        else
        {
            abilitySliderController.abilitySlider.value = 0f;
            abilitySliderController.abilitySlider.maxValue = 1f;
        }
    }

    public void Cast()
    {
        if (soulChargeHandler.SoulCharge >= soulChargeHandler.maxSoulCharge)
        {
            anim.CrossFade(AwakenAnimName, 0.1f);
            soulChargeHandler.SoulCharge = 0;
            ability.CastAbilityOnOwner();
        }
    }
}