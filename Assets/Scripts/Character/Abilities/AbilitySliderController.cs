using UnityEngine;
using UnityEngine.UI;

public class AbilitySliderController : MonoBehaviour
{
    public Slider abilitySlider;
    public Image sliderFillImage;
    public Color readyColor;
    public Color notReadyColor;

    public bool playSoundOnReady = true;
    public AudioSource readySound;
    bool canPlaySound = false;

    private void Update()
    {
        if (abilitySlider != null) 
        {
            if (abilitySlider.value >= abilitySlider.maxValue)
            {
                sliderFillImage.color = readyColor;
                if (playSoundOnReady && canPlaySound)
                {
                    if (readySound != null) readySound.Play();
                    canPlaySound = false;
                }
            }
            else
            {
                sliderFillImage.color = notReadyColor;
                canPlaySound = true;
            }
        }
    }
}
