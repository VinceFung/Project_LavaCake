using System.Collections;
using UnityEngine;

public class DeathScreenAnimation : MonoBehaviour
{
    public GameObject[] Flashes;

    [System.Serializable]
    public class DeathTextGroup
    {
        public GameObject[] deathText;
    }
    public DeathTextGroup[] deathTextGroups;

    void Start()
    {
        
    }

    void OnEnable()
    {
        for (int i = 0; i < deathTextGroups.Length; i++)
        {
            foreach (GameObject item in deathTextGroups[i].deathText)
            {
                item.SetActive(false);
            }
        }
        foreach (GameObject item in Flashes)
        {
            item.SetActive(false);
        }

        StartCoroutine(AnimateScreen());
    }

    IEnumerator AnimateScreen()
    {
        for (int i = 0; i < Flashes.Length; i++)
        {
            /*foreach (GameObject item in Flashes)
            {
                item.SetActive(false);
            }*/

            Flashes[i].SetActive(true);
            yield return new WaitForSeconds(0.1f);
        }

        foreach (GameObject item in Flashes)
        {
            item.SetActive(false);
        }

        for (int i = 0; i < deathTextGroups.Length; i++)
        {
            foreach (GameObject item in deathTextGroups[i].deathText)
            {
                item.SetActive(true);
            }

            yield return new WaitForSeconds(0.06f / (i + 1));
        }
    }
}
