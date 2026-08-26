using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class LootPool : ScriptableObject
{
    [System.Serializable]
    public class WeightedPool
    {
        public float Weight = 100f;
        public Item[] pool;
    }
    public WeightedPool[] WeightedPools;

    public Item GetItem()
    {
        Item returnItem = null;
        float totalWeight = 0f;
        foreach (var wPool in WeightedPools)
        {
            totalWeight += wPool.Weight;
        }

        if (totalWeight > 0)
        {
            float randomValue = Random.Range(0, totalWeight);
            float cumulativeWeight = 0f;

            foreach (WeightedPool weightedPool in WeightedPools)
            {
                cumulativeWeight += weightedPool.Weight;
                if (randomValue <= cumulativeWeight)
                {
                    returnItem = weightedPool.pool[Random.Range(0, weightedPool.pool.Length)];
                    break;
                }
            }
        }

        return returnItem;
    }
}
