using UnityEngine;

public class StormEngineRelic : MonoBehaviour
{
    public EntityRelic relic;

    public string BuffName;
    public Sprite BuffIcon;

    public float movementBuildUpMultiplier = 2.5f;
    public float dashBuildUpMultiplier = 5f;
    float staticBuildUp;

    public float pulseChargeBuildUpRequirement = 50f;
    public float pulseRadius = 5f;
    public DamageInstance pulseDamage;
    public GameObject pulseEffect;
    public DebuffPreset pulseDebuff;

    public float buildUpOnHit;
    float buildUp;

    public float weaponToStatusDmgMultiplier;
    public DebuffPreset debuffToApply;

    Vector3 lastFramePos;

    BuffDisplay instantiatedDisplay;

    bool dashActivated;
    bool triggerPulse;

    int pulseChargesStored;
    float pulseChargeMeter;

    MeleeWeapon lastMelee;

    private void Start()
    {
        lastMelee = relic.owner.meleeWeapon;
        lastFramePos = relic.owner.transform.position;
        relic.owner.meleeWeapon.stats.OnDirectDamageDealt.AddListener(OnHit);
    }

    private void OnDestroy()
    {
        relic.owner.meleeWeapon.stats.OnDirectDamageDealt.RemoveListener(OnHit);

        if(instantiatedDisplay != null)
        {
            Destroy(instantiatedDisplay.gameObject);
        }
    }

    private void Update()
    {
        if(relic.owner.meleeWeapon != lastMelee)
        {
            relic.owner.meleeWeapon.stats.OnDirectDamageDealt.AddListener(OnHit);
        }
        lastMelee = relic.owner.meleeWeapon;

        if (buildUpOnHit < 100f)
        {
            if (relic.owner.charMovement.dashDur > 0)
            {
                staticBuildUp += Vector3.Distance(relic.owner.transform.position, lastFramePos) * dashBuildUpMultiplier * relic.owner.MovementSpeedMultiplier;
            }
            else
            {
                staticBuildUp += Vector3.Distance(relic.owner.transform.position, lastFramePos) * movementBuildUpMultiplier;
            }

            while (staticBuildUp >= 1f)
            {
                pulseChargeMeter += 1f;
                staticBuildUp -= 1f;
                buildUpOnHit += 1f;
            }
        }
        else
        {
            buildUpOnHit = 100f;
        }

        if(pulseChargeMeter >= pulseChargeBuildUpRequirement)
        {
            pulseChargeMeter -= pulseChargeBuildUpRequirement;
            pulseChargesStored++;
        }

        if (relic.owner.charMovement.dashDur > 0 && dashActivated == false)
        {
            dashActivated = true;
            if(pulseChargesStored > 0)
            {
                triggerPulse = true;
            }
        }

        if(dashActivated && relic.owner.charMovement.dashDur <= 0)
        {
            dashActivated = false;
            OnDashEnd();
        }

            lastFramePos = relic.owner.transform.position;

        if (instantiatedDisplay == null)
        {
            instantiatedDisplay = Instantiate(Resources.Load("BuffDisplay") as GameObject, relic.owner.BuffBar).GetComponent<BuffDisplay>();
            instantiatedDisplay.BuffIconImage.sprite = BuffIcon;
            instantiatedDisplay.TopDisplay = BuffDisplay.DisplayTypes.CustomB;
            instantiatedDisplay.BottemDisplay = BuffDisplay.DisplayTypes.CustomA;

            instantiatedDisplay.customDisplayA = buildUpOnHit + "%";
            instantiatedDisplay.customDisplayB = pulseChargesStored.ToString();
        }
        else
        {
            instantiatedDisplay.customDisplayA = buildUpOnHit + "%";
            instantiatedDisplay.customDisplayB = pulseChargesStored.ToString();
        }
    }

    void OnDashEnd()
    {
        if(pulseChargesStored > 0 && triggerPulse)
        {
            GameManager.SpawnExplosion(relic.owner.Body.position, pulseRadius, pulseRadius, pulseRadius, 0f, pulseDamage, pulseDebuff, relic.owner);
            Instantiate(pulseEffect, relic.owner.Body.position, Quaternion.identity).transform.localScale = Vector3.one * pulseRadius;
            pulseChargesStored--;
            triggerPulse = false;
        }
    }

    void OnHit(Entity reciever, DamageInstance dmg, bool heavy)
    {
        if (reciever.EntityType == Entity.EntityTypes.Character)
        {
            relic.owner.meleeWeapon.AddStatusBuildUp(debuffToApply, weaponToStatusDmgMultiplier, buildUpOnHit, instance =>
            {
                instance.Damage.HealthDamage = relic.owner.meleeWeapon.lightDamage.HealthDamage * relic.owner.meleeWeapon.stats.DamageMultiplier * relic.owner.meleeWeapon.Wielder.DamageMultiplier * weaponToStatusDmgMultiplier;
            });

            buildUpOnHit = 0f;
        }
    }
}
