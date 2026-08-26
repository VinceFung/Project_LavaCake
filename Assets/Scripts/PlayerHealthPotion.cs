using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealthPotion : MonoBehaviour
{
    public Entity entity;

    public int PotionCount = 3;
    public int PotionMax = 3;
    public int HealAmount = 150;

    public UnityEvent OnHeal;

    private void Update()
    {
        UnitManager.Instance.playerHealthPotionCount.text = $"{PotionCount}/{PotionMax}";   
    }

    public void Heal()
    {
        PotionCount--;
        entity.Health += HealAmount;
        OnHeal.Invoke();

        entity.OnAbilityCast.Invoke();
    }
}
