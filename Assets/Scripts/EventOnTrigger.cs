using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventOnTrigger : MonoBehaviour
{
    public UnityEvent OnTriggered;

    private void Start()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if(UnitManager.Instance.playerObj != null)
        {
            if (other.transform == UnitManager.Instance.playerObj.transform)
            {
                OnTriggered?.Invoke();
            }
        }
    }
}
