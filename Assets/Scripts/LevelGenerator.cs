using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

public class LevelGenerator : MonoBehaviour
{
    [Header("Danger Settings")]
    public float dangerBonusHealth = 2.5f;
    public float dangerBonusHealthCap = 2.5f;
    public float initialBonusHealth = 0f;

    [Header("Tile Generation")]
    public Transform generationPointer;
    int pathTilePointsUsed;
    public int maxPathTilePoints = 20;
    public float maxTileRotation = 100f;
    public float tileRandomRotation = 50f;

    public float initialTileRadius = 22.5f;

    int sideTilePointsUsed;
    public int maxSideTilePoints = 10;

    public GameObject spawnTilePrefab;
    public float spawnTilePlacementDist = 22.5f;
    public int MaxGenerationTries = 1024;
    int remainingTries = 0;

    public LayerMask groundMask;

    [System.Serializable]
    public class Folliage
    {
        public GameObject folliageObject;
        public float weight;
    }

    [System.Serializable]
    public class FolliageGeneration
    {
        public float spawnChance = 100f;
        public int minSpawnCount = 2;
        public int maxSpawnCount = 5;
        public Folliage[] folliages;
    }

    [System.Serializable]
    public class TileGeneration
    {
        public float weight;
        [Space(10)]
        public GameObject tileObject;
        public int tilePoints = 4;
        public float tilePlacementDist = 22.5f;
        [Header("Folliage Generation")]
        public FolliageGeneration[] generationFolliages;
    }

    public TileGeneration[] generationPathNodes;
    public TileGeneration[] generationSideNodes;
    TileGeneration lastSelectedTile = null;

    public Transform bossTile;
    public Transform spawnTile;

    [System.Serializable]
    public class Tile
    {
        public TileGeneration tileGeneration;
        public Transform tileTransform;
        public Vector3 tileSpawnDir;
        public int tilePos;
        public int tileDanger;
        public float tileDangerPerc;
    }

    public GameObject tileOnMap;

    public List<Tile> spawnedPathTiles = new List<Tile>();
    public List<Tile> spawnedSideTiles = new List<Tile>();
    public List<GameObject> spawnedFolliage = new List<GameObject>();

    public bool playerTeleported = false;

    private void Start()
    {
        GeneratePathNodes();
    }

    private void Update()
    {
        if (playerTeleported == false)
        {
            if (UnitManager.Instance.playerObj != null && spawnTile != null)
            {
                UnitManager.Instance.playerObj.transform.position = spawnTile.position + new Vector3(0f, 0.75f, 0f);
                playerTeleported = true;
            }
        }
    }

    public void GeneratePathNodes()
    {
        foreach (Tile tile in spawnedPathTiles)
        {
            Destroy(tile.tileTransform.gameObject);
        }
        spawnedPathTiles.Clear();
        if (spawnTile != null) Destroy(spawnTile.gameObject);
        pathTilePointsUsed = 0;
        generationPointer.transform.localPosition = Vector3.zero;
        generationPointer.rotation = Quaternion.identity;

        if (generationPathNodes.Length > 0)
        {
            float lowestTilePoints = 99999999f;
            foreach (TileGeneration tile in generationPathNodes)
            {
                if (tile.tilePoints < lowestTilePoints)
                {
                    lowestTilePoints = tile.tilePoints;
                }
            }

            float minRot = -tileRandomRotation;
            float maxRot = tileRandomRotation;

            while ((pathTilePointsUsed + lowestTilePoints) <= maxPathTilePoints)
            {
                List<TileGeneration> validTiles = new List<TileGeneration>();
                foreach (TileGeneration n in generationPathNodes)
                {
                    if ((pathTilePointsUsed + n.tilePoints) <= maxPathTilePoints) validTiles.Add(n);
                }

                float totalWeight = validTiles.Sum(tile => tile.weight);

                if (totalWeight > 0)
                {
                    float randomValue = Random.Range(0, totalWeight);
                    float cumulativeWeight = 0f;

                    bool tileSelected = false;
                    foreach (TileGeneration tile in validTiles)
                    {
                        if (tileSelected == false)
                        {
                            cumulativeWeight += tile.weight;
                            if (randomValue <= cumulativeWeight)
                            {
                                minRot = -tileRandomRotation;
                                maxRot = tileRandomRotation;

                                if (generationPointer.localRotation.y + minRot < -maxTileRotation) minRot = -maxTileRotation + generationPointer.localRotation.y;
                                if (generationPointer.localRotation.y + maxRot > maxTileRotation) maxRot = maxTileRotation - generationPointer.localRotation.y;

                                generationPointer.localRotation = Quaternion.Euler(0f, generationPointer.localRotation.y + Random.Range(minRot, maxRot), 0f);
                                if (lastSelectedTile == null)
                                {
                                    generationPointer.transform.position -= generationPointer.forward.normalized * (initialTileRadius + tile.tilePlacementDist);
                                }
                                else
                                {
                                    generationPointer.transform.position -= generationPointer.forward.normalized * (lastSelectedTile.tilePlacementDist + tile.tilePlacementDist);
                                }
                                GameObject spawnedTileObject = Instantiate(tile.tileObject, generationPointer.position + new Vector3(0f, (spawnedPathTiles.Count + spawnedSideTiles.Count + 1) * -0.0001f, 0f), generationPointer.rotation);

                                Tile spawnedTile = new Tile();
                                spawnedTile.tileGeneration = tile;
                                spawnedTile.tileTransform = spawnedTileObject.transform;
                                spawnedTile.tileSpawnDir = spawnedTileObject.transform.forward;
                                spawnedPathTiles.Add(spawnedTile);
                                spawnedTile.tilePos = spawnedPathTiles.Count;

                                pathTilePointsUsed += tile.tilePoints;
                                lastSelectedTile = tile;
                                tileSelected = true;
                            }
                        }
                    }
                }
            }

            minRot = -tileRandomRotation;
            maxRot = tileRandomRotation;

            if (generationPointer.localRotation.y + minRot < -maxTileRotation) minRot = -maxTileRotation + generationPointer.localRotation.y;
            if (generationPointer.localRotation.y + maxRot > maxTileRotation) maxRot = maxTileRotation - generationPointer.localRotation.y;

            generationPointer.localRotation = Quaternion.Euler(0f, generationPointer.localRotation.y + Random.Range(minRot, maxRot), 0f);
            if (lastSelectedTile == null)
            {
                generationPointer.transform.position -= generationPointer.forward.normalized * (initialTileRadius + spawnTilePlacementDist);
            }
            else
            {
                generationPointer.transform.position -= generationPointer.forward.normalized * (lastSelectedTile.tilePlacementDist + spawnTilePlacementDist);
            }
            spawnTile = Instantiate(spawnTilePrefab, generationPointer.position, generationPointer.rotation).transform;
        }

        GenerateSideNodes();
    }

    void GenerateSideNodes()
    {
        remainingTries = MaxGenerationTries;
        foreach (Tile tile in spawnedSideTiles)
        {
            Destroy(tile.tileTransform.gameObject);
        }
        spawnedSideTiles.Clear();
        sideTilePointsUsed = 0;
        if (generationSideNodes.Length > 0)
        {
            float lowestNodePoints = 99999999f;
            foreach (TileGeneration node in generationSideNodes)
            {
                if (node.tilePoints < lowestNodePoints)
                {
                    lowestNodePoints = node.tilePoints;
                }
            }

            while ((sideTilePointsUsed + lowestNodePoints) <= maxSideTilePoints && remainingTries > 0)
            {
                int randomChosenTile = Random.Range(0, spawnedPathTiles.Count + spawnedSideTiles.Count);

                if (randomChosenTile < spawnedPathTiles.Count)
                {
                    Transform chosenTile = spawnedPathTiles[randomChosenTile].tileTransform;
                    Transform chosenTileHigher = bossTile;
                    Transform chosenTileLower = spawnTile;


                    if (randomChosenTile - 1 > -1) chosenTileHigher = spawnedPathTiles[randomChosenTile - 1].tileTransform;

                    if (randomChosenTile + 1 < spawnedPathTiles.Count) chosenTileLower = spawnedPathTiles[randomChosenTile + 1].tileTransform;

                    Vector2 dir = new Vector2(chosenTileHigher.position.x - chosenTileLower.position.x, chosenTileHigher.position.z - chosenTileLower.position.z).normalized;
                    Vector2 perpendicularDir = Vector2.Perpendicular(dir);
                    Vector3 newDir = new Vector3(perpendicularDir.x, 0, perpendicularDir.y);
                    Vector3 midPoint = new Vector3((chosenTileHigher.position.x + chosenTileLower.position.x) / 2f, 0f, (chosenTileHigher.position.z + chosenTileLower.position.z) / 2f);
                    List<TileGeneration> validNodes = new List<TileGeneration>();
                    foreach (TileGeneration n in generationSideNodes)
                    {
                        if ((sideTilePointsUsed + n.tilePoints) <= maxSideTilePoints) validNodes.Add(n);
                    }

                    float totalWeight = validNodes.Sum(node => node.weight);

                    if (totalWeight > 0)
                    {
                        float randomValue = Random.Range(0, totalWeight);
                        float cumulativeWeight = 0f;

                        bool tileSelected = false;
                        foreach (TileGeneration tile in validNodes)
                        {
                            if (tileSelected == false)
                            {
                                cumulativeWeight += tile.weight;
                                if (randomValue <= cumulativeWeight)
                                {
                                    Vector3 spawnPos = chosenTile.position + newDir * (tile.tilePlacementDist + spawnedPathTiles[randomChosenTile].tileGeneration.tilePlacementDist);
                                    Vector3 spawnPosRight = chosenTile.position - newDir * (tile.tilePlacementDist + spawnedPathTiles[randomChosenTile].tileGeneration.tilePlacementDist);
                                    bool LeftAvailable = !Physics.CheckSphere(spawnPos, tile.tilePlacementDist / 1.67f);
                                    bool rightAvailable = !Physics.CheckSphere(spawnPosRight, tile.tilePlacementDist / 1.67f);

                                    if (LeftAvailable && rightAvailable)
                                    {
                                        if (Random.Range(0f, 100f) < 50f)
                                        {
                                            spawnPos = spawnPosRight;
                                            newDir = -newDir;
                                        }
                                    }
                                    else if (rightAvailable)
                                    {
                                        spawnPos = spawnPosRight;
                                        newDir = -newDir;
                                    }

                                    if (LeftAvailable || rightAvailable)
                                    {
                                        GameObject spawnedTileObject = Instantiate(tile.tileObject, spawnPos + new Vector3(0f, (spawnedPathTiles.Count + spawnedSideTiles.Count + 1) * -0.001f, 0f), Quaternion.LookRotation(newDir));
                                        Tile spawnedTile = new Tile();
                                        spawnedTile.tileGeneration = tile;
                                        spawnedTile.tileTransform = spawnedTileObject.transform;
                                        spawnedTile.tileSpawnDir = spawnedTileObject.transform.forward;
                                        spawnedTile.tilePos = spawnedPathTiles[randomChosenTile].tilePos + 1;
                                        spawnedSideTiles.Add(spawnedTile);

                                        sideTilePointsUsed += tile.tilePoints;
                                        lastSelectedTile = tile;
                                        tileSelected = true;
                                    }
                                    else
                                    {
                                        remainingTries--;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    randomChosenTile -= spawnedPathTiles.Count;
                    List<TileGeneration> validNodes = new List<TileGeneration>();
                    foreach (TileGeneration n in generationSideNodes)
                    {
                        if ((sideTilePointsUsed + n.tilePoints) <= maxSideTilePoints) validNodes.Add(n);
                    }

                    float totalWeight = validNodes.Sum(tile => tile.weight);

                    if (totalWeight > 0)
                    {
                        float randomValue = Random.Range(0, totalWeight);
                        float cumulativeWeight = 0f;

                        bool tileSelected = false;
                        foreach (TileGeneration tile in validNodes)
                        {
                            if (tileSelected == false)
                            {
                                cumulativeWeight += tile.weight;
                                if (randomValue <= cumulativeWeight)
                                {
                                    Vector3 spawnPos = spawnedSideTiles[randomChosenTile].tileTransform.position + spawnedSideTiles[randomChosenTile].tileSpawnDir.normalized * (tile.tilePlacementDist + spawnedSideTiles[randomChosenTile].tileGeneration.tilePlacementDist);
                                    bool positionAvailable = !Physics.CheckSphere(spawnPos, tile.tilePlacementDist / 2f);

                                    if (positionAvailable)
                                    {
                                        GameObject spawnedTileObject = Instantiate(tile.tileObject, spawnPos + new Vector3(0f, (spawnedPathTiles.Count + spawnedSideTiles.Count + 1) * -0.001f, 0f), Quaternion.LookRotation(spawnedSideTiles[randomChosenTile].tileSpawnDir.normalized));

                                        Tile spawnedTile = new Tile();
                                        spawnedTile.tileGeneration = tile;
                                        spawnedTile.tileTransform = spawnedTileObject.transform;
                                        spawnedTile.tileSpawnDir = spawnedTileObject.transform.forward;
                                        spawnedTile.tilePos = spawnedSideTiles[randomChosenTile].tilePos + 1;
                                        spawnedSideTiles.Add(spawnedTile);

                                        sideTilePointsUsed += tile.tilePoints;
                                        lastSelectedTile = tile;
                                        tileSelected = true;
                                    }
                                    else
                                    {
                                        tileSelected = true;
                                        remainingTries--;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (remainingTries <= 0)
            {
                GeneratePathNodes();
            }
            else
            {
                CalculateTileDanger();
            }
        }
    }

    void CalculateTileDanger()
    {
        int highestPos = -99999;
        int highestDanger = -99999;
        List<Tile> tiles = new List<Tile>();
        tiles.AddRange(spawnedPathTiles);
        tiles.AddRange(spawnedSideTiles);

        foreach (Tile tile in tiles)
        {
            if (tile.tilePos > highestPos)
            {
                highestPos = tile.tilePos;
            }
        }

        foreach (Tile tile in tiles)
        {
            tile.tileDanger = highestPos - tile.tilePos + 1;
            if (tile.tileDanger > highestDanger)
            {
                highestDanger = tile.tileDanger;
            }
        }

        foreach (Tile tile in tiles)
        {
            tile.tileDangerPerc = (float)tile.tileDanger / (float)highestDanger;
        }

        SetStructureDanger();
        SpawnFolliage();
    }

    void SpawnFolliage()
    {
        /*foreach (GameObject spawn in spawnedFolliage)
        {
            Destroy(spawn);
        }
        spawnedFolliage.Clear();*/
        List<Tile> tiles = new List<Tile>();
        tiles.AddRange(spawnedPathTiles);
        tiles.AddRange(spawnedSideTiles);

        foreach (Tile tile in tiles)
        {
            foreach (FolliageGeneration folliage in tile.tileGeneration.generationFolliages)
            {
                if (Random.Range(0f, 100f) <= folliage.spawnChance)
                {
                    float totalWeight = folliage.folliages.Sum(folliage => folliage.weight);

                    int spawnCount = Random.Range(folliage.minSpawnCount, folliage.maxSpawnCount);
                    int spawnsCompleted = 0;
                    while (spawnsCompleted < spawnCount)
                    {
                        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

                        float randomRadius = Mathf.Sqrt(Random.Range(0f, 1f)) * tile.tileGeneration.tilePlacementDist;

                        Vector3 randomDir = new Vector3(Mathf.Cos(randomAngle), 0f, Mathf.Sin(randomAngle)) * randomRadius;

                        Vector3 spawnPos = tile.tileTransform.position + new Vector3(0f, 100f, 0f) + randomDir;

                        RaycastHit hit;
                        if (Physics.Raycast(spawnPos, Vector3.down, out hit, 105f, groundMask))
                        {
                            float randomValue = Random.Range(0, totalWeight);
                            float cumulativeWeight = 0f;

                            bool folliageSelected = false;
                            foreach (Folliage fol in folliage.folliages)
                            {
                                if (folliageSelected == false)
                                {
                                    cumulativeWeight += fol.weight;
                                    if (randomValue <= cumulativeWeight)
                                    {
                                        GameObject folGameObj = Instantiate(fol.folliageObject, hit.point, Quaternion.AngleAxis(Random.Range(-180f, 180f), Vector3.up));
                                        folGameObj.transform.parent = tile.tileTransform;
                                        //spawnedFolliage.Add(folGameObj);
                                        spawnsCompleted++;
                                        folliageSelected = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        //SpawnStructures();
    }

    void SetStructureDanger()
    {
        List<Tile> tiles = new List<Tile>();
        tiles.AddRange(spawnedPathTiles);
        tiles.AddRange(spawnedSideTiles);
        foreach (Tile tile in tiles)
        {
            /*StructureGeneration[] strucGenerators = tile.tileTransform.GetComponentsInChildren<StructureGeneration>();
            foreach (StructureGeneration spawner in strucGenerators)
            {
                spawner.spawnerDanger = spawnerDanger;
                spawner.dangerBonusHealth = dangerBonusHealth;
                spawner.dangerBonusHealthCap = dangerBonusHealthCap;
                spawner.initialBonusHealth = initialBonusHealth;
            }*/

            StructureGeneration strucGenerator = tile.tileTransform.GetComponent<StructureGeneration>();
            strucGenerator.spawnerDanger = tile.tileDangerPerc;
            strucGenerator.dangerBonusHealth = dangerBonusHealth;
            strucGenerator.dangerBonusHealthCap = dangerBonusHealthCap;
            strucGenerator.initialBonusHealth = initialBonusHealth;
        }

        GenerateMap();
    }

    void GenerateMap()
    {
        foreach (Tile tile in spawnedPathTiles)
        {
            Instantiate(tileOnMap, tile.tileTransform.position + new Vector3(0f, 100f, 0f), Quaternion.Euler(90f, 0f, 0f), tile.tileTransform).transform.localScale = new Vector3(tile.tileGeneration.tilePlacementDist * 2.5f, tile.tileGeneration.tilePlacementDist * 2.5f, 1f);
        }

        foreach (Tile tile in spawnedSideTiles)
        {
            Instantiate(tileOnMap, tile.tileTransform.position + new Vector3(0f, 100f, 0f), Quaternion.Euler(90f, 0f, 0f), tile.tileTransform).transform.localScale = new Vector3(tile.tileGeneration.tilePlacementDist * 2.5f, tile.tileGeneration.tilePlacementDist * 2.5f, 1f);
        }

        Instantiate(tileOnMap, spawnTile.position + new Vector3(0f, 100f, 0f), Quaternion.Euler(90f, 0f, 0f), spawnTile).transform.localScale = new Vector3(spawnTilePlacementDist * 2.5f, spawnTilePlacementDist * 2.5f, 1f);
    }
}
