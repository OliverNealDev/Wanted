using UnityEngine;

public class AspectAndResolutionManager : MonoBehaviour
{
    public static AspectAndResolutionManager Instance { get; private set; }

    private float targetAspect = 16.0f / 9.0f;
    private Camera mainCamera;

    private bool setInitialAudio = false;

    void Awake()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            Destroy(gameObject);
            return;
        }

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return; 
        }

        mainCamera = FindAnyObjectByType<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError("AspectAndResolutionManager: Main Camera not found on Awake!");
        }
        
        if (!setInitialAudio)
        {
            PlayerPrefs.SetFloat("MusicVolume", 0.125f);
            PlayerPrefs.SetFloat("SFXVolume", 0.125f);
            setInitialAudio = true;
        }
    }

    void Start()
    {
        if (Instance != this) return;
        if (Application.platform == RuntimePlatform.WebGLPlayer) return;
        
        SetInitialResolution(false); 

        if (mainCamera != null)
        {
            AdjustAspect();
        }
    }

    void SetInitialResolution(bool fullscreen)
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer) return;

        int targetWidth, targetHeight;

        if (Display.main.systemWidth <= 1920) 
        {
            targetWidth = 1280;
            targetHeight = 720;
        }
        else 
        {
            targetWidth = 1920;
            targetHeight = 1080;
        }

        if (!fullscreen) {
            targetWidth = Mathf.Min(targetWidth, Display.main.systemWidth);
            targetHeight = Mathf.Min(targetHeight, Display.main.systemHeight);
        }

        Screen.SetResolution(targetWidth, targetHeight, fullscreen);
    }


    void Update()
    {
        if (Instance != this && Application.platform != RuntimePlatform.WebGLPlayer) return;
        if (Application.platform == RuntimePlatform.WebGLPlayer) return;

        if (mainCamera == null)
        {
            mainCamera = FindAnyObjectByType<Camera>();
            if (mainCamera == null)
            {
                return;
            }
            AdjustAspect();
        }


        if (Input.GetKeyDown(KeyCode.F11))
        {
            Screen.fullScreen = !Screen.fullScreen;

            if (Screen.fullScreen)
            {
                int targetWidth, targetHeight;
                if (Display.main.systemWidth <= 1920)
                {
                    targetWidth = 1280;
                    targetHeight = 720;
                }
                else 
                {
                    targetWidth = 1920;
                    targetHeight = 1080;
                }
                Screen.SetResolution(targetWidth, targetHeight, true);
            }
            else
            {
                SetInitialResolution(false);
            }
        }

        if (mainCamera != null) 
        {
            AdjustAspect();
        }
    }

    void AdjustAspect()
    {
        if (mainCamera == null)
        {
            return;
        }

        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;
        Rect rect = mainCamera.rect;

        if (scaleHeight < 1.0f) 
        {
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
        }
        else 
        {
            float scaleWidth = 1.0f / scaleHeight;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
        }

        if (mainCamera.rect != rect) 
        {
            mainCamera.rect = rect;
        }
    }

    public void UpdateMainCameraReference()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            AdjustAspect();
        }
        else
        {
            Debug.LogError("AspectAndResolutionManager: Main Camera not found when trying to update reference!");
        }
    }
}
