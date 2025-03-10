using UnityEngine;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    public Button musicVolumeUpButton;
    public Button musicVolumeDownButton;
    public Button sfxVolumeUpButton;
    public Button sfxVolumeDownButton;
    public float volumeStep = 0.1f; // How much to change volume per button click
    private float musicVolume = 1;
    private float sfxVolume = 1;

    private void Start()
    {
        if (AudioManager.instance != null)
        {
            // Load saved volume levels
            musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1);

            AudioManager.instance.SetMusicVolume(musicVolume);
            AudioManager.instance.SetSFXVolume(sfxVolume);
        }

        // Add listeners for buttons
        musicVolumeUpButton.onClick.AddListener(IncreaseMusicVolume);
        musicVolumeDownButton.onClick.AddListener(DecreaseMusicVolume);
        sfxVolumeUpButton.onClick.AddListener(IncreaseSFXVolume);
        sfxVolumeDownButton.onClick.AddListener(DecreaseSFXVolume);
    }

    public void IncreaseMusicVolume()
    {
        musicVolume = Mathf.Clamp(musicVolume + volumeStep, 0, 1);
        AudioManager.instance.SetMusicVolume(musicVolume);
        SaveVolume("MusicVolume", musicVolume);
    }

    public void DecreaseMusicVolume()
    {
        musicVolume = Mathf.Clamp(musicVolume - volumeStep, 0, 1);
        AudioManager.instance.SetMusicVolume(musicVolume);
        SaveVolume("MusicVolume", musicVolume);
    }

    public void IncreaseSFXVolume()
    {
        sfxVolume = Mathf.Clamp(sfxVolume + volumeStep, 0, 1);
        AudioManager.instance.SetSFXVolume(sfxVolume);
        SaveVolume("SFXVolume", sfxVolume);
    }

    public void DecreaseSFXVolume()
    {
        sfxVolume = Mathf.Clamp(sfxVolume - volumeStep, 0, 1);
        AudioManager.instance.SetSFXVolume(sfxVolume);
        SaveVolume("SFXVolume", sfxVolume);
    }

    private void SaveVolume(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }
}
