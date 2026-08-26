using UnityEngine;
using UnityEngine.Events;

public class Ability : MonoBehaviour
{
    public AbilitySliderController abilitySliderController;
    public Entity Owner;
    public float AbilityEnergyRequirement = 100f;
    public float currentAbilityEnergy = 100f;
    public float AbilityRegenSpeed = 10f;

    public bool consumeEnergyOnCast = true;
    public UnityEvent<Vector3> OnAbilityCast;

    private void Update()
    {
        if (currentAbilityEnergy < AbilityEnergyRequirement)
        {
            currentAbilityEnergy += AbilityRegenSpeed * Time.deltaTime;
            if (currentAbilityEnergy > AbilityEnergyRequirement)
            {
                currentAbilityEnergy = AbilityEnergyRequirement;
            }
        }

        if (abilitySliderController != null)
        {
            abilitySliderController.abilitySlider.value = currentAbilityEnergy;
            abilitySliderController.abilitySlider.maxValue = AbilityEnergyRequirement;
        }
    }

    public void CastAbility(Vector3 pos)
    {
        if (currentAbilityEnergy < AbilityEnergyRequirement)
        {
            return;
        }
        OnAbilityCast.Invoke(pos);
        if (consumeEnergyOnCast) currentAbilityEnergy = 0f;
    }

    public void CastAbilityOnOwner()
    {
        if (currentAbilityEnergy < AbilityEnergyRequirement)
        {
            return;
        }
        OnAbilityCast.Invoke(Owner.Body.position);
        if (consumeEnergyOnCast) currentAbilityEnergy = 0f;
    }
}
