using UnityEngine;

public class AnchorTransform : MonoBehaviour
{
    public Transform AnchorTo;
    public Vector3 Offset;
    public bool InstantAnchor = true;
    public float anchorSpeed;

    void Update()
    {
        if(AnchorTo != null)
        {
            if (InstantAnchor)
            {
                transform.position = AnchorTo.position + Offset;
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, (AnchorTo.position + Offset), Time.deltaTime * anchorSpeed);
            }
        }
    }
}
