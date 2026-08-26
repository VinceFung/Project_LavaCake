using UnityEngine;

/// <summary>
/// Example of how to extend BossArena with custom behavior using function overrides
/// </summary>
public class CustomBossArena : BossArena
{
    [Header("Custom Arena Effects")]
    public ParticleSystem arenaActivationEffect;
    public ParticleSystem arenaClearedEffect;
    public AudioSource arenaAudioSource;
    public AudioClip arenaStartSound;
    public AudioClip arenaClearedSound;
    
    public override void OnArenaActivated()
    {
        // Call the base functionality first
        base.OnArenaActivated();
        
        // Add custom effects
        if (arenaActivationEffect != null)
            arenaActivationEffect.Play();
            
        if (arenaAudioSource != null && arenaStartSound != null)
            arenaAudioSource.PlayOneShot(arenaStartSound);
            
        Debug.Log($"Custom arena {gameObject.name} activated with special effects!");
    }
    
    public override void OnArenaCleared()
    {
        // Call the base functionality first
        base.OnArenaCleared();
        
        // Add custom effects
        if (arenaClearedEffect != null)
            arenaClearedEffect.Play();
            
        if (arenaAudioSource != null && arenaClearedSound != null)
            arenaAudioSource.PlayOneShot(arenaClearedSound);
            
        Debug.Log($"Custom arena {gameObject.name} cleared with celebration effects!");
    }
    
    public override void OnArenaExited()
    {
        // Call the base functionality first
        base.OnArenaExited();
        
        // Add custom reset behavior
        Debug.Log($"Custom arena {gameObject.name} exited - custom cleanup performed!");
    }
}
