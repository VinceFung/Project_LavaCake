using UnityEngine;

[System.Serializable]
public class DamageInstance
{
    public enum DamageTypes
    {
        DirectDamage, StatusDamage
    }
    public DamageTypes DamageType = DamageTypes.DirectDamage;
    public float HealthDamage;
    public float StaggerDamage;
    public float SeverenceDamage;
    public float knockbackAmount;
    public Vector3 knockbackDir;
    public float FinalSeverenceDamageMultiplier = 1f;
    public float Multiplier = 1f;
    public float FriendlyFire;
    public float gunChargeMultiplier = 1f;

    public DamageInstance(DamageInstance other)
    {
        if (other != null)
        {
            this.DamageType = other.DamageType;
            this.HealthDamage = other.HealthDamage;
            this.StaggerDamage = other.StaggerDamage;
            this.SeverenceDamage = other.SeverenceDamage;
            this.knockbackAmount = other.knockbackAmount;
            this.knockbackDir = other.knockbackDir;
            this.FinalSeverenceDamageMultiplier = other.FinalSeverenceDamageMultiplier;
            this.Multiplier = other.Multiplier;
            this.FriendlyFire = other.FriendlyFire;
            this.gunChargeMultiplier = other.gunChargeMultiplier;
        }
    }
}
