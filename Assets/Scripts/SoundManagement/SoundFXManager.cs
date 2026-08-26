using UnityEngine;
using UnityEngine.Audio;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager Instance;
    public AudioMixerGroup mixerGroup;

    private void Awake()
    {
        Instance = this;
    }

    public void PlaySoundClip(AudioClip audioClip, Vector3 soundPos, float volume, float pitch)
    {
        GameObject soundObject = new GameObject();
        soundObject.name = audioClip.name;
        soundObject.transform.position = soundPos;

        AudioSource soundSource = soundObject.AddComponent<AudioSource>();
        soundSource.outputAudioMixerGroup = mixerGroup;
        soundSource.clip = audioClip;
        soundSource.volume = volume;
        soundSource.pitch = pitch;
        soundSource.spatialBlend = 0.25f;
        soundSource.Play();

        Destroy(soundObject, soundSource.clip.length);
    }
}
