using UnityEngine;

public class SpriteColorChanger : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Color newColor;

    bool colorChanged = false;
    float maxTicks = 12f;
    float tick;

    private void Update()
    {
        if (!colorChanged)
        {
            if (tick >= maxTicks)
            {
                tick = 0f;

                if(UnitManager.Instance.playerObj != null)
                {
                    Vector3 mapPosition = transform.position + new Vector3(0, -95f, 0);
                    Vector3 playerPosition = UnitManager.Instance.playerObj.transform.position;
                    
                    // Check if player is within rectangular bounds
                    Vector3 mapScale = transform.localScale;
                    float halfWidth = mapScale.x * 0.5f;
                    float halfDepth = mapScale.y * 0.5f; // Using Y as depth since map is rotated 90 degrees
                    
                    float deltaX = Mathf.Abs(playerPosition.x - mapPosition.x);
                    float deltaZ = Mathf.Abs(playerPosition.z - mapPosition.z);
                    
                    if (deltaX <= halfWidth && deltaZ <= halfDepth)
                    {
                        ChangeColor();
                        colorChanged = true;
                    }
                }
            }
            else
            {
                tick++;
            }
        }
    }

    public void ChangeColor()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = newColor;
        }
        else
        {
            Debug.LogWarning("SpriteRenderer is not assigned.");
        }
    }
}
