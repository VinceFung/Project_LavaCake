using UnityEngine;

public class CharacterTargeting : MonoBehaviour
{
    public Transform objectToRotate;
    public float turnSpeed;
    public float penalizedTurnSpeed;

    public bool turnPenalized = false;

    float currentTurnSpeed;

    public Vector3 pointToLook;

    void Update()
    {
        Vector3 lookDir = new Vector3(pointToLook.x, transform.position.y, pointToLook.z);

        Quaternion targetRotation = Quaternion.LookRotation(lookDir - objectToRotate.position);

        if (!turnPenalized)
        {
            currentTurnSpeed = Mathf.Lerp(currentTurnSpeed, turnSpeed, Time.deltaTime * (turnSpeed - penalizedTurnSpeed));
        }
        else
        {
            currentTurnSpeed = Mathf.Lerp(currentTurnSpeed, penalizedTurnSpeed, Time.deltaTime * (turnSpeed - penalizedTurnSpeed));
        }

        objectToRotate.rotation = Quaternion.Lerp(objectToRotate.rotation, targetRotation, currentTurnSpeed * Time.deltaTime);
    }
}