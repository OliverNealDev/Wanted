using UnityEngine;

public class AspectAndResolutionManager : MonoBehaviour
{
    private float targetAspect = 16.0f / 9.0f;
    private Camera mainCamera;

    void Start()
    {
        // If running as a WebGL build, do nothing.
        if (Application.platform == RuntimePlatform.WebGLPlayer)
            return;

        mainCamera = Camera.main;

        Screen.fullScreen = !Screen.fullScreen;
        if (Display.main.systemWidth <= 1920)
        {
            Screen.SetResolution(1280, 720, false);
        }
        else if (Display.main.systemWidth > 1920)
        {
            Screen.SetResolution(1920, 1080, false);
        }
        else
        {
            Screen.SetResolution(1280, 720, false);
        }

        // Optionally call AdjustAspect() here if needed.
        // AdjustAspect();
    }

    void Update()
    {
        // If running as a WebGL build, do nothing.
        if (Application.platform == RuntimePlatform.WebGLPlayer)
            return;

        if (Input.GetKeyDown(KeyCode.F11))
        {
            Screen.fullScreen = !Screen.fullScreen;
            if (Screen.fullScreen)
            {
                if (Display.main.systemWidth <= 1920)
                {
                    Screen.SetResolution(1280, 720, false);
                }
                else if (Display.main.systemWidth > 1920)
                {
                    Screen.SetResolution(1920, 1080, false);
                }
                else
                {
                    Screen.SetResolution(1280, 720, false);
                }
            }
            else
            {
                int nativeWidth = Display.main.systemWidth;
                int nativeHeight = Display.main.systemHeight;
                Screen.SetResolution(nativeWidth, nativeHeight, true);
            }
        }

        AdjustAspect();
    }

    void AdjustAspect()
    {
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

        mainCamera.rect = rect;
    }
}
