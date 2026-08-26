using UnityEngine;

public class ParticleSystemPlayer : MonoBehaviour
{
    [System.Serializable]
    public class PlayableParticle
    {
        public string ParticleCallID;
        public ParticleSystem[] Particles;
        public ParticleSystem[] StopParticles;
        public float audioVolume = 1f;
        public AudioClip[] PlayAudio;
    }
    public PlayableParticle[] PlayParticles;

    public void PlayParticle(string id)
    {
        foreach (PlayableParticle item in PlayParticles)
        {
            if(item.ParticleCallID == id)
            {
                foreach (ParticleSystem particle in item.Particles)
                {
                    particle.Play();
                }
            }
        }

        foreach (PlayableParticle item in PlayParticles)
        {
            if (item.ParticleCallID == id)
            {
                foreach (ParticleSystem particle in item.StopParticles)
                {
                    particle.Stop();
                }
            }
        }

        foreach (PlayableParticle item in PlayParticles)
        {
            if (item.ParticleCallID == id)
            {
                foreach (AudioClip clip in item.PlayAudio)
                {
                    SoundFXManager.Instance.PlaySoundClip(clip, transform.position, item.audioVolume, Random.Range(0.95f, 1.05f));
                }
            }
        }
    }
}
