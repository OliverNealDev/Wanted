using UnityEngine;
using UnityEngine.UI;

public class audioLevelsManager : MonoBehaviour
{
    public static audioLevelsManager Instance;
    
    public float musicVolume = 1f;
    public float sfxVolume = 1f;
    
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    
    void Awake()
    {
        //Object.DontDestroyOnLoad(gameObject);
        Instance = this;
        
        PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
    }
    
    public void SetMusicVolume()
    {
        musicVolume = musicVolumeSlider.value;
        PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
        // Here you would typically set the volume on your audio source
        // For example: AudioManager.Instance.SetMusicVolume(volume);
    }
    
    public void SetSFXVolume()
    {
        sfxVolume = sfxVolumeSlider.value;
        PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
        // Here you would typically set the volume on your audio source
        // For example: AudioManager.Instance.SetSFXVolume(volume);
    }
}
