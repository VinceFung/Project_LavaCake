using UnityEngine;

public class PlayRandomSound : MonoBehaviour
{
    public AudioClip[] sounds;
    public float volume = 1f;

    private void Start()
    {
        SoundFXManager.Instance.PlaySoundClip(sounds[Random.Range(0, sounds.Length)], transform.position, volume, 1f);
    }
}
