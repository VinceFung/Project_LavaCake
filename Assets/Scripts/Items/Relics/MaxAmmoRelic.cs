using UnityEngine;

public class MaxAmmoRelic : MonoBehaviour
{
    public EntityRelic relic;
    public int bonusAmmo = 1;
    public float DamageRequirementDecrease = 0f;

    bool bonusGranted = false;

    private void Start()
    {
        if (!bonusGranted)
        {
            relic.owner.entityGun.MaxAmmo += bonusAmmo;
            relic.owner.entityGun.AmmoChargeRequirement -= DamageRequirementDecrease;
            bonusGranted = true;
        }
    }

    private void OnDestroy()
    {
        if (bonusGranted)
        {
            relic.owner.entityGun.MaxAmmo -= bonusAmmo;
            relic.owner.entityGun.AmmoChargeRequirement += DamageRequirementDecrease;
        }
    }
}
