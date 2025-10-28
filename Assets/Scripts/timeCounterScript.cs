using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; 

public class CityTimerManager : MonoBehaviour
{
    public TMP_Text timerText; 

    private float playTime = 0f;
    private bool isTracking = false;

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "CityScene")
        {
            isTracking = true;
        }
    }

    void Update()
    {
        if (isTracking)
        {
            playTime += Time.deltaTime;

            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(playTime / 60);
                int seconds = Mathf.FloorToInt(playTime % 60);
                timerText.text = $"Time: {minutes:00}:{seconds:00}";
            }
        }
    }

    public float GetPlayTime()
    {
        return playTime;
    }
}
