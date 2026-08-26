using UnityEngine;

public class CharacterLegIK : MonoBehaviour
{
    public Transform legTargetObject;
    public Transform legTargetRaycastPoint;
    public Vector3 currentLegPoint;
    public Vector3 targetPoint;
    public float baseTargDistanceThreshold = 0.5f;
    public float speedThresholdMultiplier = 0.2f;
    float targDist;

    public LayerMask groundMask;

    public float legMoveSpeed = 8f;
    public float stepHeight = 0.2f;
    public float legMoveDistance = 0.2f;
    public float maxStepDistance = 1.0f;
    float lerp;

    RaycastHit hit;

    bool legGrounded = true;
    public CharacterLegIK OppositeLeg;

    Vector3 oldPos;
    Vector3 newPos;

    // For predictive stepping
    public Rigidbody characterRigidbody;
    public float predictionTime = 0.15f;

    // Step cooldown
    public float stepCooldown = 0.1f;
    float lastStepTime;

    private void Start()
    {
        oldPos = legTargetObject.position;
        newPos = legTargetObject.position;
        lerp = 1f;
    }

    void Update()
    {
        legTargetObject.position = currentLegPoint;

        // Predictive target point based on velocity
        Vector3 velocity = characterRigidbody ? characterRigidbody.linearVelocity : Vector3.zero;
        Vector3 predictedTarget = targetPoint + velocity * predictionTime;

        // Dynamic threshold based on speed
        float dynamicThreshold = baseTargDistanceThreshold + velocity.magnitude * speedThresholdMultiplier;

        float dist = Vector3.Distance(predictedTarget, newPos);

        if (OppositeLeg == null || OppositeLeg.legGrounded)
        {
            if (Physics.Raycast(legTargetRaycastPoint.position, Vector3.down, out hit, 10f, groundMask))
            {
                targetPoint = hit.point;
            }

            // Only step if cooldown passed and distance threshold exceeded
            if (Time.time - lastStepTime > stepCooldown && dist >= dynamicThreshold)
            {
                Vector3 moveDir = (predictedTarget - oldPos).normalized;
                float stepDist = Mathf.Clamp(Vector3.Distance(predictedTarget, oldPos), 0, maxStepDistance);
                newPos = predictedTarget + moveDir * legMoveDistance;
                newPos = oldPos + (newPos - oldPos).normalized * stepDist;
                lerp = 0f;
                legGrounded = false;
                lastStepTime = Time.time;
            }
        }

        if (lerp < 1f)
        {
            Vector3 footPos = Vector3.Lerp(oldPos, newPos, lerp);
            footPos.y += Mathf.Sin(lerp * Mathf.PI) * stepHeight;
            currentLegPoint = footPos;
            lerp += Time.deltaTime * legMoveSpeed * (1f + dist);
            legGrounded = false;
        }
        else
        {
            oldPos = newPos;
            legGrounded = true;
            lerp = 1f;
        }
    }
}