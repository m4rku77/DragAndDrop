using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WinManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject winPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI timeText;
    public Image[] stars;
    public Button restartButton;
    public Button menuButton;

    [Header("Star Time Thresholds (seconds)")]
    public float threeStarTime = 30f;
    public float twoStarTime = 60f;

    [Header("All Vehicle Tags")]
    public string[] carTags = {
        "Garbage", "Medicine", "Fire", "b2", "e61", "e46",
        "eskavators", "tractor1", "tractor2", "police", "cement", "bus"
    };

    private float timeElapsed;
    private bool gameEnded = false;
    private const int maxCount = 12; // ✅ Count only 12 vehicles

    void Start()
    {
        if (winPanel != null)
            winPanel.SetActive(false);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (menuButton != null)
            menuButton.onClick.AddListener(GoToMenu);

        if (titleText != null) titleText.text = "";
        if (timeText != null) timeText.text = "";
    }

    void Update()
    {
        if (gameEnded) return;

        timeElapsed += Time.deltaTime;

        // 🧩 DEBUG: Press "P" to instantly win (auto place all cars)
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("✅ DEBUG: Auto-placing all cars...");
            AutoPlaceAllCars();
            StartCoroutine(AutoWinAfterDelay(0.5f));
            gameEnded = true; // 🛑 stops further checks so it won't recalculate wrong
            return;
        }

        // ✅ Check win condition only if P not pressed
        if (AllCarsProcessed())
        {
            gameEnded = true;
            StartCoroutine(ShowWinWithDelay(1f));
        }
    }


    private bool AllCarsProcessed()
    {
        foreach (string tag in carTags)
        {
            GameObject[] cars = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject car in cars)
            {
                if (car == null || !car.activeInHierarchy) continue;

                var drag = car.GetComponent<DragAndDropScript>();
                if (drag != null && drag.enabled)
                    return false;
            }
        }
        return true;
    }

    private System.Collections.IEnumerator ShowWinWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        (int starsEarned, int placedCars, int totalCars) = CalculateStars();
        ShowWinScreen(starsEarned, placedCars, totalCars);
    }

    private (int starsEarned, int placedCars, int totalCars) CalculateStars()
    {
        int totalCars = 0;
        int placedCars = 0;

        foreach (string tag in carTags)
        {
            if (totalCars >= maxCount) break;

            GameObject[] cars = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject car in cars)
            {
                if (totalCars >= maxCount) break;
                totalCars++;

                if (car == null) continue;

                var drag = car.GetComponent<DragAndDropScript>();

                // ✅ Count as placed only if drag is disabled but car is still active
                if (drag != null && !drag.enabled && car.activeInHierarchy)
                {
                    placedCars++;
                }
            }
        }

        if (totalCars == 0) return (0, 0, 0);

        float completion = (float)placedCars / totalCars;
        Debug.Log($"Completion: {completion:P0} ({placedCars}/{totalCars})");

        int starsEarned = 0;

        if (completion >= 0.9f && timeElapsed <= threeStarTime)
            starsEarned = 3;
        else if (completion >= 0.75f && timeElapsed <= twoStarTime)
            starsEarned = 2;
        else if (completion >= 0.5f)
            starsEarned = 1;

        return (starsEarned, placedCars, totalCars);
    }

    private void AutoPlaceAllCars()
    {
        foreach (string tag in carTags)
        {
            GameObject[] cars = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject car in cars)
            {
                DragAndDropScript drag = car.GetComponent<DragAndDropScript>();
                if (drag == null) continue;

                DropPlaceScript[] places = FindObjectsByType<DropPlaceScript>(FindObjectsSortMode.None);
                foreach (DropPlaceScript place in places)
                {
                    if (place.gameObject.CompareTag(tag))
                    {
                        RectTransform carRect = car.GetComponent<RectTransform>();
                        RectTransform placeRect = place.GetComponent<RectTransform>();
                        if (carRect != null && placeRect != null)
                        {
                            carRect.anchoredPosition = placeRect.anchoredPosition;
                            carRect.localRotation = placeRect.localRotation;
                            carRect.localScale = placeRect.localScale;
                        }
                        drag.enabled = false;
                        break;
                    }
                }
            }
        }
    }

    private System.Collections.IEnumerator AutoWinAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowWinScreen(3, maxCount, maxCount);
        gameEnded = true;
        Debug.Log("✅ Auto-win triggered successfully!");
    }

    public void ShowWinScreen(int starsEarned, int placedCars, int totalCars)
    {
        if (winPanel != null)
            winPanel.SetActive(true);

        if (titleText != null)
        {
            titleText.gameObject.SetActive(true);

            if (placedCars >= 6)
                titleText.text = "Līmenis pabeigts!";
            else
                titleText.text = "Nekas, mēģini vēlreiz!";
        }

        if (timeText != null)
        {
            timeText.gameObject.SetActive(true);
            timeText.text = $"Tu novietoji {placedCars} no {maxCount} transportlīdzekļiem!\n" +
                            $"Laiks: {timeElapsed:F2} s";
        }

        // ✅ Show stars only if at least 6 cars placed
        for (int i = 0; i < stars.Length; i++)
            stars[i].enabled = (placedCars >= 6 && i < starsEarned);
    }

    public void RestartGame() =>
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    public void GoToMenu() =>
        SceneManager.LoadScene("SakumsScene");
}
