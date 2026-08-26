using System.Linq;
using UnityEngine;

public class StructureGeneration : MonoBehaviour
{
    public float spawnerDanger = 0f;
    public float dangerBonusHealth = 2.5f;
    public float dangerBonusHealthCap = 2.5f;
    public float initialBonusHealth = 0f;

    [System.Serializable]
    public class StructureSpawn
    {
        public Transform SpawnPoint;
        public GameObject[] StructuresToSpawn;
    }

    public StructureSpawn[] StructureSpawns;

    private bool hasGenerated = false;

    public void Generate()
    {
        if (hasGenerated) return;
        
        foreach (StructureSpawn strucSpawn in StructureSpawns)
        {
            if (strucSpawn.StructuresToSpawn.Length > 0 && strucSpawn.SpawnPoint != null)
            {
                GameObject instantiatedStruc = Instantiate(strucSpawn.StructuresToSpawn[Random.Range(0, strucSpawn.StructuresToSpawn.Length)], strucSpawn.SpawnPoint.position, strucSpawn.SpawnPoint.rotation);
                instantiatedStruc.transform.parent = this.transform;
            }
        }

        hasGenerated = true;
    }

    public void InitializeStructure()
    {
        Generate();
    }
}
