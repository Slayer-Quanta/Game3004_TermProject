using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public Slider musicVolumeSlider;

    private void Start()
    {
        SoundManager.self.PlayMenuMusic();
        
        musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1);
        SliderValueChange(musicVolumeSlider.value);
    }

    public void SliderValueChange(float value)
    {
        SoundManager.self.ChangeVolume(value);
    }

    public void Btn_Play()
    {
        SoundManager.self.PlayGamePlayMusic();
    }
}
