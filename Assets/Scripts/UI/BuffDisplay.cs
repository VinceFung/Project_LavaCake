using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuffDisplay : MonoBehaviour
{
    public TextMeshProUGUI TopText;
    public GameObject topTextBG;
    public TextMeshProUGUI BottemText;
    public GameObject bottemTextBG;
    public Image BuffIconImage;

    public Debuff debuff;

    public enum DisplayTypes
    {
        None, Duration, DamageBonus, DamageInputBonus, AttackSpeedBonus, MovementSpeedBonus, GunChargeBonus, CustomA, CustomB
    }
    public DisplayTypes TopDisplay = DisplayTypes.None;
    public DisplayTypes BottemDisplay = DisplayTypes.None;

    public string customDisplayA;
    public string customDisplayB;

    public void SetDebuff(Debuff debuff)
    {
        this.debuff = debuff;
        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        if (debuff == null) return;

        SetDisplay(debuff.topDisplay, TopText, topTextBG);
        SetDisplay(debuff.bottomDisplay, BottemText, bottemTextBG);
        BuffIconImage.sprite = debuff.instancePreset.icon;
    }

    private void Update()
    {
        if (debuff.instancePreset == null)
        {
            SetDisplay(TopDisplay, TopText, topTextBG);
            SetDisplay(BottemDisplay, BottemText, bottemTextBG);
        }
    }

    void SetDisplay(DisplayTypes displayType, TextMeshProUGUI tmproTex, GameObject texBG)
    {
        switch (displayType)
        {
            case DisplayTypes.None:
                texBG.gameObject.SetActive(false);
                tmproTex.text = "";
                break;
            case DisplayTypes.Duration:
                texBG.gameObject.SetActive(true);
                tmproTex.text = debuff.Duration.ToString("0.0");
                break;
            case DisplayTypes.DamageBonus:
                texBG.gameObject.SetActive(true);
                tmproTex.text = $"{debuff.debuffDamageMultiplier * 100f}%";
                break;
            case DisplayTypes.DamageInputBonus:
                texBG.gameObject.SetActive(true);
                tmproTex.text = $"{debuff.debuffDamageInputMultiplier * 100f}%";
                break;
            case DisplayTypes.AttackSpeedBonus:
                texBG.gameObject.SetActive(true);
                tmproTex.text = $"{debuff.debuffAttackSpeedMultiplier * 100f}%";
                break;
            case DisplayTypes.MovementSpeedBonus:
                texBG.gameObject.SetActive(true);
                tmproTex.text = $"{debuff.debuffMovementSpeedMultiplier * 100f}%";
                break;
            case DisplayTypes.GunChargeBonus:
                texBG.gameObject.SetActive(true);
                tmproTex.text = $"{debuff.debuffGunChargeMultiplier * 100f}%";
                break;
            case DisplayTypes.CustomA:
                texBG.gameObject.SetActive(true);
                tmproTex.text = customDisplayA;
                break;
            case DisplayTypes.CustomB:
                texBG.gameObject.SetActive(true);
                tmproTex.text = customDisplayB;
                break;
        }
    }
}