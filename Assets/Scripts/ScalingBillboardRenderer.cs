using UnityEngine;

public class ScalingBillboardRenderer : MonoBehaviour
{
    Transform camTrans;

    void Start()
    {
        camTrans = UnitManager.Instance.mainCamera.transform;
    }

    void Update()
    {
        transform.LookAt(camTrans.position);
    }
}
