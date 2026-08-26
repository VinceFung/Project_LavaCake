using UnityEngine;

public class FixedRotationObject : MonoBehaviour
{
    public Vector3 fixedRotation;
    
    void Update()
    {
        transform.rotation = Quaternion.Euler(fixedRotation);
    }
}
