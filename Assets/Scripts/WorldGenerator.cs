using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

public class WorldGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    public int mainTilePoints = 10;
    public int sideTilePoints = 5;
    public int maxGenerationAttempts = 10;
    
    [Header("Tile Prefabs")]
    public List<GeneratorTile> mainTilePrefabs = new List<GeneratorTile>();
    public List<GeneratorTile> sideTilePrefabs = new List<GeneratorTile>();
    
    [Header("Spawn Settings")]
    public GeneratorTile endTileInScene;
    public GeneratorTile finalSpawnTile;
    
    [Header("Debug")]
    public bool debugMode = true;
    public bool autoGenerate = true;
    
    [Header("Structure & Danger System")]
    public bool autoInitializeStructures = true;
    public float dangerBonusHealth = 2.5f;
    public float dangerBonusHealthCap = 2.5f;
    public float initialBonusHealth = 0f;
    
    [Header("Visual Map Generation")]
    public GameObject tileMapPrefab;
    public float mapElevation = 100f;
    public float mapScale = 2.5f;
    
    [Header("Player Spawn Management")]
    public bool autoTeleportPlayer = true;
    private bool playerTeleported = false;
    
    private List<GeneratorTile> spawnedTiles = new List<GeneratorTile>();
    private GeneratorTile placedFinalSpawnTile;
    private Dictionary<GeneratorTile, int> tileSpawnCounts = new Dictionary<GeneratorTile, int>();
    private int totalPointsUsed = 0;
    
    void Start() { if (autoGenerate) GenerateWorld(); }
    
    void Update()
    {
        // Player spawn management
        if (autoTeleportPlayer && !playerTeleported && UnitManager.Instance != null)
        {
            if (UnitManager.Instance.playerObj != null && placedFinalSpawnTile != null)
            {
                Vector3 spawnPosition = GetPlayerSpawnPosition();
                UnitManager.Instance.playerObj.transform.position = spawnPosition + Vector3.up * 0.75f;
                playerTeleported = true;
                if (debugMode) Debug.Log($"Player teleported to: {spawnPosition}");
            }
        }
    }
    
    public void GenerateWorld()
    {
        ClearWorld();
        if (!ValidateSettings()) return;
        
        playerTeleported = false; // Reset for new world
        tileSpawnCounts.Clear();
        totalPointsUsed = 0;
        spawnedTiles.Add(endTileInScene);
        endTileInScene.ResetSpawnPoints();
        
        var mainPathTiles = GenerateMainPath();
        PlaceFinalSpawnTile(mainPathTiles);
        GenerateSideTiles(mainPathTiles);
        
        if (autoInitializeStructures)
            InitializeStructureSpawning();
        
        GenerateVisualMap();
        
        if (debugMode) Debug.Log($"World generated: {spawnedTiles.Count} tiles, {totalPointsUsed} points used");
    }
    
    bool ValidateSettings()
    {
        if (endTileInScene == null) { Debug.LogError("End tile not assigned!"); return false; }
        if (finalSpawnTile == null) { Debug.LogError("Final spawn tile not assigned!"); return false; }
        if (mainTilePrefabs.Count == 0) { Debug.LogError("No main tile prefabs assigned!"); return false; }
        return true;
    }
    
    List<GeneratorTile> GenerateMainPath()
    {
        var mainPathTiles = new List<GeneratorTile>();
        var currentTile = endTileInScene;
        var remainingPoints = mainTilePoints;
        
        while (remainingPoints > 0)
        {
            var mainSpawnPoints = currentTile.GetAvailableSpawnPoints(GeneratorTile.TileSpawnPoint.SpawnPointTypes.Main);
            if (mainSpawnPoints.Count == 0) break;
            
            var affordableTiles = GetAffordableTiles(mainTilePrefabs, remainingPoints);
            if (affordableTiles.Count == 0) break;
            
            var selectedTile = SelectTileByWeight(affordableTiles);
            var newTile = PlaceTileOnSpawnPoint(selectedTile, mainSpawnPoints[0]);
            
            if (newTile != null)
            {
                remainingPoints -= newTile.pointCost;
                totalPointsUsed += newTile.pointCost;
                currentTile.UseSpawnPoint(mainSpawnPoints[0]);
                TrackTileSpawn(selectedTile);
                
                foreach (var spawnPoint in mainSpawnPoints)
                    if (spawnPoint != mainSpawnPoints[0])
                        currentTile.UseSpawnPoint(spawnPoint);
                
                newTile.distanceFromEnd = Vector3.Distance(newTile.transform.position, endTileInScene.transform.position);
                mainPathTiles.Add(newTile);
                currentTile = newTile;
            }
            else break;
        }
        
        return mainPathTiles;
    }
    
    void PlaceFinalSpawnTile(List<GeneratorTile> mainPathTiles)
    {
        if (mainPathTiles.Count == 0)
        {
            placedFinalSpawnTile = Instantiate(finalSpawnTile, transform.position, transform.rotation);
            spawnedTiles.Add(placedFinalSpawnTile);
            return;
        }
        
        var firstTileFromEnd = mainPathTiles[mainPathTiles.Count - 1];
        var mainSpawnPoints = firstTileFromEnd.GetAvailableSpawnPoints(GeneratorTile.TileSpawnPoint.SpawnPointTypes.Main);
        
        if (mainSpawnPoints.Count == 0)
        {
            placedFinalSpawnTile = Instantiate(finalSpawnTile, transform.position, transform.rotation);
            spawnedTiles.Add(placedFinalSpawnTile);
            return;
        }
        
        placedFinalSpawnTile = PlaceTileOnSpawnPoint(finalSpawnTile, mainSpawnPoints[0]);
        
        if (placedFinalSpawnTile != null)
        {
            firstTileFromEnd.UseSpawnPoint(mainSpawnPoints[0]);
            foreach (var otherSpawnPoint in mainSpawnPoints)
                if (otherSpawnPoint != mainSpawnPoints[0])
                    firstTileFromEnd.UseSpawnPoint(otherSpawnPoint);
            
            placedFinalSpawnTile.distanceFromEnd = Vector3.Distance(placedFinalSpawnTile.transform.position, endTileInScene.transform.position);
        }
        else
        {
            placedFinalSpawnTile = Instantiate(finalSpawnTile, transform.position, transform.rotation);
            spawnedTiles.Add(placedFinalSpawnTile);
        }
    }
    
    void GenerateSideTiles(List<GeneratorTile> mainPathTiles)
    {
        var generatedSideTiles = new List<GeneratorTile>();
        var remainingPoints = sideTilePoints;
        
        while (remainingPoints > 0)
        {
            var (availableSpawnPoints, parentTiles) = GetAllAvailableSpawnPoints(mainPathTiles, generatedSideTiles);
            if (availableSpawnPoints.Count == 0) break;
            
            var affordableSideTiles = GetAffordableTiles(sideTilePrefabs, remainingPoints);
            if (affordableSideTiles.Count == 0) break;
            
            ShuffleSpawnPoints(availableSpawnPoints, parentTiles);
            
            bool placedTileThisRound = false;
            for (int i = 0; i < availableSpawnPoints.Count; i++)
            {
                var spawnPoint = availableSpawnPoints[i];
                var parentTile = parentTiles[i];
                
                if (spawnPoint.isUsed) continue;
                
                var sidePrefab = SelectTileByWeight(affordableSideTiles);
                var newTile = PlaceTileOnSpawnPoint(sidePrefab, spawnPoint);
                
                if (newTile != null)
                {
                    remainingPoints -= newTile.pointCost;
                    totalPointsUsed += newTile.pointCost;
                    parentTile.UseSpawnPoint(spawnPoint);
                    newTile.distanceFromEnd = parentTile.distanceFromEnd + Vector3.Distance(newTile.transform.position, parentTile.transform.position);
                    generatedSideTiles.Add(newTile);
                    TrackTileSpawn(sidePrefab);
                    placedTileThisRound = true;
                    break;
                }
            }
            
            if (!placedTileThisRound) break;
        }
    }
    
    List<GeneratorTile> GetAffordableTiles(List<GeneratorTile> tilePrefabs, int remainingPoints)
    {
        var affordableTiles = new List<GeneratorTile>();
        foreach (var tilePrefab in tilePrefabs)
        {
            if (tilePrefab.pointCost <= remainingPoints)
            {
                affordableTiles.Add(tilePrefab);
            }
        }
        return affordableTiles;
    }

    GeneratorTile SelectTileByWeight(List<GeneratorTile> tiles)
    {
        if (tiles.Count == 0) return null;
        if (tiles.Count == 1) return tiles[0];
        
        float totalWeight = tiles.Sum(tile => tile.weight);
        if (totalWeight <= 0) return tiles[Random.Range(0, tiles.Count)]; // Fallback to random if no weights
        
        float randomValue = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;
        
        foreach (var tile in tiles)
        {
            cumulativeWeight += tile.weight;
            if (randomValue <= cumulativeWeight)
            {
                return tile;
            }
        }
        
        return tiles[tiles.Count - 1]; // Fallback to last tile
    }

    List<GeneratorTile> GetTilesByCategory(TileCategory category)
    {
        var result = new List<GeneratorTile>();
        result.AddRange(mainTilePrefabs.FindAll(tile => tile.category == category));
        result.AddRange(sideTilePrefabs.FindAll(tile => tile.category == category));
        return result;
    }
    
    int GetTileSpawnCount(GeneratorTile tilePrefab)
    {
        return tileSpawnCounts.ContainsKey(tilePrefab) ? tileSpawnCounts[tilePrefab] : 0;
    }
    
    int GetSpawnedTileCount(TileCategory category)
    {
        int count = 0;
        foreach (var tile in spawnedTiles)
        {
            if (tile != null && tile != endTileInScene && tile.category == category)
                count++;
        }
        return count;
    }
    
    void TrackTileSpawn(GeneratorTile tilePrefab)
    {
        if (tileSpawnCounts.ContainsKey(tilePrefab))
            tileSpawnCounts[tilePrefab]++;
        else
            tileSpawnCounts[tilePrefab] = 1;
    }
    
    (List<GeneratorTile.TileSpawnPoint>, List<GeneratorTile>) GetAllAvailableSpawnPointsForGeneration()
    {
        var availableSpawnPoints = new List<GeneratorTile.TileSpawnPoint>();
        var parentTiles = new List<GeneratorTile>();
        
        foreach (var tile in spawnedTiles)
        {
            if (tile == null || tile == endTileInScene) continue;
            AddSpawnPointsFromTile(tile, availableSpawnPoints, parentTiles);
        }
        
        var endTileSideSpawnPoints = endTileInScene.GetAvailableSpawnPoints(GeneratorTile.TileSpawnPoint.SpawnPointTypes.Side);
        foreach (var spawnPoint in endTileSideSpawnPoints)
        {
            availableSpawnPoints.Add(spawnPoint);
            parentTiles.Add(endTileInScene);
        }
        
        return (availableSpawnPoints, parentTiles);
    }
    
    (List<GeneratorTile.TileSpawnPoint>, List<GeneratorTile>) GetAllAvailableSpawnPoints(List<GeneratorTile> mainPathTiles, List<GeneratorTile> generatedSideTiles)
    {
        var availableSpawnPoints = new List<GeneratorTile.TileSpawnPoint>();
        var parentTiles = new List<GeneratorTile>();
        
        var endTileSideSpawnPoints = endTileInScene.GetAvailableSpawnPoints(GeneratorTile.TileSpawnPoint.SpawnPointTypes.Side);
        foreach (var spawnPoint in endTileSideSpawnPoints)
        {
            availableSpawnPoints.Add(spawnPoint);
            parentTiles.Add(endTileInScene);
        }
        
        foreach (var mainTile in mainPathTiles)
        {
            var sideSpawnPoints = mainTile.GetAvailableSpawnPoints(GeneratorTile.TileSpawnPoint.SpawnPointTypes.Side);
            foreach (var spawnPoint in sideSpawnPoints)
            {
                availableSpawnPoints.Add(spawnPoint);
                parentTiles.Add(mainTile);
            }
        }
        
        if (placedFinalSpawnTile != null)
            AddSpawnPointsFromTile(placedFinalSpawnTile, availableSpawnPoints, parentTiles);
        
        foreach (var sideTile in generatedSideTiles)
            AddSpawnPointsFromTile(sideTile, availableSpawnPoints, parentTiles);
        
        return (availableSpawnPoints, parentTiles);
    }
    
    void AddSpawnPointsFromTile(GeneratorTile tile, List<GeneratorTile.TileSpawnPoint> spawnPointsList, List<GeneratorTile> parentTilesList)
    {
        var mainSpawnPoints = tile.GetAvailableSpawnPoints(GeneratorTile.TileSpawnPoint.SpawnPointTypes.Main);
        foreach (var spawnPoint in mainSpawnPoints)
        {
            spawnPointsList.Add(spawnPoint);
            parentTilesList.Add(tile);
        }
        
        var sideSpawnPoints = tile.GetAvailableSpawnPoints(GeneratorTile.TileSpawnPoint.SpawnPointTypes.Side);
        foreach (var spawnPoint in sideSpawnPoints)
        {
            spawnPointsList.Add(spawnPoint);
            parentTilesList.Add(tile);
        }
    }
    
    void ShuffleSpawnPoints(List<GeneratorTile.TileSpawnPoint> availableSpawnPoints, List<GeneratorTile> parentTiles)
    {
        for (int i = availableSpawnPoints.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (availableSpawnPoints[i], availableSpawnPoints[randomIndex]) = (availableSpawnPoints[randomIndex], availableSpawnPoints[i]);
            (parentTiles[i], parentTiles[randomIndex]) = (parentTiles[randomIndex], parentTiles[i]);
        }
    }
    
    GeneratorTile PlaceTileOnSpawnPoint(GeneratorTile tilePrefab, GeneratorTile.TileSpawnPoint spawnPoint)
    {
        if (spawnPoint?.spawnPoint == null) return null;
        
        var spawnPosition = spawnPoint.spawnPoint.position;
        var spawnRotation = spawnPoint.spawnPoint.rotation;
        
        for (int attempt = 0; attempt < maxGenerationAttempts; attempt++)
        {
            var tryPosition = spawnPosition;
            if (attempt > 0)
            {
                var smallOffset = new Vector3(Random.Range(-0.05f, 0.05f), 0f, Random.Range(-0.05f, 0.05f));
                tryPosition = spawnPosition + smallOffset;
            }
            
            var newTileObject = Instantiate(tilePrefab.gameObject, tryPosition, spawnRotation);
            var newTile = newTileObject.GetComponent<GeneratorTile>();
            
            bool hasIntersection = false;
            foreach (var existingTile in spawnedTiles)
            {
                if (newTile.IsIntersectingWith(existingTile))
                {
                    hasIntersection = true;
                    break;
                }
            }
            
            if (!hasIntersection)
            {
                spawnedTiles.Add(newTile);
                return newTile;
            }
            else
            {
                DestroyImmediate(newTileObject);
            }
        }
        
        return null;
    }
    
    public void ClearWorld()
    {
        foreach (var tile in spawnedTiles)
            if (tile != null && tile != endTileInScene)
                DestroyImmediate(tile.gameObject);
        
        spawnedTiles.Clear();
        placedFinalSpawnTile = null;
        tileSpawnCounts.Clear();
        totalPointsUsed = 0;
        playerTeleported = false; // Reset player teleportation flag
    }
    
    public Vector3 GetPlayerSpawnPosition() 
    {
        if (placedFinalSpawnTile == null) return transform.position;
        
        // Look for PlayerSpawnPoint object inside the spawn tile
        Transform playerSpawnPoint = placedFinalSpawnTile.transform.Find("PlayerSpawnPoint");
        if (playerSpawnPoint != null)
        {
            return playerSpawnPoint.position;
        }
        
        // Fallback to tile position if PlayerSpawnPoint not found
        return placedFinalSpawnTile.transform.position;
    }
    public List<GeneratorTile> GetAllTiles() => new List<GeneratorTile>(spawnedTiles);

    void GenerateVisualMap()
    {
        if (tileMapPrefab == null) return;
        
        // Generate map for all spawned tiles (including boss/end tile)
        foreach (var tile in spawnedTiles)
        {
            if (tile != null)
            {
                // Position map tile at the bounds center (considering offset) + elevation
                Vector3 boundsCenter = tile.transform.position + tile.transform.TransformDirection(tile.tileOffset);
                Vector3 mapPosition = boundsCenter + Vector3.up * mapElevation;
                GameObject mapTile = Instantiate(tileMapPrefab, mapPosition, Quaternion.Euler(90f, 0f, 0f), tile.transform);
                
                // Scale to match actual bounds dimensions
                Vector3 bounds = tile.TileBounds.size;
                mapTile.transform.localScale = new Vector3(bounds.x * mapScale, bounds.z * mapScale, 1f);
                
                if (debugMode) Debug.Log($"Generated map tile for {tile.name} at {mapPosition}");
            }
        }
        
        // Also generate for the final spawn tile if it exists and isn't already in spawnedTiles
        if (placedFinalSpawnTile != null && !spawnedTiles.Contains(placedFinalSpawnTile))
        {
            Vector3 boundsCenter = placedFinalSpawnTile.transform.position + placedFinalSpawnTile.transform.TransformDirection(placedFinalSpawnTile.tileOffset);
            Vector3 mapPosition = boundsCenter + Vector3.up * mapElevation;
            GameObject mapTile = Instantiate(tileMapPrefab, mapPosition, Quaternion.Euler(90f, 0f, 0f), placedFinalSpawnTile.transform);
            Vector3 bounds = placedFinalSpawnTile.TileBounds.size;
            mapTile.transform.localScale = new Vector3(bounds.x * mapScale, bounds.z * mapScale, 1f);
        }
    }
    
    void OnDrawGizmos()
    {
        if (!debugMode) return;
        
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 1f);
        Gizmos.DrawRay(transform.position, Vector3.up * 3f);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 5f);
        
        if (endTileInScene != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, endTileInScene.transform.position);
            Gizmos.DrawSphere(endTileInScene.transform.position, 0.5f);
        }
    }
    
    public void InitializeStructureSpawning()
    {
        var allTiles = new List<GeneratorTile>(spawnedTiles);
        if (placedFinalSpawnTile != null)
            allTiles.Add(placedFinalSpawnTile);
        CalculateAndApplyDangerSystem(allTiles);
    }
    
    public void CalculateAndApplyDangerSystem(List<GeneratorTile> tiles)
    {
        if (tiles.Count == 0) return;
        CalculateTileDanger(tiles);
        SetStructureDanger(tiles);
    }
    
    void CalculateTileDanger(List<GeneratorTile> tiles)
    {
        float maxDistance = 0f;
        foreach (var tile in tiles)
            if (tile.distanceFromEnd > maxDistance)
                maxDistance = tile.distanceFromEnd;
        
        foreach (var tile in tiles)
            tile.dangerLevel = maxDistance > 0 ? 1f - (tile.distanceFromEnd / maxDistance) : 0f;
    }
    
    void SetStructureDanger(List<GeneratorTile> tiles)
    {
        foreach (var tile in tiles)
        {
            var structureGen = tile.GetComponent("StructureGeneration");
            if (structureGen != null)
                SetDangerOnComponent(structureGen, tile.dangerLevel);
            
            var childComponents = tile.GetComponentsInChildren(typeof(MonoBehaviour));
            foreach (var component in childComponents)
            {
                if (component.GetType().Name == "StructureGeneration")
                    SetDangerOnComponent(component, tile.dangerLevel);
            }
        }
    }
    
    void SetDangerOnComponent(Component component, float dangerLevel)
    {
        var type = component.GetType();
        
        type.GetField("spawnerDanger")?.SetValue(component, dangerLevel);
        type.GetField("dangerBonusHealth")?.SetValue(component, dangerBonusHealth);
        type.GetField("dangerBonusHealthCap")?.SetValue(component, dangerBonusHealthCap);
        type.GetField("initialBonusHealth")?.SetValue(component, initialBonusHealth);
        type.GetMethod("InitializeStructure")?.Invoke(component, null);
    }
    
    public void RecalculateDanger()
    {
        var allTiles = new List<GeneratorTile>(spawnedTiles);
        if (placedFinalSpawnTile != null)
            allTiles.Add(placedFinalSpawnTile);
        CalculateAndApplyDangerSystem(allTiles);
    }

    public void RegenerateWorldAndTeleportPlayer()
    {
        GenerateWorld();
        // Force immediate teleportation if UnitManager and player exist
        if (UnitManager.Instance != null && UnitManager.Instance.playerObj != null && placedFinalSpawnTile != null)
        {
            Vector3 spawnPosition = GetPlayerSpawnPosition();
            UnitManager.Instance.playerObj.transform.position = spawnPosition + Vector3.up * 0.75f;
            playerTeleported = true;
            if (debugMode) Debug.Log($"Player force-teleported to: {spawnPosition}");
        }
    }
}
