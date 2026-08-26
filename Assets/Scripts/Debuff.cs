using UnityEngine;

[System.Serializable]
public class Debuff
{
    public DebuffPreset instancePreset;
    public string DebuffName;
    public Sprite displayIcon;
    public BuffDisplay.DisplayTypes topDisplay;
    public BuffDisplay.DisplayTypes bottomDisplay;
    public enum DebuffTypes
    {
        None, Fire, Electric, Poison, Cold, Bleed, Unstable
    }
    public DebuffTypes debuffType = DebuffTypes.None;
    public enum StackTypes
    {
        DoesNotStack, RefreshDuration, IncreaseDuration, IncreasePowerAndRefresh, IncreasePowerAndDuration
    }
    public StackTypes stackType = StackTypes.DoesNotStack;
    public Entity Applier;
    public int Stacks;
    public float Duration;
    public float nextTickTime;
    public float damageRequirement = 0f;
    public float debuffDamageTaken;
    public bool transfersToCorpse;
    public bool oneTimeEffectActivated;
    public bool removedOnRequirementMet;
    public GameObject debuffEffect;
    public GameObject instantiatedEffect;

    public float TickRate;
    public DamageInstance Damage;

    public float debuffDamageInputMultiplier = 0f;
    public float debuffDamageMultiplier = 0f;
    public float debuffHealingMultiplier = 0f;
    public float debuffAttackSpeedMultiplier = 0f;
    public float debuffMovementSpeedMultiplier = 0f;
    public float debuffStatusBuildUpMultiplier = 0f;
    public float debuffStatusDamageMultiplier = 0f;
    public float debuffGunChargeMultiplier = 0f;
    public float debuffStaminaEfficiencyMultiplier = 0f;
    public float debuffAbilityDamageMultiplier = 0f;
    public float debuffResourceGainMultiplier = 0f;

    public Debuff(DebuffPreset preset, Entity applier)
    {
        this.instancePreset = preset;
        if(preset != null)
        {
            this.displayIcon = preset.icon;
            this.DebuffName = preset.DebuffName;
            this.topDisplay = preset.topDisplay;
            this.bottomDisplay = preset.bottomDisplay;
            this.debuffType = preset.debuffType;
            this.stackType = preset.stackType;
            this.transfersToCorpse = preset.transfersToCorpse;
            this.removedOnRequirementMet = preset.removedOnRequirementMet;
            this.damageRequirement = preset.damageRequirement;
            this.debuffDamageInputMultiplier = preset.debuffDamageInputMultiplier;
            this.debuffDamageMultiplier = preset.debuffDamageMultiplier;
            this.debuffHealingMultiplier = preset.debuffHealingMultiplier;
            this.debuffAttackSpeedMultiplier = preset.debuffAttackSpeedMultiplier;
            this.debuffMovementSpeedMultiplier = preset.debuffMovementSpeedMultiplier;
            this.debuffStatusBuildUpMultiplier = preset.debuffStatusBuildUpMultiplier;
            this.debuffStatusDamageMultiplier = preset.debuffStatusDamageMultiplier;
            this.debuffGunChargeMultiplier = preset.debuffGunChargeMultiplier;
            this.debuffStaminaEfficiencyMultiplier = preset.debuffStaminaEfficiencyMultiplier;
            this.debuffAbilityDamageMultiplier = preset.debuffAbilityDamageMultiplier;
            this.debuffResourceGainMultiplier = preset.debuffResourceGainMultiplier;
            this.debuffEffect = preset.debuffEffect;
            this.Stacks = 1;
            this.Applier = applier;
            this.Duration = preset.Duration;
            this.TickRate = preset.TickRate;
            this.Damage = preset.Damage;
            this.nextTickTime = Time.time + TickRate;
        }
    }
}