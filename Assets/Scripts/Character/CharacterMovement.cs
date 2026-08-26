using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;

public class CharacterMovement : MonoBehaviour
{
    public Entity characterEntity;
    public Rigidbody rb;
    public Transform groundCheckPos;
    public float groundCheckRadius;
    public LayerMask groundMask;
    public float gravity;
    float currentGravity;
    public bool IsGrounded;
    
    [Header("Step Detection")]
    public float stepHeight = 0.3f;
    public float stepCheckDistance = 0.6f;
    public float stepUpForce = 8f;
    
    // Step state tracking for frame-rate independence
    private bool isSteppingUp = false;
    private float stepCooldownTimer = 0f;
    private const float STEP_COOLDOWN = 0.1f;
    
    public float moveSpeed;
    public float penalizedMovementSpeed;
    public float drag;
    public float dashStaminaCost;
    public float DashSpeed;
    public float DashDuration;
    public float ImmunityDuration;
    public bool ImmuneDuringDash = false;
    [HideInInspector]
    public float dashDur;
    public ParticleSystem dashParticles;
    Vector3 dashDir;

    public Vector3 moveInput;

    public bool movementPenalized = false;

    float interpolatedMoveSpeed;

    public CharacterTargeting targeting;
    public float lungeDrag = 10f;
    public float LungeSpeed;

    //[HideInInspector]
    public float externalMoveSpeedMultiplier = 1f;

    public List<Vector3> appliedForces = new List<Vector3>();

    public UnityEvent OnDash;

    void Update()
    {
        IsGrounded = Physics.CheckSphere(groundCheckPos.position, groundCheckRadius, groundMask);
        if (IsGrounded)
        {
            currentGravity = 0f;
            if (gravity != 0)
            {
                //currentGravity = -0.5f;
            }
            else
            {

            }
        }
        else
        {
            currentGravity += Time.deltaTime * gravity;
        }

        moveInput.Normalize();

        Vector3 appliedForceDir = Vector3.zero;

        if (appliedForces.Count > 0)
        {
            List<Vector3> removeForces = new List<Vector3>();
            for (int i = 0; i < appliedForces.Count; i++)
            {
                appliedForceDir += appliedForces[i];

                appliedForces[i] -= appliedForces[i].normalized * drag * Time.deltaTime;
                if (appliedForces[i].magnitude <= 0.2f)
                {
                    appliedForces[i] = Vector3.zero;
                    removeForces.Add(appliedForces[i]);
                }
            }

            foreach (Vector3 f in removeForces)
            {
                appliedForces.Remove(f);
            }
        }

        if ((!movementPenalized || appliedForceDir.magnitude > 0) && !characterEntity.Staggered)
        {
            interpolatedMoveSpeed = Mathf.Lerp(interpolatedMoveSpeed, moveSpeed, Time.deltaTime * (moveSpeed-penalizedMovementSpeed));
        }
        else
        {
            interpolatedMoveSpeed = Mathf.Lerp(interpolatedMoveSpeed, penalizedMovementSpeed, Time.deltaTime * (moveSpeed - penalizedMovementSpeed));
        }

        if (dashDur <= 0)
        {
            Vector3 finalVelocity = moveInput * interpolatedMoveSpeed * externalMoveSpeedMultiplier * characterEntity.MovementSpeedMultiplier + targeting.objectToRotate.forward * LungeSpeed + appliedForceDir + Vector3.up * currentGravity;
            
            rb.linearVelocity = finalVelocity;
            
            // Handle step-up separately with impulse (frame-rate independent)
            if (stepCooldownTimer > 0f)
            {
                stepCooldownTimer -= Time.deltaTime;
            }
            
            if (IsGrounded && moveInput.magnitude > 0.1f && stepCooldownTimer <= 0f)
            {
                if (CheckForStepUp())
                {
                    // Apply impulse for step-up
                    rb.AddForce(Vector3.up * stepUpForce, ForceMode.Impulse);
                    stepCooldownTimer = STEP_COOLDOWN;
                    isSteppingUp = true;
                }
            }
            
            // Reset stepping flag when we're clearly on solid ground
            if (IsGrounded && rb.linearVelocity.y <= 0.1f)
            {
                isSteppingUp = false;
            }
        }
        else
        {
            dashDur -= Time.deltaTime;
            rb.linearVelocity = dashDir * DashSpeed * (dashDur / DashDuration);
        }

        if (ImmuneDuringDash && dashDur >= (DashDuration - ImmunityDuration))
        {
            characterEntity.DashImmunity = true;
        }
        else
        {
            characterEntity.DashImmunity = false;
        }


        if (LungeSpeed > 0.1f)
        {
            LungeSpeed -= Time.deltaTime * lungeDrag;
        }
        else
        {
            LungeSpeed = 0f;
        }
    }

    public void Dash()
    {
        if (moveInput.magnitude != 0 && dashDur <= 0 && !movementPenalized && characterEntity.Stamina > 0)
        {
            OnDash.Invoke();
            characterEntity.UseStamina(dashStaminaCost);
            dashParticles.transform.rotation = Quaternion.LookRotation(-moveInput);
            dashParticles.Play();
            dashDur = DashDuration;
            dashDir = moveInput.normalized;
        }
    }

    public void setLungeSpeed(float speed)
    {
        LungeSpeed = speed;
    }

    bool CheckForStepUp()
    {
        // Check if there's an obstacle in front that we might step up
        Vector3 moveDirection = moveInput.normalized;
        if (moveDirection.magnitude < 0.1f) return false;
        
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        Vector3 rayDirection = moveDirection;
        
        // Check for obstacle at character level
        if (Physics.Raycast(rayStart, rayDirection, out RaycastHit hit, stepCheckDistance, groundMask))
        {
            // Check if there's ground above the obstacle (step surface)
            Vector3 stepCheckStart = hit.point + Vector3.up * stepHeight;
            
            // Cast downward from above the step to see if there's a valid step surface
            if (Physics.Raycast(stepCheckStart, Vector3.down, out RaycastHit stepHit, stepHeight + 0.1f, groundMask))
            {
                float stepUpHeight = stepHit.point.y - transform.position.y;
                
                // If it's a reasonable step height and we're not already stepping up
                if (stepUpHeight > 0.05f && stepUpHeight <= stepHeight && !isSteppingUp)
                {
                    return true;
                }
            }
        }
        
        return false;
    }
}
