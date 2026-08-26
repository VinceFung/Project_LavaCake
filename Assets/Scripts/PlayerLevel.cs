using UnityEngine;
using UnityEngine.UI;

public class PlayerLevel : MonoBehaviour
{
    public int Level = 0;
    public float currentXp = 0f;
    public float currentXpRequirement = 100f;
    public float baseXpRequirement = 100f;
    public float xpRequirementScaler = 1.2f;

    public float xpGainMultiplier = 1f;

    public Slider XpSlider;
    public Slider DelayedXpSlider;

    public void GainXp(float amount)
    {
        currentXp += amount * xpGainMultiplier;
        while (currentXp >= currentXpRequirement)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        Level++;
        currentXp -= currentXpRequirement;
        currentXpRequirement = baseXpRequirement * Mathf.Pow(xpRequirementScaler, Level);
    }

    void Update()
    {
        if (XpSlider != null)
        {
            XpSlider.value = currentXp;
            XpSlider.maxValue = currentXpRequirement;
        }

        if (DelayedXpSlider != null)
        {
            DelayedXpSlider.maxValue = currentXpRequirement;
            if (currentXp > DelayedXpSlider.value)
            {
                DelayedXpSlider.value += Time.deltaTime * currentXpRequirement / 1.5f;
            }
            else
            {
                DelayedXpSlider.value = currentXp;
            }
        }
    }
}
