using Helper.Tween;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager self;

    public AudioSource audioSource;
    
    public float fadeDuration = 1;
    public Sounds sounds;

    private void Awake()
    {
        if (self == null)
        {
            self = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void PlayMenuMusic()
    {
        PlayMusic(sounds.menuMusic);
    }

    public void PlayGamePlayMusic()
    {
        PlayMusic(sounds.gamePlay);
    }

    public void PlayPauseMenuMusic()
    {
        PlayMusic(sounds.pauseMenu);
    }

    public void PlayEnemyChaseMusic()
    {
        PlayMusic(sounds.enemyChase);
    }

    public void PlayDeathMusic()
    {
        audioSource.DoFade(0, fadeDuration);
    }

    public void ChangeVolume(float volume)
    {
        audioSource.volume = volume;
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    void PlayMusic(Sound sound)
    {
        audioSource.DoFade(0, fadeDuration).OnComplete(() =>
        {
            audioSource.clip = sound.clip;
            audioSource.Play();
            audioSource.DoFade(sound.volume, fadeDuration);
        });
    }

    [System.Serializable]
    public class Sounds
    {
        public Sound menuMusic;
        public Sound gamePlay;
        public Sound pauseMenu;
        public Sound enemyChase;
    }

    [System.Serializable]
    public class Sound
    {
        public AudioClip clip;
        [Range(0, 1)] public float volume = 1;
    }
}
