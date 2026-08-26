using UnityEngine;
using UnityEngine.Events;

public class TeleportAbility : MonoBehaviour
{
    public Ability ability;

    public float teleportDistance = 6f;

    public LayerMask teleportMask;

    public UnityEvent OnTeleportEnd;

    public void Teleport()
    {
        Vector3 teleportDir = ability.Owner.Body.forward.normalized;

        Physics.Raycast(ability.Owner.Body.position, teleportDir, out RaycastHit hit, teleportDistance, teleportMask);
        Vector3 teleportPos = hit.point;
        if (hit.collider == null)
        {
            teleportPos = ability.Owner.Body.position + teleportDir * teleportDistance;
        }
        ability.Owner.transform.position = teleportPos;
        
        OnTeleportEnd.Invoke();

        ability.Owner.OnAbilityCast.Invoke();
    }
}
