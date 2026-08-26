using UnityEngine;

public class BossForgiveness : MonoBehaviour
{
    public Entity entity;
    public float healthThreshold = 0.9f;
    public float forgivenessDuration = 5f;
    float forgivenessDur;
    
    private BossArena parentArena = null; // Reference to the parent arena managing this boss

    void Start()
    {
        forgivenessDur = forgivenessDuration;
    }

    /// <summary>
    /// Set the parent arena that manages this boss (called by BossArena)
    /// </summary>
    /// <param name="arena">The arena managing this boss</param>
    public void SetBossArena(BossArena arena)
    {
        parentArena = arena;
    }

    /// <summary>
    /// Check if this boss is managed by a BossArena
    /// </summary>
    /// <returns>True if managed by BossArena</returns>
    public bool HasParentArena()
    {
        return parentArena != null;
    }

    private void Update()
    {
        // Forgiveness logic - reduces damage when boss health is low
        if (entity.Health < entity.MaxHealth * healthThreshold)
        {
            forgivenessDur -= Time.deltaTime;
        }

        if (forgivenessDur > 0)
        {
            entity.baseDamageMultiplier = 0.5f;
        }
        else
        {
            entity.baseDamageMultiplier = 1f;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (entity == null) return;
        
        // Draw boss position
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(entity.transform.position, 2f);
        
        // Show if this boss is managed by a BossArena
        if (parentArena != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(entity.transform.position, parentArena.transform.position);
            
            // Draw connection indicator
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(entity.transform.position + Vector3.up * 3f, Vector3.one * 0.5f);
        }
    }
}
