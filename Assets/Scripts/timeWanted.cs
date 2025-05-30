using UnityEngine;
using TMPro;

public class TimeWanted : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    private float elapsedTime = 0f;

    void Start()
    {
        if (timerText == null)
        {
            Debug.LogError("Timer Text (TextMeshProUGUI) not assigned in the Inspector!");
            enabled = false;
            return;
        }
        UpdateTimerDisplay(0);
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        UpdateTimerDisplay(elapsedTime);
    }

    void UpdateTimerDisplay(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}