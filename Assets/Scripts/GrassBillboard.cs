using UnityEngine;

public class GrassBillboard : MonoBehaviour
{
    public LODGroup lod;
    public Transform[] grassTransforms;

    void Start()
    {
        foreach (Transform t in grassTransforms)
        {
            t.rotation = Quaternion.Euler(37.5f, 0, 0);
        }
    }
}
