using UnityEngine;

public class volumeAssigner : MonoBehaviour
{
    [SerializeField] private bool isMusic;

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
        
        InvokeRepeating("CheckVolume", 0.1f, 0.1f);
    }

    void CheckVolume()
    {
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
