using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public Sound[] musicSounds, sfxSounds;
    public AudioSource musicSource, sfxSource;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayMusic("Menu Background Music"); 
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Loaded Scene: " + scene.name); 
        musicSource.Stop();

        switch (scene.name)
        {
            case "MainMenu":
                PlayMusic("Menu Background Music");
                break;
            case "Achievements":
                PlayMusic("Achievements Background Music");
                break;
            case "GameOverScene":
                PlayMusic("GameOver Background Music");
                break;
            case "GameWinScene":
                PlayMusic("GameWin Background Music");
                break;
            case "InstructionsScene":
                PlayMusic("Instructions Background Music");
                break;
            case "OptionsScene":
                PlayMusic("Options Background Music");
                break;
            case "SampleScene":
                PlayMusic("Sample Background Music");
                break;
            default:
                // Optional: Handle unexpected cases or stop music
                break;
        }
    }

    public void PlayMusic(string name)
    {
        Sound s = Array.Find(musicSounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning($"Sound: {name} not found!");
            return;
        }
        musicSource.clip = s.clip;
        musicSource.Play();
    }

    public void PlaySFX(string name)
    {
        Sound s = Array.Find(sfxSounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning($"Sound: {name} not found!");
            return;
        }
        sfxSource.clip = s.clip;
        sfxSource.Play();
    }

    public void ToggleMusic()
    {
        musicSource.mute = !musicSource.mute;
    }

    public void ToggleSFX()
    {
        sfxSource.mute = !sfxSource.mute;
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }

    // Specific sound effect methods
    public void PlayButtonClick()
    {
        PlaySFX("Button Click");
    }

    public void PlayJumpSound()
    {
        PlaySFX("Jump");
    }

    public void PlayWalkSound()
    {
        PlaySFX("Walk");
    }

    public void PlayAttackSound()
    {
        PlaySFX("Attack");
    }

    public void PlayPickupSound()
    {
        PlaySFX("Pickup");
    }
}
