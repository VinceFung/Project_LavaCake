using UnityEngine;
using UnityEngine.Events;
using static Debuff;

[CreateAssetMenu(menuName = "Status/DebuffPreset")]
public class DebuffPreset : ScriptableObject
{
    public string DebuffName;
    public DebuffTypes debuffType = DebuffTypes.None;
    public StackTypes stackType = StackTypes.DoesNotStack;
    public int MaxStacks = 1;

    [Header("Visual")]

    public BuffDisplay.DisplayTypes topDisplay;
    public BuffDisplay.DisplayTypes bottomDisplay;
    public Sprite icon;
    public GameObject debuffEffect;

    [Header("Damage")]

    public float TickRate = 1f;
    public DamageInstance Damage;

    [Header("Removal Conditions")]

    public float Duration = 5f;
    public bool transfersToCorpse = true;
    public bool removedOnRequirementMet = false;
    public float damageRequirement = 0f;

    [Header("Stat Modifiers")]

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
}