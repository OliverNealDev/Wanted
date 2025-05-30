using UnityEngine;

public class volumeAssigner : MonoBehaviour
{
    [SerializeField] private bool isMusic;
    [SerializeField] private bool updateFrequently = true;

    void Start()
    {
        if(GetComponent<AudioSource>() != null)
        {
            if (isMusic)
            {
                GetComponent<AudioSource>().volume *= PlayerPrefs.GetFloat("MusicVolume");
            }
            else
            {
                GetComponent<AudioSource>().volume *= PlayerPrefs.GetFloat("SFXVolume");
            }
        }
        
        InvokeRepeating("CheckVolume", 0.025f, 0.025f);
    }

    void CheckVolume()
    {
        if (!updateFrequently) return;
        
        if (GetComponent<AudioSource>() != null)
        {
            if (isMusic)
            {
                GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("MusicVolume");
            }
            else
            {
                GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("SFXVolume");
            }
        }
    }
}
