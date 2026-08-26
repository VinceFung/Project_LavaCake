using UnityEngine;
using UnityEngine.UI;

public class SliderAlphaController : MonoBehaviour
{
    public Slider slider;
    public Image sliderFill;

    public Color notFilledColor;
    public Color FilledColor;

    void Update()
    {
        if(slider.value >= slider.maxValue)
        {
            sliderFill.color = FilledColor;
        }
        else
        {
            sliderFill.color = notFilledColor;
        }
    }
}
