using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTextMotion : MonoBehaviour
{
    public float speed;
    public float speedAccel;
    public float lifeTime;

    void Update()
    {
        transform.Translate(0, speed * Time.deltaTime, 0);
        if (speed > -1)
        {
            speed += speedAccel * Time.deltaTime;
        }
        else
        {
            speed = -1f;
        }


        if (lifeTime <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            lifeTime -= Time.deltaTime;
        }
    }
}