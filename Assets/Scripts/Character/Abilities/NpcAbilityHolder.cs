using UnityEngine;
using UnityEngine.Events;

public class NpcAbilityHolder : MonoBehaviour
{
    public Entity Owner;

    [System.Serializable]
    public class CallableAbility
    {
        public string AbilityCallID;
        public UnityEvent<Vector3> OnAbilityCast;
    }
    public CallableAbility[] Abilities;

    public void CallAbilityOnOwner(string id)
    {
        foreach (CallableAbility item in Abilities)
        {
            if (item.AbilityCallID == id)
            {
                item.OnAbilityCast.Invoke(Owner.Body.position);
            }
        }
    }

    public void CallAbility(string id, Vector3 pos)
    {
        foreach (CallableAbility item in Abilities)
        {
            if (item.AbilityCallID == id)
            {
                item.OnAbilityCast.Invoke(pos);
            }
        }
    }
}
