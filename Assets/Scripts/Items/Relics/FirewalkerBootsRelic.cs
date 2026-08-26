using System.Security.Principal;
using UnityEngine;

public class FirewalkerBootsRelic : MonoBehaviour
{
    public EntityRelic relic;
    public string BuffName;
    public Sprite BuffIcon;
    int currentStacks;
    public int maxStacks = 5;
    public float duration = 6f;
    float currentDuration;

    BuffDisplay instantiatedDisplay;

    public float pulseSpeed = 4f;
    public float pulseSpeedDecreasePerStack = 0.5f;
    float nextTimeToPulse;
    public float pulseRadius = 5f;
    public DamageInstance pulseDamage;
    public GameObject pulseEffect;
    public DebuffPreset pulseDebuff;

    public float buildUpOnHit;
    float buildUp;

    public float weaponToStatusDmgMultiplier;
    public DebuffPreset debuffToApply;

    MeleeWeapon lastMelee;

    private void Start()
    {
        lastMelee = relic.owner.meleeWeapon;
        relic.owner.OnKill.AddListener(IncreaseStack);
        relic.owner.OnAbilityCast.AddListener(IncreaseStack);
        relic.owner.meleeWeapon.stats.OnDirectDamageDealt.AddListener(OnHit);
    }

    private void OnDestroy()
    {
        relic.owner.OnKill.RemoveListener(IncreaseStack);
        relic.owner.OnAbilityCast.RemoveListener(IncreaseStack);
        relic.owner.meleeWeapon.stats.OnDirectDamageDealt.RemoveListener(OnHit);

        if (instantiatedDisplay != null)
        {
            Destroy(instantiatedDisplay.gameObject);
        }
    }

    private void Update()
    {
        if (relic.owner.meleeWeapon != lastMelee)
        {
            relic.owner.meleeWeapon.stats.OnDirectDamageDealt.AddListener(OnHit);
        }
        lastMelee = relic.owner.meleeWeapon;

        if (currentStacks > maxStacks)
        {
            currentStacks = maxStacks;
        }

        currentDuration -= Time.deltaTime;
        if (currentDuration <= 0)
        {
            currentStacks = 0;
        }

        if (instantiatedDisplay != null)
        {
            instantiatedDisplay.debuff.Duration = currentDuration;
            instantiatedDisplay.customDisplayA = currentStacks.ToString();

            if (instantiatedDisplay.debuff.Duration <= 0)
            {
                Destroy(instantiatedDisplay.gameObject);
            }
        }

        if (currentStacks > 0)
        {
            if (Time.time >= nextTimeToPulse)
            {
                nextTimeToPulse = Time.time + pulseSpeed - (pulseSpeedDecreasePerStack * currentStacks);
                GameManager.SpawnExplosion(relic.owner.Body.position, pulseRadius, pulseRadius, pulseRadius, 0f, pulseDamage, pulseDebuff, relic.owner);
                Instantiate(pulseEffect, relic.owner.Body.position, Quaternion.identity).transform.localScale = Vector3.one * pulseRadius;
            }
        }
    }

    public void IncreaseStack(Entity d)
    {
        currentDuration = duration;
        currentStacks++;

        if(instantiatedDisplay == null)
        {
            instantiatedDisplay = Instantiate(Resources.Load("BuffDisplay") as GameObject, relic.owner.BuffBar).GetComponent<BuffDisplay>();
            instantiatedDisplay.TopDisplay = BuffDisplay.DisplayTypes.Duration;
            instantiatedDisplay.BottemDisplay = BuffDisplay.DisplayTypes.CustomA;
            instantiatedDisplay.BuffIconImage.sprite = BuffIcon;
        }
    }

    public void IncreaseStack()
    {
        currentDuration = duration;
        currentStacks++;

        if (instantiatedDisplay == null)
        {
            instantiatedDisplay = Instantiate(Resources.Load("BuffDisplay") as GameObject, relic.owner.BuffBar).GetComponent<BuffDisplay>();
            instantiatedDisplay.TopDisplay = BuffDisplay.DisplayTypes.Duration;
            instantiatedDisplay.BottemDisplay = BuffDisplay.DisplayTypes.CustomA;
            instantiatedDisplay.BuffIconImage.sprite = BuffIcon;
        }
    }

    void OnHit(Entity reciever, DamageInstance dmg, bool heavy)
    {
        if (reciever.EntityType == Entity.EntityTypes.Character)
        {
            if (reciever.EntityType == Entity.EntityTypes.Character)
            {
                relic.owner.meleeWeapon.AddStatusBuildUp(debuffToApply, weaponToStatusDmgMultiplier, buildUpOnHit * currentStacks, instance =>
                {
                    instance.Damage.HealthDamage = relic.owner.meleeWeapon.lightDamage.HealthDamage * relic.owner.meleeWeapon.stats.DamageMultiplier * relic.owner.meleeWeapon.Wielder.DamageMultiplier * weaponToStatusDmgMultiplier;
                });
            }
        }
    }
}
