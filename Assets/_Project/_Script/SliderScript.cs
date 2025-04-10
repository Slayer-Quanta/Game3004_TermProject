using UnityEngine;
using UnityEngine.UI;

public class SliderScript : MonoBehaviour
{
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    private void Start()
    {
        if (AudioManager.instance != null)
        {
            // Load saved volume levels
            float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1);
            float savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1);

            musicVolumeSlider.value = savedMusicVolume;
            sfxVolumeSlider.value = savedSFXVolume;

            AudioManager.instance.SetMusicVolume(savedMusicVolume);
            AudioManager.instance.SetSFXVolume(savedSFXVolume);
        }

        // Add listeners to detect when sliders are moved
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMusicVolume(float value)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetMusicVolume(value);
            PlayerPrefs.SetFloat("MusicVolume", value);
            PlayerPrefs.Save();
        }
    }

    public void SetSFXVolume(float value)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetSFXVolume(value);
            PlayerPrefs.SetFloat("SFXVolume", value);
            PlayerPrefs.Save();
        }
    }
}
