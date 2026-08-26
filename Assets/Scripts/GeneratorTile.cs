using UnityEngine;
using System.Collections.Generic;

public enum TileCategory
{
    Normal, Boss, Secret, Treasure, Shop, Checkpoint
}

public class GeneratorTile : MonoBehaviour
{
    [Header("Tile Properties")]
    public float distanceFromEnd;
    public float dangerLevel;
    public Vector3 tileSize = Vector3.one;
    public Vector3 tileOffset = Vector3.zero;
    public int pointCost = 1;
    public float weight = 1f;
    public TileCategory category = TileCategory.Normal;
    
    [Header("Spawn Points")]
    public List<TileSpawnPoint> spawnPoints = new List<TileSpawnPoint>();
    
    [Header("Debug")]
    public bool showGizmos = true;
    public Color gizmoColor = Color.green;
    
    private Bounds tileBounds;

    public Bounds TileBounds { get { UpdateBounds(); return tileBounds; } }

    [System.Serializable]
    public class TileSpawnPoint
    {
        public enum SpawnPointTypes { Main, Side }
        public SpawnPointTypes spawnPointType;
        public Transform spawnPoint;
        public bool isUsed = false;
    }
    
    void Awake()
    {
        /*if (GetComponent<Collider>() == null)
        {
            var boxCol = gameObject.AddComponent<BoxCollider>();
            boxCol.size = tileSize;
            boxCol.isTrigger = true;
        }*/
        UpdateBounds();
    }
    
    public void UpdateBounds()
    {
        Vector3 boundsCenter = transform.position + transform.TransformDirection(tileOffset);
        Vector3 rotatedSize = new Vector3(
            Mathf.Abs(transform.right.x * tileSize.x) + Mathf.Abs(transform.up.x * tileSize.y) + Mathf.Abs(transform.forward.x * tileSize.z),
            Mathf.Abs(transform.right.y * tileSize.x) + Mathf.Abs(transform.up.y * tileSize.y) + Mathf.Abs(transform.forward.y * tileSize.z),
            Mathf.Abs(transform.right.z * tileSize.x) + Mathf.Abs(transform.up.z * tileSize.y) + Mathf.Abs(transform.forward.z * tileSize.z)
        );
        tileBounds = new Bounds(boundsCenter, rotatedSize);
    }
    
    public bool IsIntersectingWith(GeneratorTile otherTile)
    {
        if (otherTile == null || otherTile == this) return false;
        UpdateBounds();
        otherTile.UpdateBounds();
        return tileBounds.Intersects(otherTile.tileBounds);
    }
    
    public List<TileSpawnPoint> GetAvailableSpawnPoints(TileSpawnPoint.SpawnPointTypes type)
    {
        var availablePoints = new List<TileSpawnPoint>();
        foreach (var point in spawnPoints)
        {
            if (point.spawnPointType == type && !point.isUsed && point.spawnPoint != null)
                availablePoints.Add(point);
        }
        return availablePoints;
    }
    
    public void UseSpawnPoint(TileSpawnPoint spawnPoint) => spawnPoint.isUsed = true;
    
    public void ResetSpawnPoints()
    {
        foreach (var point in spawnPoints)
            point.isUsed = false;
    }
    
    void OnDrawGizmos() { if (showGizmos) DrawTileGizmos(0.5f); }
    void OnDrawGizmosSelected() { if (showGizmos) DrawTileGizmos(1.0f); }
    
    void DrawTileGizmos(float alpha)
    {
        Vector3 boundsCenter = transform.position + transform.TransformDirection(tileOffset);
        
        var color = gizmoColor;
        color.a = alpha;
        Gizmos.color = color;
        
        var oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(boundsCenter, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, tileSize);
        Gizmos.matrix = oldMatrix;
        
        foreach (var point in spawnPoints)
        {
            if (point.spawnPoint != null)
            {
                var pointColor = point.spawnPointType == TileSpawnPoint.SpawnPointTypes.Main ? Color.blue : Color.red;
                if (point.isUsed) pointColor = Color.gray;
                pointColor.a = alpha;
                
                Gizmos.color = pointColor;
                Gizmos.DrawSphere(point.spawnPoint.position, 0.2f);
                Gizmos.DrawRay(point.spawnPoint.position, point.spawnPoint.forward * 0.5f);
            }
        }
    }
}
