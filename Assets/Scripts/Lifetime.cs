using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Lifetime : MonoBehaviour
{
    public float lifetime;

    public UnityEvent onDeath;

    void Update()
    {
        if (lifetime > 0)
        {
            lifetime -= Time.deltaTime;
        }
        else
        {
            Die();
        }
    }

    public void Die()
    {
        if (onDeath != null) onDeath.Invoke();
        Destroy(gameObject);
    }
}