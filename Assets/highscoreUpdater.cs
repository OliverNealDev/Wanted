using TMPro;
using UnityEngine;

public class HighscoreUpdater : MonoBehaviour
{
    private TextMeshProUGUI highscoreText;

    void Start()
    {
        highscoreText = GetComponent<TextMeshProUGUI>();
        if (highscoreText == null)
        {
            Debug.LogError("TextMeshProUGUI component not found on this GameObject.");
            return;
        }

        float totalSeconds = PlayerPrefs.GetFloat("Highscore");

        int minutes = Mathf.FloorToInt(totalSeconds / 60f);
        int seconds = Mathf.FloorToInt(totalSeconds % 60f);

        highscoreText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}