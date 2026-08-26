using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundMixerManager : MonoBehaviour
{
    public AudioMixer audioMixer;

    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider soundFXVolumeSlider;

    public TextMeshProUGUI masterVolumeText;
    public TextMeshProUGUI musicVolumeText;
    public TextMeshProUGUI soundFXVolumeText;

    public void SetMasterVolume(float level)
    {
        audioMixer.SetFloat("masterVolume", Mathf.Log10(level) * 40);
    }

    public void SetMusicVolume(float level)
    {
        audioMixer.SetFloat("musicVolume", Mathf.Log10(level) * 40);
    }

    public void SetSoundFXVolume(float level)
    {
        audioMixer.SetFloat("soundFXVolume", Mathf.Log10(level) * 40);
    }

    private void OnEnable()
    {
        if (masterVolumeSlider != null)
        {
            float masterVolume;
            audioMixer.GetFloat("masterVolume", out masterVolume);
            masterVolumeSlider.value = Mathf.Pow(10, masterVolume / 40);
        }
        if (musicVolumeSlider != null)
        {
            float musicVolume;
            audioMixer.GetFloat("musicVolume", out musicVolume);
            musicVolumeSlider.value = Mathf.Pow(10, musicVolume / 40);
        }
        if (soundFXVolumeSlider != null)
        {
            float soundFXVolume;
            audioMixer.GetFloat("soundFXVolume", out soundFXVolume);
            soundFXVolumeSlider.value = Mathf.Pow(10, soundFXVolume / 40);
        }
    }

    private void Update()
    {
        if (masterVolumeText != null)
        {
            float masterVolume;
            audioMixer.GetFloat("masterVolume", out masterVolume);
            masterVolumeText.text = Mathf.Pow(10, masterVolume / 40).ToString("F2");
        }

        if (musicVolumeText != null)
        {
            float musicVolume;
            audioMixer.GetFloat("musicVolume", out musicVolume);
            musicVolumeText.text =  Mathf.Pow(10, musicVolume / 40).ToString("F2");
        }

        if (soundFXVolumeText != null)
        {
            float soundFXVolume;
            audioMixer.GetFloat("soundFXVolume", out soundFXVolume);
            soundFXVolumeText.text = Mathf.Pow(10, soundFXVolume / 40).ToString("F2");
        }
    }
}
