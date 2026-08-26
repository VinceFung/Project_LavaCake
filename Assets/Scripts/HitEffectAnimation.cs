using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HitEffectAnimation : MonoBehaviour
{
    public GameObject effectHolder;
    public Image HudEffectImage;
    public float hudEffectStartAlpha = 0.5f;
    public float hudEffectEndAlpha = 0f;
    public float textStartPos = 750f;
    public float textEndPos = 1250f;
    public float defaultFontSize = 144f;
    public Transform rightTextHolder;
    public Transform leftTextHolder;

    public int totalFrames = 6;
    public float timeBtwFrames = 0.1f;

    public string[] textStrings;
    public TextMeshProUGUI[] effectTexts;

    private void Update()
    {
    }

    public void PlayAnimation()
    {
        StartCoroutine(AnimateHit());
    }

    IEnumerator AnimateHit()
    {
        effectHolder.SetActive(true);
        rightTextHolder.localPosition = new Vector3(textStartPos, rightTextHolder.localPosition.y, rightTextHolder.localPosition.z);
        leftTextHolder.localPosition = new Vector3(-textStartPos, leftTextHolder.localPosition.y, leftTextHolder.localPosition.z);

        for (int i = 0; i < effectTexts.Length; i++)
        {
            effectTexts[i].fontSize = defaultFontSize;
            Color newTextColor = HudEffectImage.color;
            newTextColor.a = 1f;
            effectTexts[i].color = newTextColor;
        }

        Color hudEffectColor = HudEffectImage.color;
        hudEffectColor.a = hudEffectStartAlpha;
        HudEffectImage.color = hudEffectColor;

        for (int i = 0; i < totalFrames; i++)
        {
            for (int j = 0; j < effectTexts.Length; j++)
            {
                Color newTextColor = HudEffectImage.color;
                newTextColor.a = newTextColor.a * Random.Range(0.75f, 0.95f);
                effectTexts[j].color = newTextColor;
                effectTexts[j].text = textStrings[Random.Range(0, textStrings.Length)];
                effectTexts[j].fontSize = effectTexts[j].fontSize * Random.Range(0.75f, 1.2f);
            }
            float posChange = i * (textEndPos - textStartPos) / totalFrames;
            rightTextHolder.localPosition = new Vector3(textStartPos + posChange, rightTextHolder.localPosition.y, rightTextHolder.localPosition.z);
            leftTextHolder.localPosition = new Vector3(-textStartPos - posChange, leftTextHolder.localPosition.y, leftTextHolder.localPosition.z);

            hudEffectColor.a = Mathf.Lerp(hudEffectStartAlpha, hudEffectEndAlpha, (float)i / totalFrames);
            HudEffectImage.color = hudEffectColor;
            yield return new WaitForSeconds(timeBtwFrames);
        }
        effectHolder.SetActive(false);
    }
}
