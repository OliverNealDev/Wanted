using UnityEngine;
using UnityEngine.UI;

public class calibrateSoundSliders : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    
    void Start()
    {
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");;
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume");
    }
}
