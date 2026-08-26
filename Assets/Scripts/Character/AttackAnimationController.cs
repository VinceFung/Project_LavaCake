using UnityEngine;

public class AttackAnimationController : MonoBehaviour
{
    public Entity entity;
    public MeleeWeapon selectedWeapon;
    public Transform weaponHolder;
    public Transform offhandWeaponHolder;
    public Transform animatedImpalementPos;
    public CharacterMovement movement;
    public CharacterTargeting targeting;
    public Animator anim;
    public bool meleeHitboxActive;
    public bool OffhandMeleeHitboxActive;
    public bool IsAttacking;
    public bool IsArmed;

    [Header("Effects")]
    public bool enableWeaponTrails;

    public bool uniqueAnim = false;

    [Header("Animation Smoothing")]
    public float animationSmoothTime = 0.1f;
    
    private float currentMoveX;
    private float currentMoveZ;
    private float moveXVelocity;
    private float moveZVelocity;

    void Update()
    {
        selectedWeapon.IsAttacking = IsAttacking;
        anim.SetBool("IsAttacking", IsAttacking);
        anim.SetBool("IsStaggered", entity.Staggered || entity.Impaled);
        if (selectedWeapon.Trails != null) selectedWeapon.Trails.SetActive(enableWeaponTrails);
        
        UpdateMovementAnimationParameters();
    }

    private void UpdateMovementAnimationParameters()
    {
        if (movement == null || targeting == null)
        {
            anim.SetFloat("MoveX", 0f);
            anim.SetFloat("MoveZ", 0f);
            return;
        }

        Vector3 moveInput = movement.moveInput;
        Vector3 targetForward = targeting.objectToRotate.forward;
        Vector3 targetRight = targeting.objectToRotate.right;

        float targetMoveX = Vector3.Dot(moveInput, targetRight);
        float targetMoveZ = Vector3.Dot(moveInput, targetForward);

        currentMoveX = Mathf.SmoothDamp(currentMoveX, targetMoveX, ref moveXVelocity, animationSmoothTime);
        currentMoveZ = Mathf.SmoothDamp(currentMoveZ, targetMoveZ, ref moveZVelocity, animationSmoothTime);

        anim.SetFloat("MoveX", currentMoveX);
        anim.SetFloat("MoveZ", currentMoveZ);
    }

    public void CrossFadeAnimation(string animationName)
    {
        anim.CrossFade(animationName, 0.1f);
    }

    public void playWeaponParticle(string name)
    {
        if (selectedWeapon.particlePlayer != null) selectedWeapon.particlePlayer.PlayParticle(name);
    }

    public void SetAttackState(int attackState)
    {
        if (attackState == 1)
        {
            IsAttacking = true;
        }
        else
        {
            IsAttacking = false;
        }
    }

    public void SetTurnSpeedPenalty(int penaltyState)
    {
        if (penaltyState == 1)
        {
            targeting.turnPenalized = true;
        }
        else
        {
            targeting.turnPenalized = false;
        }
    }

    public void SetMovementPenalty(int penaltyState)
    {
        if (penaltyState == 1)
        {
            movement.movementPenalized = true;
        }
        else
        {
            movement.movementPenalized = false;
        }
    }

    public void CallHeavyDamage()
    {
        selectedWeapon.DealHeavyDamage();
    }

    public void CallImpalementEvent(int impalementIndex)
    {
        selectedWeapon.impalementEvents[impalementIndex].Invoke();
    }

    public void CallDropCorpses()
    {
        selectedWeapon.DropCorpses();
    }

    public void PlayLightAttackSound()
    {
        SoundFXManager.Instance.PlaySoundClip(selectedWeapon.lightAttackSound, transform.position, selectedWeapon.lightAttackSoundVolume, Random.Range(0.9f, 1.1f));
    }
}
