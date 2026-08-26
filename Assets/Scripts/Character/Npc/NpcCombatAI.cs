using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
//using System;

public class NpcCombatAI : MonoBehaviour
{
    public TextMeshProUGUI stateText;
    public Entity entity;
    public NpcMovement movement;
    public CharacterTargeting targeting;
    public CharacterCombatHandler Target;

    [Header("Voice")]
    public float voiceVolume = 1f;
    public float maxVoicelineCooldown = 11f;
    public float minVoicelineCooldown = 7f;
    float voicelineCooldownTimeStamp = 0f;
    public AudioClip[] voiceLines;

    [Header("Target Detection")]
    public float smellRadius = 3f;
    public float viewRadius = 30f;
    public float viewAngle = 70f;
    public float timeForAlert = 1f;
    public float alertOthersRadius = 10f;
    public LayerMask ObstacleMask;
    public LayerMask characterMask;
    List<Transform> visibleTargets = new List<Transform>();

    [Space(20f)]

    public int heat = 4;
    public float baseAttackScore;
    public float attackScore;

    public enum MovementStates
    {
        Patrol, Stay, Strafe, MoveBackward, MoveForward,
    }
    public MovementStates movementState;
    public bool Alerted;
    public bool selectedAttacker;

    [Space(5)]
    public float patrolRadius = 10f;
    public float patrolMoveSpeed = 1f;
    public float forwardMoveSpeed = 1f;
    public float backwardMoveSpeed = 0.5f;
    public float horizontalMoveSpeed = 0.7f;

    [Space(10f)]

    [Range(-1f, 1f)]
    public float Fear = 0f;
    public float fearGainMultiplier = 1f;
    public float fearLossMultiplier = 1f;

    Vector3 dirToTarg;
    Vector3 lastScenePos;
    bool HasLineOfSight = false;

    [HideInInspector]
    public Vector3 spawnPos;
    float randIdleSeed;

    float randStrafeDir = 1f;

    // Magic numbers as constants
    const float MaxTargetDistance = 999999f;
    const float StrafeDistance = 10f;
    const float PatrolNoiseDistance = 10f;

    public int NpcPhase;
    [Header("Action Energy")]
    public float maxActionEnergy = 100f;
    public float actionEnergy = 100f;
    public float actionEnergyRefillCooldown = 3f;
    float actionEnergyRefillTimeStamp = 0f;

    [System.Serializable]
    public class ActionConditionBool
    {
        public enum BoolsToCheck
        {
            LineOfSight,
            IsAttacker
        }
        public BoolsToCheck boolToCheck;
        public bool requiredState;
    }

    [System.Serializable]
    public class ActionConditionFloat
    {
        public enum FloatsToCheck
        {
            Distance,
            ActionEnergy,
            AttackScore,
            HealthPercentage,
            Phase,
            Fear
        }
        public FloatsToCheck floatToCheck;
        public enum ComparisonOperators
        {
            Equals, GreaterThan, LessThan
        }
        public ComparisonOperators comparisionType;
        public float comparedTo;
    }

    [System.Serializable]
    public class MovementAction
    {
        public string MovementActionName;
        public ActionConditionBool[] selectionBoolConditions;
        public ActionConditionFloat[] selectionFloatConditions;
        public MovementStates actionState;
        public float Duration;
        public float Cooldown;
        [Space(5)]
        public float weight;
        [HideInInspector]
        public float cooldownTimeStamp;
    }

    [System.Serializable]
    public class AttackAction
    {
        public string AttackActionName;
        public ActionConditionBool[] selectionBoolConditions;
        public ActionConditionFloat[] selectionFloatConditions;
        public MovementStates actionState;
        public float Duration;
        public bool executeOnDurationEnd = true;
        public ActionConditionBool[] triggerBoolConditions;
        public ActionConditionFloat[] triggerFloatConditions;
        public UnityEvent attackEvent;

        public float Cooldown;
        public float ActionEnergyCost = 10f;
        [Space(5)]
        public float weight;
        [HideInInspector]
        public float cooldownTimeStamp;

        public string[] followUpActions;
        // Shared cooldown group (leave empty for no shared cooldown)
        public string sharedCooldownGroup;
    }

    public MovementAction[] movementActions;
    public AttackAction[] attackActions;

    public float behaviourDuration;

    AttackAction selectedAttackAction = null;
    AttackAction lastSelectedAttackAction = null;

    int losCap = 1;
    int losUpdate = 0;

    float internalActionCooldown;

    // Shared cooldowns for attack actions
    private Dictionary<string, float> sharedCooldowns = new Dictionary<string, float>();

    private void Start()
    {
        randIdleSeed = Random.Range(0f, 999f);
        spawnPos = transform.position;

        entity.OnDamageTaken.AddListener(RetargetOnDamage);
    }

    private void Update()
    {
        if (stateText != null)
        {
            stateText.text = movementState.ToString();
        }

        if (Target != null)
        {
            attackScore = baseAttackScore + 10f / Vector3.Distance(entity.Body.position, Target.entity.Body.position);
            dirToTarg = Target.entity.Body.position - entity.Body.position;
            // Keep only horizontal direction for movement calculations
            dirToTarg.y = 0;

            if (!HasLineOfSight)
            {
                FindTargets();
                SetTarget();
            }
        }
        else
        {
            Alerted = false;
            selectedAttacker = false;
            FindTargets();
            SetTarget();
            if (losUpdate >= losCap)
            {
                losUpdate = 0;
            }
            else
            {
                losUpdate++;
            }
        }

        if (Alerted)
        {
            if (Target != null)
            {
                if (!Target.Opponents.Contains(this))
                {
                    Target.Opponents.Add(this);
                }

                selectedAttacker = Target.currentlyAttacking.Contains(this);

                if (behaviourDuration <= 0 && !entity.meleeWeapon.IsAttacking && !entity.Staggered)
                {
                    if (selectedAttackAction != null && selectedAttackAction.executeOnDurationEnd)
                    {
                        PlayVoiceline();
                        selectedAttackAction.attackEvent.Invoke();
                        lastSelectedAttackAction = selectedAttackAction;
                        internalActionCooldown = Time.time + 0.1f;
                        selectedAttackAction = null;
                    }
                    else if (!entity.meleeWeapon.IsAttacking && entity.charMovement.dashDur <= 0f && Time.time >= internalActionCooldown)
                    {
                        ChooseBehaviour();
                    }
                }
            }

            if (selectedAttackAction != null)
            {
                TryExecuteAttack();
            }

            CalculateLastScenePos();
        }

        HandleStates();

        behaviourDuration -= Time.deltaTime;

        if(actionEnergy < maxActionEnergy && entity.charMovement.dashDur <= 0f && !entity.meleeWeapon.IsAttacking)
        {
            if (Time.time >= actionEnergyRefillTimeStamp)
            {
                actionEnergy = maxActionEnergy;
                actionEnergyRefillTimeStamp = Time.time + actionEnergyRefillCooldown;
            }
        }
        else
        {
            actionEnergyRefillTimeStamp = Time.time + actionEnergyRefillCooldown;
        }
    }

    void TryExecuteAttack()
    {
        if (selectedAttackAction == null)
            return;

        bool allConditionsMet = true;

        // Bool conditions
        if (selectedAttackAction.triggerBoolConditions != null && selectedAttackAction.triggerBoolConditions.Length > 0)
        {
            foreach (ActionConditionBool condition in selectedAttackAction.triggerBoolConditions)
            {
                switch (condition.boolToCheck)
                {
                    case ActionConditionBool.BoolsToCheck.LineOfSight:
                        if (HasLineOfSight != condition.requiredState)
                        {
                            allConditionsMet = false;
                            break;
                        }
                        break;
                    case ActionConditionBool.BoolsToCheck.IsAttacker:
                        if (selectedAttacker != condition.requiredState)
                        {
                            allConditionsMet = false;
                            break;
                        }
                        break;
                }
                if (!allConditionsMet) break;
            }
        }

        // Float conditions
        if (allConditionsMet && selectedAttackAction.triggerFloatConditions != null && selectedAttackAction.triggerFloatConditions.Length > 0)
        {
            foreach (ActionConditionFloat condition in selectedAttackAction.triggerFloatConditions)
            {
                float valueToCompare = 0f;
                switch (condition.floatToCheck)
                {
                    case ActionConditionFloat.FloatsToCheck.AttackScore:
                        valueToCompare = attackScore;
                        break;
                    case ActionConditionFloat.FloatsToCheck.Distance:
                        valueToCompare = dirToTarg.magnitude;
                        break;
                    case ActionConditionFloat.FloatsToCheck.Fear:
                        valueToCompare = Fear;
                        break;
                }

                switch (condition.comparisionType)
                {
                    case ActionConditionFloat.ComparisonOperators.Equals:
                        if (valueToCompare != condition.comparedTo)
                        {
                            allConditionsMet = false;
                            break;
                        }
                        break;
                    case ActionConditionFloat.ComparisonOperators.GreaterThan:
                        if (valueToCompare < condition.comparedTo)
                        {
                            allConditionsMet = false;
                            break;
                        }
                        break;
                    case ActionConditionFloat.ComparisonOperators.LessThan:
                        if (valueToCompare > condition.comparedTo)
                        {
                            allConditionsMet = false;
                            break;
                        }
                        break;
                }
                if (!allConditionsMet) break;
            }
        }

        if (allConditionsMet)
        {
            PlayVoiceline();
            selectedAttackAction.attackEvent.Invoke();
            lastSelectedAttackAction = selectedAttackAction;
            internalActionCooldown = Time.time + 0.1f;
            selectedAttackAction = null;
            behaviourDuration = 0f;
        }
    }

    void ChooseBehaviour()
    {
        List<AttackAction> totalAttackActions = GetSatisfiedAttackActions().Where(action => (Time.time >= action.cooldownTimeStamp && action.ActionEnergyCost <= actionEnergy)).ToList();

        List<MovementAction> totalMovementActions = new List<MovementAction>();
        if (totalAttackActions.Count == 0)
        {
            totalMovementActions = GetSatisfiedMovementActions().Where(action => Time.time >= action.cooldownTimeStamp).ToList();
        }
        else
        {
            if (lastSelectedAttackAction != null)
            {
                if (lastSelectedAttackAction.followUpActions.Length > 0)
                {
                    List<AttackAction> filteredAttackActions = new List<AttackAction>();
                    foreach (AttackAction action in totalAttackActions) 
                    {
                        foreach (string followUpName in lastSelectedAttackAction.followUpActions)
                        {
                            if(action.AttackActionName == followUpName)
                            {
                                filteredAttackActions.Add(action);
                            }
                        }
                    }

                    if (filteredAttackActions.Count > 0)
                    {
                        totalAttackActions = filteredAttackActions;
                    }
                }
            }
        }

        float totalMovementWeight = totalMovementActions.Sum(a => a.weight);
        float totalAttackWeight = totalAttackActions.Sum(a => a.weight);
        float totalWeight = totalMovementWeight + totalAttackWeight;

        if (totalWeight > 0)
        {
            float randomValue = Random.Range(0, totalWeight);
            float cumulativeWeight = 0f;

            foreach (MovementAction weightedAction in totalMovementActions)
            {
                cumulativeWeight += weightedAction.weight;
                if (randomValue <= cumulativeWeight)
                {
                    PerformMovementAction(weightedAction);
                    return;
                }
            }

            foreach (AttackAction weightedAction in totalAttackActions)
            {
                cumulativeWeight += weightedAction.weight;
                if (randomValue <= cumulativeWeight)
                {
                    PerformAttackAction(weightedAction);
                    return;
                }
            }
        }
    }

    List<MovementAction> GetSatisfiedMovementActions()
    {
        var satisfied = new List<MovementAction>();
        foreach (var action in movementActions)
        {
            bool allConditionsMet = true;

            // Bool conditions
            if (action.selectionBoolConditions != null)
            {
                foreach (var condition in action.selectionBoolConditions)
                {
                    switch (condition.boolToCheck)
                    {
                        case ActionConditionBool.BoolsToCheck.LineOfSight:
                            if (HasLineOfSight != condition.requiredState)
                                allConditionsMet = false;
                            break;
                        case ActionConditionBool.BoolsToCheck.IsAttacker:
                            if (selectedAttacker != condition.requiredState)
                                allConditionsMet = false;
                            break;
                    }
                    if (!allConditionsMet) break;
                }
            }

            // Float conditions
            if (allConditionsMet && action.selectionFloatConditions != null)
            {
                foreach (var condition in action.selectionFloatConditions)
                {
                    float valueToCompare = 0f;
                    switch (condition.floatToCheck)
                    {
                        case ActionConditionFloat.FloatsToCheck.AttackScore:
                            valueToCompare = attackScore;
                            break;
                        case ActionConditionFloat.FloatsToCheck.Distance:
                            valueToCompare = dirToTarg.magnitude;
                            break;
                        case ActionConditionFloat.FloatsToCheck.HealthPercentage:
                            valueToCompare = (float)entity.Health / (float)entity.MaxHealth;
                            break;
                        case ActionConditionFloat.FloatsToCheck.Phase:
                            valueToCompare = NpcPhase;
                            break;
                        case ActionConditionFloat.FloatsToCheck.ActionEnergy:
                            valueToCompare = actionEnergy;
                            break;
                        case ActionConditionFloat.FloatsToCheck.Fear:
                            valueToCompare = Fear;
                            break;
                    }

                    switch (condition.comparisionType)
                    {
                        case ActionConditionFloat.ComparisonOperators.Equals:
                            if (valueToCompare != condition.comparedTo)
                                allConditionsMet = false;
                            break;
                        case ActionConditionFloat.ComparisonOperators.GreaterThan:
                            if (valueToCompare < condition.comparedTo)
                                allConditionsMet = false;
                            break;
                        case ActionConditionFloat.ComparisonOperators.LessThan:
                            if (valueToCompare > condition.comparedTo)
                                allConditionsMet = false;
                            break;
                    }
                    if (!allConditionsMet) break;
                }
            }

            if (allConditionsMet)
                satisfied.Add(action);
        }
        return satisfied;
    }

    List<AttackAction> GetSatisfiedAttackActions()
    {
        var satisfied = new List<AttackAction>();
        foreach (var action in attackActions)
        {
            bool allConditionsMet = true;

            // Bool conditions
            if (action.selectionBoolConditions != null)
            {
                foreach (var condition in action.selectionBoolConditions)
                {
                    switch (condition.boolToCheck)
                    {
                        case ActionConditionBool.BoolsToCheck.LineOfSight:
                            if (HasLineOfSight != condition.requiredState)
                                allConditionsMet = false;
                            break;
                        case ActionConditionBool.BoolsToCheck.IsAttacker:
                            if (selectedAttacker != condition.requiredState)
                                allConditionsMet = false;
                            break;
                    }
                    if (!allConditionsMet) break;
                }
            }

            // Float conditions
            if (allConditionsMet && action.selectionFloatConditions != null)
            {
                foreach (var condition in action.selectionFloatConditions)
                {
                    float valueToCompare = 0f;
                    switch (condition.floatToCheck)
                    {
                        case ActionConditionFloat.FloatsToCheck.AttackScore:
                            valueToCompare = attackScore;
                            break;
                        case ActionConditionFloat.FloatsToCheck.Distance:
                            valueToCompare = dirToTarg.magnitude;
                            break;
                        case ActionConditionFloat.FloatsToCheck.ActionEnergy:
                            valueToCompare = actionEnergy;
                            break;
                        case ActionConditionFloat.FloatsToCheck.HealthPercentage:
                            valueToCompare = (float)entity.Health / (float)entity.MaxHealth;
                            break;
                        case ActionConditionFloat.FloatsToCheck.Phase:
                            valueToCompare = NpcPhase;
                            break;
                        case ActionConditionFloat.FloatsToCheck.Fear:
                            valueToCompare = Fear;
                            break;
                    }

                    switch (condition.comparisionType)
                    {
                        case ActionConditionFloat.ComparisonOperators.Equals:
                            if (valueToCompare != condition.comparedTo)
                                allConditionsMet = false;
                            break;
                        case ActionConditionFloat.ComparisonOperators.GreaterThan:
                            if (valueToCompare < condition.comparedTo)
                                allConditionsMet = false;
                            break;
                        case ActionConditionFloat.ComparisonOperators.LessThan:
                            if (valueToCompare > condition.comparedTo)
                                allConditionsMet = false;
                            break;
                    }
                    if (!allConditionsMet) break;
                }
            }

            // Shared cooldown check
            if (allConditionsMet && !string.IsNullOrEmpty(action.sharedCooldownGroup))
            {
                if (sharedCooldowns.TryGetValue(action.sharedCooldownGroup, out float groupCooldown))
                {
                    if (Time.time < groupCooldown)
                    {
                        allConditionsMet = false;
                    }
                }
            }
            else if (allConditionsMet)
            {
                if (Time.time < action.cooldownTimeStamp)
                {
                    allConditionsMet = false;
                }
            }

            if (allConditionsMet)
                satisfied.Add(action);
        }
        return satisfied;
    }

    float GetRandomStrafeDir()
    {
        return Random.Range(0f, 100f) <= 50f ? -1f : 1f;
    }

    void PerformMovementAction(MovementAction movementAction)
    {
        behaviourDuration = movementAction.Duration;
        randStrafeDir = GetRandomStrafeDir();
        movementAction.cooldownTimeStamp = Time.time + movementAction.Cooldown;
        movementState = movementAction.actionState;
    }

    void PerformAttackAction(AttackAction attackAction)
    {
        if (entity.Staggered)
        {
            movementState = MovementStates.Stay;
            return;
        }

        actionEnergy -= attackAction.ActionEnergyCost;
        behaviourDuration = attackAction.Duration;
        randStrafeDir = GetRandomStrafeDir();
        attackAction.cooldownTimeStamp = Time.time + attackAction.Cooldown;
        movementState = attackAction.actionState;
        selectedAttackAction = attackAction;

        // Set shared cooldown if applicable
        if (!string.IsNullOrEmpty(attackAction.sharedCooldownGroup))
        {
            sharedCooldowns[attackAction.sharedCooldownGroup] = Time.time + attackAction.Cooldown;
        }
    }

    void FindTargets()
    {
        visibleTargets.Clear();
        if (entity.Body == null) return;

        // 1. Add all targets in smell radius (no angle or LOS check)
        Collider[] targetsInSmellRadius = Physics.OverlapSphere(entity.Body.position, smellRadius, characterMask);
        foreach (Collider col in targetsInSmellRadius)
        {
            if (!visibleTargets.Contains(col.transform))
            {
                visibleTargets.Add(col.transform);
            }
        }

        // 2. Add targets in view radius that are in FOV and have LOS, but not already in visibleTargets
        Collider[] targetsInViewRadius = Physics.OverlapSphere(entity.Body.position, viewRadius, characterMask);
        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            if (visibleTargets.Contains(target)) continue; // Already added via smell

            Vector3 dirToTarget = (target.position - entity.Body.position).normalized;
            if (Vector3.Angle(targeting.objectToRotate.forward, dirToTarget) < viewAngle / 2f)
            {
                float dstToTarget = Vector3.Distance(entity.Body.position, target.position);

                // Use 3D raycast but check if target is at reasonable height difference
                Vector3 heightDifference = target.position - entity.Body.position;
                if (Mathf.Abs(heightDifference.y) < 5f) // Allow some height difference for stairs/ramps
                {
                    if (!Physics.Raycast(entity.Body.position, dirToTarget, dstToTarget, ObstacleMask))
                    {
                        visibleTargets.Add(target);
                    }
                }
            }
        }
    }

    void SetTarget()
    {
        float closestTarg = MaxTargetDistance;
        CharacterCombatHandler targCombatHandler = null;
        foreach (Transform target in visibleTargets)
        {
            CharacterCombatHandler targCombat = target.GetComponent<CharacterCombatHandler>();
            if (targCombat != null)
            {
                // Use horizontal distance for target selection
                Vector3 horizontalDistance = target.position - transform.position;
                horizontalDistance.y = 0;
                float dist = horizontalDistance.magnitude;
                
                if (targCombat.entity.Team != entity.Team && dist < closestTarg && targCombat.entity.EntityType == Entity.EntityTypes.Character)
                {
                    closestTarg = dist;
                    targCombatHandler = targCombat;
                }
            }
        }

        if (targCombatHandler != null)
        {
            Target = targCombatHandler;
            if (!Alerted)
            {
                Alerted = true;
            }
        }
    }

    void CalculateLastScenePos()
    {
        if (entity.Body == null) return;
        if (!Physics.Raycast(entity.Body.position, dirToTarg.normalized, dirToTarg.magnitude, ObstacleMask))
        {
            if (Target != null) lastScenePos = Target.entity.Body.position;
            HasLineOfSight = true;
        }
        else
        {
            HasLineOfSight = false;
        }
    }

    void HandleStates()
    {
        if (Target == null)
        {
            movementState = MovementStates.Patrol;
        }

        switch (movementState)
        {
            case MovementStates.Patrol:
                Patrol();
                break;

            case MovementStates.Stay:
                Stay();
                break;

            case MovementStates.Strafe:
                Strafe();
                break;

            case MovementStates.MoveBackward:
                MoveBackward();
                break;

            case MovementStates.MoveForward:
                MoveForward();
                break;
        }
    }

    void Patrol()
    {
        movement.movement.externalMoveSpeedMultiplier = patrolMoveSpeed;

        // Use horizontal distance for patrol calculations
        Vector3 horizontalSpawnDistance = spawnPos - transform.position;
        horizontalSpawnDistance.y = 0;
        float spawnDistPercent = horizontalSpawnDistance.magnitude / patrolRadius;
        Vector3 dirToSpawn = horizontalSpawnDistance.normalized;

        float noisyX = (Mathf.PerlinNoise(Time.time * (1 + spawnDistPercent) / 16f + randIdleSeed, Time.time * (1 + spawnDistPercent) / 4f - randIdleSeed) * 2f) - 1f;
        float noisyZ = (Mathf.PerlinNoise(Time.time * (1 + spawnDistPercent) / 4f - randIdleSeed, Time.time * (1 + spawnDistPercent) / 16f + randIdleSeed) * 2) - 1f;
        
        // Keep patrol movement on ground level, let CharacterMovement handle Y positioning
        Vector3 patrolTarget = transform.position + new Vector3(noisyX, 0, noisyZ) * PatrolNoiseDistance * (1 - spawnDistPercent) + dirToSpawn * spawnDistPercent * PatrolNoiseDistance;
        patrolTarget.y = transform.position.y; // Maintain current Y level
        movement.moveTo = patrolTarget;

        targeting.pointToLook = movement.moveTo;
    }

    void Stay()
    {
        movement.movement.externalMoveSpeedMultiplier = 0f;
        targeting.pointToLook = lastScenePos;

        movement.moveTo = entity.Body.position;
    }

    void Strafe()
    {
        movement.movement.externalMoveSpeedMultiplier = horizontalMoveSpeed;
        targeting.pointToLook = lastScenePos;

        // Calculate strafe direction in horizontal plane
        Vector3 horizontalDirToTarg = dirToTarg;
        horizontalDirToTarg.y = 0;
        horizontalDirToTarg = horizontalDirToTarg.normalized;
        
        Vector3 strafeTarget = entity.Body.position + (Quaternion.AngleAxis(90 * randStrafeDir, Vector3.up) * horizontalDirToTarg) * StrafeDistance;
        strafeTarget.y = entity.Body.position.y; // Maintain current Y level
        movement.moveTo = strafeTarget;
    }

    void MoveBackward()
    {
        movement.movement.externalMoveSpeedMultiplier = backwardMoveSpeed;
        targeting.pointToLook = lastScenePos;

        // Move backward in horizontal plane
        Vector3 horizontalDirToTarg = dirToTarg;
        horizontalDirToTarg.y = 0;
        horizontalDirToTarg = horizontalDirToTarg.normalized;
        
        Vector3 backwardTarget = entity.Body.position - (horizontalDirToTarg * StrafeDistance);
        backwardTarget.y = entity.Body.position.y; // Maintain current Y level
        movement.moveTo = backwardTarget;
    }

    void MoveForward()
    {
        movement.movement.externalMoveSpeedMultiplier = forwardMoveSpeed;
        targeting.pointToLook = lastScenePos;

        movement.moveTo = lastScenePos;
    }

    public void SetHeat(int heatValue)
    {
        heat = heatValue;
    }

    public void SetPhase(int phaseValue)
    {
        NpcPhase = phaseValue;
    }

    void PlayVoiceline()
    {
        if(voiceLines.Length > 0)
        {
            if (Time.time >= voicelineCooldownTimeStamp)
            {
                SoundFXManager.Instance.PlaySoundClip(voiceLines[Random.Range(0, voiceLines.Length)], entity.Body.position, voiceVolume, Random.Range(0.95f, 1.05f));
                voicelineCooldownTimeStamp = Time.time + Random.Range(minVoicelineCooldown, maxVoicelineCooldown);
            }
        }
    }

    void RetargetOnDamage()
    {
        FindTargets();
        SetTarget();
    }

    private void OnDrawGizmos()
    {
        if (entity != null && entity.Body != null)
        {
            // Draw direction to target
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(entity.Body.position, dirToTarg);

            // Draw line of sight ray (green if clear, red if blocked)
            if (Target != null)
            {
                Vector3 toTarget = Target.entity.Body.position - entity.Body.position;
                bool hasLOS = !Physics.Raycast(entity.Body.position, toTarget.normalized, toTarget.magnitude, ObstacleMask);
                Gizmos.color = hasLOS ? Color.green : Color.red;
                Gizmos.DrawLine(entity.Body.position, Target.entity.Body.position);
            }
        }
    }
}