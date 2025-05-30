using UnityEngine;

public class playButton : MonoBehaviour
{
    public void onPlayButtonClick()
    {
        // Load the game scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }
}
