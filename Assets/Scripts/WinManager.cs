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
        var allDrags = FindObjectsByType<DragAndDropScript>(FindObjectsSortMode.None);

        int total = 0;
        int remaining = 0;

        foreach (var d in allDrags)
        {
            if (total >= maxCount) break;
            if (d == null) continue;

            total++;

            // still not placed if drag script is enabled
            if (d.enabled && d.gameObject.activeInHierarchy)
                remaining++;
        }

        Debug.Log($"Draggables processed: {total - remaining}/{total} (remaining: {remaining})");

        //  consider complete when no draggables are left active/enabled
        return remaining == 0;
    }



    private System.Collections.IEnumerator ShowWinWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        (int starsEarned, int placedCars, int totalCars) = CalculateStars();
        ShowWinScreen(starsEarned, placedCars, totalCars);
    }

    private (int starsEarned, int placedCars, int totalCars) CalculateStars()
    {
        var allDrags = FindObjectsByType<DragAndDropScript>(FindObjectsSortMode.None);

        int total = 0;
        int placed = 0;

        foreach (var d in allDrags)
        {
            if (total >= maxCount) break;
            if (d == null) continue;

            if (!d.gameObject.activeInHierarchy)
            {
                // destroyed → does NOT count as placed
                total++;
                continue;
            }

            total++;

            // placed only if active AND drag is disabled
            if (!d.enabled) placed++;
        }

        if (total == 0) return (0, 0, 0);

        float completion = (float)placed / total;
        int stars = 0;
        if (completion >= 0.9f && timeElapsed <= threeStarTime) stars = 3;
        else if (completion >= 0.75f && timeElapsed <= twoStarTime) stars = 2;
        else if (completion >= 0.5f) stars = 1;

        Debug.Log($"Stars calc: placed {placed}/{total} (completion {completion:P0})");
        return (stars, placed, total);
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
