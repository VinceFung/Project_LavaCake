using System.Collections.Generic;
using UnityEngine;

public class NpcMovement : MonoBehaviour
{
    public Entity entity;
    //public Transform Target;
    public Vector3 moveTo;
    public CharacterMovement movement;

    [Header("CONTEXT BASED STEERING")]
    public float colliderRadius;
    public float obstacleDetectionRadius;
    float[] danger = new float[8];
    float[] interest = new float[8];
    public LayerMask avoidLayers;

    Vector3 dirToTarg;

    Vector3 moveDir;

    private void Start()
    {
        moveTo = entity.Body.position;
    }

    void Update()
    {
        dirToTarg = (moveTo - entity.Body.position);
        // Keep only horizontal direction for movement calculations
        dirToTarg.y = 0;
        dirToTarg = dirToTarg.normalized;

        calculateInterest();
        calculateDanger();

        moveDir = Vector3.zero;
        for (int i = 0; i < movementDirections.Count; i++)
        {
            float resultantInterest = Mathf.Clamp01(interest[i] - danger[i]);
            moveDir += movementDirections[i] * resultantInterest;
        }

        movement.moveInput = moveDir.normalized;
    }

    void calculateInterest()
    {
        for (int i = 0; i < movementDirections.Count; i++)
        {
            interest[i] = 0f;
            float result = Vector3.Dot(dirToTarg, movementDirections[i]);

            if (result > 0)
            {
                interest[i] = result;
            }
        }
    }

    void calculateDanger()
    {
        for (int i = 0; i < movementDirections.Count; i++)
        {
            danger[i] = 0;
        }

        Collider[] colliders = Physics.OverlapSphere(entity.Body.position, obstacleDetectionRadius, avoidLayers);
        foreach (Collider col in colliders)
        {
            bool detectingSelf = false;
            if (col.gameObject == this.gameObject)
            {
                detectingSelf = true;
            }

            foreach (Collider c in transform.GetComponentsInChildren<Collider>())
            {
                if (col.gameObject == c.gameObject)
                {
                    detectingSelf = true;
                }
            }

            Entity colEntity = col.gameObject.GetComponent<Entity>();
            if (colEntity != null)
            {
                if (colEntity.Team != entity.Team)
                {
                    detectingSelf = true;
                }
            }

            if (detectingSelf == false)
            {
                Vector3 dirToObstacle = col.ClosestPoint(entity.Body.position) - entity.Body.position;
                // Only consider horizontal direction for obstacle avoidance
                dirToObstacle.y = 0;
                float distToObstacle = dirToObstacle.magnitude;

                float dangerWeight = 0f;
                if (distToObstacle <= colliderRadius)
                {
                    dangerWeight = 1f;
                }
                else
                {
                    //might want to change obstacleDetectionRadius here to a static value or something else
                    dangerWeight = (obstacleDetectionRadius - distToObstacle) / obstacleDetectionRadius;
                }

                for (int i = 0; i < movementDirections.Count; i++)
                {
                    float result = Vector3.Dot(dirToObstacle.normalized, movementDirections[i]);
                    danger[i] = result * dangerWeight;
                }
            }
        }
    }

    List<Vector3> movementDirections = new List<Vector3>
    {
        new Vector3(0,0,1).normalized,
        new Vector3(1,0,1).normalized,
        new Vector3(1,0,0).normalized,
        new Vector3(1,0,-1).normalized,
        new Vector3(0,0,-1).normalized,
        new Vector3(-1,0,-1).normalized,
        new Vector3(-1,0,0).normalized,
        new Vector3(-1,0,1).normalized,
    };

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(entity.Body.position, colliderRadius);
        Gizmos.DrawWireSphere(entity.Body.position, obstacleDetectionRadius);

        for (int i = 0; i < movementDirections.Count; i++)
        {
            if (interest[i] > danger[i])
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(entity.Body.position, movementDirections[i] * interest[i] * 4f);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(entity.Body.position, movementDirections[i] * danger[i] * 4f);
            }
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(entity.Body.position, moveDir * 8f);

        Gizmos.DrawSphere(moveTo, 0.5f);
    }
}