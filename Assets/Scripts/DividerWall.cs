using System.Collections.Generic;
using UnityEngine;

public class DividerWall : MonoBehaviour
{
    public Entity owner;
    public NpcCombatAI ownerAi;
    public Transform firePoint;
    public float drawingSpeed = 10f;
    public float drawingRotationSpeed = 5f;
    public float drawingLifetime = 2f;
    public float setTargetDelay = 0.5f;
    public GameObject drawingParticles;
    public float drawingParticlesHeight = 1f;
    public float particleDistanceIncrement = 0.2f;
    public GameObject wallPrefab;
    public float wallHeight = 0f;
    public LayerMask groundLayerMask = ~0;

    [Header("Wall Damage Settings")]
    public float wallDamage = 10f;
    public float wallTickInterval = 0.5f;
    public float wallDuration = 5f;
    public DebuffPreset wallDebuffPreset;
    public float wallHitRadius = 0.75f;
    public LayerMask entityLayerMask;

    [Header("Wall Animation")]
    public float wallRiseSpeed = 5f;
    public float wallSinkSpeed = 2f;
    public float wallSinkStartTime = 1.5f;
    public float wallYOffset = -3f;

    class DividerWallInstance
    {
        public bool retargetted = false;
        public Vector3 moveDir;
        public Vector3 drawTargetDir;
        public Vector3 spawnPoint;
        public Vector3 drawingPoint;
        public float drawLifeTime;
        public bool drawingComplete = false;
        public Vector3 lastParticlePos;

        public Vector3 startBoneDir;
        public Vector3 middleBonePos;
        public Vector3 middleBoneDir;
        
        public List<Vector3> curvePoints = new List<Vector3>();
        public Vector3 lastCurvePoint;
    }
    
    class SpawnedWallObject
    {
        public GameObject wallObj;
        public float timer;
        public List<Vector3> curvePoints;
        public float tickTimer;
        public HashSet<Entity> recentlyHitEntities = new HashSet<Entity>();
        public Vector3 targetPosition;
        public bool isRising = true;
        public bool isSinking = false;
    }
    
    List<DividerWallInstance> wallinstances = new List<DividerWallInstance>();
    List<SpawnedWallObject> spawnedWalls = new List<SpawnedWallObject>();

    void Update() { HandleWalls(); }

    public void SpawnWallDrawingProjectile()
    {
        DividerWallInstance newWall = new DividerWallInstance();
        newWall.drawTargetDir = (firePoint.forward * drawingLifetime * drawingSpeed).normalized;
        newWall.startBoneDir = newWall.drawTargetDir;
        newWall.spawnPoint = firePoint.position;
        newWall.drawingPoint = newWall.spawnPoint;
        newWall.drawLifeTime = drawingLifetime;
        newWall.lastParticlePos = newWall.spawnPoint;
        newWall.lastCurvePoint = newWall.spawnPoint;
        
        RaycastHit startGroundHit;
        Vector3 startGroundPoint = newWall.spawnPoint;
        if (Physics.Raycast(newWall.spawnPoint + Vector3.up * wallHeight, Vector3.down, out startGroundHit, wallHeight * 2f, groundLayerMask))
        {
            startGroundPoint = startGroundHit.point;
        }
        newWall.curvePoints.Add(startGroundPoint);
        wallinstances.Add(newWall);
    }

    void HandleWalls()
    {
        foreach (DividerWallInstance wall in wallinstances)
        {
            if (!wall.drawingComplete)
            {
                if (wall.drawLifeTime <= drawingLifetime - setTargetDelay && !wall.retargetted)
                {
                    RaycastHit middleHit;
                    if (Physics.Raycast(wall.drawingPoint, Vector3.down, out middleHit, wallHeight * 2f, groundLayerMask))
                    {
                        wall.middleBonePos = middleHit.point;
                    }
                    wall.middleBoneDir = wall.moveDir;
                    if (ownerAi != null) wall.drawTargetDir = ownerAi.Target.transform.position - wall.drawingPoint;
                    wall.drawTargetDir = new Vector3(wall.drawTargetDir.x, 0f, wall.drawTargetDir.z);
                    wall.drawTargetDir = wall.drawTargetDir.normalized;
                    wall.retargetted = true;
                }

                wall.moveDir = Vector3.Slerp(wall.moveDir, wall.drawTargetDir, Time.deltaTime * drawingRotationSpeed);
                wall.moveDir = new Vector3(wall.moveDir.x, 0f, wall.moveDir.z).normalized;
                wall.drawingPoint += wall.moveDir * drawingSpeed * Time.deltaTime;

                float distSinceLastCurve = Vector3.Distance(wall.drawingPoint, wall.lastCurvePoint);
                if (distSinceLastCurve >= wallHitRadius)
                {
                    RaycastHit groundHit;
                    Vector3 groundPoint = wall.drawingPoint;
                    if (Physics.Raycast(wall.drawingPoint + Vector3.up * wallHeight, Vector3.down, out groundHit, wallHeight * 2f, groundLayerMask))
                    {
                        groundPoint = groundHit.point;
                    }
                    wall.curvePoints.Add(groundPoint);
                    wall.lastCurvePoint = wall.drawingPoint;
                }

                wall.drawLifeTime -= Time.deltaTime;

                float distSinceLast = Vector3.Distance(wall.drawingPoint, wall.lastParticlePos);
                if (distSinceLast >= particleDistanceIncrement)
                {
                    if (drawingParticles != null)
                    {
                        Vector3 rayOrigin = wall.drawingPoint + Vector3.up * drawingParticlesHeight;
                        RaycastHit hit;
                        Vector3 particlePos = rayOrigin;
                        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, drawingParticlesHeight * 2f, groundLayerMask))
                            particlePos = hit.point;
                        Instantiate(drawingParticles, particlePos, Quaternion.LookRotation(wall.moveDir));
                    }
                    wall.lastParticlePos = wall.drawingPoint;
                }
                
                if (wall.drawLifeTime <= 0f)
                {
                    wall.drawingComplete = true;
                    RaycastHit finalGroundHit;
                    Vector3 finalGroundPoint = wall.drawingPoint;
                    if (Physics.Raycast(wall.drawingPoint + Vector3.up * wallHeight, Vector3.down, out finalGroundHit, wallHeight * 2f, groundLayerMask))
                    {
                        finalGroundPoint = finalGroundHit.point;
                    }
                    wall.curvePoints.Add(finalGroundPoint);
                    
                    if (wallPrefab != null)
                    {
                        Vector3 startRayOrigin = wall.spawnPoint + Vector3.up * wallHeight;
                        Vector3 endRayOrigin = wall.drawingPoint + Vector3.up * wallHeight;
                        RaycastHit startHit, endHit;
                        Vector3 wallStartPos = startRayOrigin;
                        Vector3 wallEndPos = endRayOrigin;

                        if (Physics.Raycast(startRayOrigin, Vector3.down, out startHit, wallHeight * 2f, groundLayerMask))
                        {
                            wallStartPos = startHit.point;
                        }
                        if (Physics.Raycast(endRayOrigin, Vector3.down, out endHit, wallHeight * 2f, groundLayerMask))
                        {
                            wallEndPos = endHit.point;
                        }

                        var wallObj = Instantiate(wallPrefab, wallStartPos, Quaternion.LookRotation(wallEndPos - wallStartPos));
                        var armature = wallObj.transform.Find("Armature");
                        if (armature != null)
                        {
                            var startBone = armature.Find("StartBone");
                            var endBone = armature.Find("EndBone");
                            var middleBone = armature.Find("MiddleBone");
                            if (startBone != null)
                            {
                                startBone.position = wallStartPos;
                                startBone.rotation = Quaternion.LookRotation(wall.startBoneDir);
                            }
                            if (endBone != null)
                            {
                                endBone.position = wallEndPos;
                                endBone.rotation = Quaternion.LookRotation(wall.moveDir);
                            }
                            if (middleBone != null)
                            {
                                middleBone.position = wall.middleBonePos;
                                middleBone.rotation = Quaternion.LookRotation(wall.middleBoneDir);
                            }
                        }

                        SpawnedWallObject spawnedWall = new SpawnedWallObject
                        {
                            wallObj = wallObj,
                            timer = wallDuration,
                            curvePoints = new List<Vector3>(wall.curvePoints),
                            tickTimer = wallTickInterval,
                            targetPosition = wallStartPos,
                            isRising = true
                        };
                        
                        wallObj.transform.position = wallStartPos + Vector3.up * wallYOffset;
                        spawnedWalls.Add(spawnedWall);
                    }
                }
            }
        }

        for (int i = spawnedWalls.Count - 1; i >= 0; i--)
        {
            SpawnedWallObject spawnedWall = spawnedWalls[i];
            spawnedWall.timer -= Time.deltaTime;
            spawnedWall.tickTimer -= Time.deltaTime;

            if (spawnedWall.isRising && spawnedWall.wallObj != null)
            {
                spawnedWall.wallObj.transform.position = Vector3.Lerp(spawnedWall.wallObj.transform.position, spawnedWall.targetPosition, Time.deltaTime * wallRiseSpeed);
                if (Vector3.Distance(spawnedWall.wallObj.transform.position, spawnedWall.targetPosition) < 0.1f)
                {
                    spawnedWall.wallObj.transform.position = spawnedWall.targetPosition;
                    spawnedWall.isRising = false;
                }
            }

            if (spawnedWall.timer <= wallSinkStartTime && !spawnedWall.isSinking && !spawnedWall.isRising)
            {
                spawnedWall.isSinking = true;
                spawnedWall.targetPosition = spawnedWall.targetPosition + Vector3.up * wallYOffset;
            }

            if (spawnedWall.isSinking && spawnedWall.wallObj != null)
            {
                spawnedWall.wallObj.transform.position = Vector3.Lerp(spawnedWall.wallObj.transform.position, spawnedWall.targetPosition, Time.deltaTime * wallSinkSpeed);
            }

            if (spawnedWall.timer <= 0f)
            {
                if (spawnedWall.wallObj != null)
                    Destroy(spawnedWall.wallObj);
                spawnedWalls.RemoveAt(i);
                continue;
            }

            if (spawnedWall.tickTimer <= 0f)
            {
                spawnedWall.tickTimer = wallTickInterval;
                
                foreach (Vector3 point in spawnedWall.curvePoints)
                {
                    Collider[] hits = Physics.OverlapSphere(point, wallHitRadius, entityLayerMask);
                    foreach (Collider hit in hits)
                    {
                        Entity entity = hit.GetComponent<Entity>();
                        if (entity == null) continue;
                        
                        if (owner != null && entity.Team == owner.Team) continue;
                        
                        if (spawnedWall.recentlyHitEntities.Contains(entity)) continue;
                        
                        DamageInstance baseDamage = new DamageInstance(null);
                        baseDamage.HealthDamage = wallDamage;
                        baseDamage.DamageType = DamageInstance.DamageTypes.DirectDamage;
                        DamageInstance dmg = new DamageInstance(baseDamage);
                        
                        entity.TakeDamage(dmg, owner, false);
                        
                        if (wallDebuffPreset != null)
                        {
                            entity.ApplyDebuff(wallDebuffPreset, owner);
                        }
                        
                        spawnedWall.recentlyHitEntities.Add(entity);
                    }
                }
                
                spawnedWall.recentlyHitEntities.Clear();
            }
        }
    }

    void OnDrawGizmos()
    {
        if (spawnedWalls != null)
        {
            Gizmos.color = Color.red;
            foreach (SpawnedWallObject spawnedWall in spawnedWalls)
            {
                if (spawnedWall.curvePoints != null)
                {
                    foreach (Vector3 point in spawnedWall.curvePoints)
                    {
                        Gizmos.DrawWireSphere(point, wallHitRadius);
                    }
                }
            }
        }

        if (wallinstances != null)
        {
            Gizmos.color = Color.yellow;
            foreach (DividerWallInstance wall in wallinstances)
            {
                if (!wall.drawingComplete)
                {
                    Gizmos.DrawWireSphere(wall.drawingPoint, wallHitRadius * 0.5f);
                    
                    Gizmos.color = Color.cyan;
                    foreach (Vector3 point in wall.curvePoints)
                    {
                        Gizmos.DrawWireSphere(point, wallHitRadius * 0.7f);
                    }
                    Gizmos.color = Color.yellow;
                }
            }
        }
    }
}
