using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    public AudioClip pickUpSound;
    public Item item;
    public ItemInstanceData itemData;

    public void OnPickUp(Entity entity)
    {
        if(entity.transform.tag == "Player")
        {
            PlayerInventory playerInventory = entity.GetComponent<PlayerInventory>();
            if (playerInventory == null)
            {
                // Try to find PlayerInventory in the scene if not on the entity
                playerInventory = FindFirstObjectByType<PlayerInventory>();
            }

            if (playerInventory != null)
            {
                // Create ItemInstance from the item and itemData
                ItemInstance itemInstance = new ItemInstance();
                itemInstance.item = item;
                itemInstance.itemData = itemData;

                bool itemAdded = false;

                // Try to add to inventory slots first
                foreach (PlayerInventory.ItemSlot slot in playerInventory.InventoryItems)
                {
                    if (!itemAdded && (slot.itemInstance == null || slot.itemInstance.item == null))
                    {
                        itemAdded = true;
                        GameManager.Instance.AddItemToSlot(itemInstance, slot, playerInventory);
                        break;
                    }
                }

                if (itemAdded)
                {
                    // Immediately disable the InteractableObject component to hide interaction prompt
                    InteractableObject interactable = GetComponent<InteractableObject>();
                    if (interactable != null)
                    {
                        interactable.enabled = false;
                    }
                    
                    SoundFXManager.Instance.PlaySoundClip(pickUpSound, entity.transform.position, 0.75f, Random.Range(0.95f, 1.05f));
                    Destroy(gameObject);
                }
            }
        }
    }
}
