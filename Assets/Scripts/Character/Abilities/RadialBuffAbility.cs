using UnityEngine;

public class RadialBuffAbility : MonoBehaviour
{
    public Ability ability;

    public float grantRadius = 7f;
    public LayerMask entityMask;
    public BuffDisplay.DisplayTypes bottemDisplayType;
    public DebuffPreset debuffToApply;
    public ParticleSystem GrantEffect;
    public AudioClip GrantSound;
    public float GrantSoundVolume = 1f;

    public void GrantBuff()
    {
        if(GrantEffect != null)
        {
            GrantEffect.Play();
        }
        if(GrantSound != null)
        {
            SoundFXManager.Instance.PlaySoundClip(GrantSound, ability.Owner.Body.position, GrantSoundVolume, Random.Range(0.95f, 1.05f));
        }

        Collider[] colliders = Physics.OverlapSphere(ability.Owner.Body.position, grantRadius, entityMask);
        foreach (Collider entityCaught in colliders)
        {
            Entity entityScript = entityCaught.GetComponent<Entity>();
            if (entityScript != null)
            {
                if (entityScript.Team == ability.Owner.Team)
                {
                    entityScript.ApplyDebuff(debuffToApply, ability.Owner);
                }
            }
        }
    }
}
