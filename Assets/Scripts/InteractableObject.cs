using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour
{
    public UnityEvent<Entity> InteractionEvent;
    [Header("Interaction")]
    public string interactionPrompt = "Press [E] to interact";
    public float interactionRadius = 2f;
    public bool disableOnInteract = false;
    [HideInInspector]
    public bool hasBeenInteracted = false;

    public void OnInteract(Entity entity)
    {
        // Check if this object can be interacted with
        if (hasBeenInteracted && disableOnInteract) return;
        
        // Check if this is a locked chest
        Chest chest = GetComponent<Chest>();
        if (chest != null && chest.Locked) return;
        
        Debug.Log("interacted with " + transform.name);
        InteractionEvent.Invoke(entity);
        
        // Mark as interacted if disable on interact is enabled
        if (disableOnInteract)
        {
            hasBeenInteracted = true;
        }
    }
    
    public bool CanInteract()
    {
        // Check if already interacted and disabled
        if (hasBeenInteracted && disableOnInteract) return false;
        
        // Check if this is a locked chest
        Chest chest = GetComponent<Chest>();
        if (chest != null && chest.Locked) return false;
        
        return true;
    }
}
