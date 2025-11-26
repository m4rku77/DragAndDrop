using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class HanoiWinManager : MonoBehaviour
{
    public static HanoiWinManager Instance { get; private set; }

    [Header("Setup")]
    public Transform targetStick;        // where all blocks must end
    public GameObject winPanel;          // win UI panel

    [Header("Win Panel UI")]
    public TMP_Text winTimeText;         // time shown on win panel
    public TMP_Text winMovesText;        // moves shown on win panel

    [Header("HUD (In-game UI)")]
    public TMP_Text liveTimerText;       // the timer text you already show on top
    public MoveCounter moveCounter;      // reference to your MoveCounter

    [Header("Settings")]
    public float xTolerance = 0.5f;
    public bool pauseOnWin = true;
    public bool autoCheckEachFrame = true;

    [Header("Scenes")]
    public string menuSceneName;         // scene name of your main menu

    private bool hasWon = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (winPanel != null)
            winPanel.SetActive(false);

        hasWon = false;

        if (targetStick == null)
            Debug.LogError("[HanoiWinManager] targetStick NOT assigned in Inspector!");
    }

    void Update()
    {
        if (autoCheckEachFrame && !hasWon)
        {
            CheckWin();
        }
    }

    public void CheckWin()
    {
        if (hasWon || targetStick == null)
            return;

        // find all real blocks
        HanoiBlock[] allBlocks = FindObjectsByType<HanoiBlock>(FindObjectsSortMode.None);

        bool allOnStick = true;

        foreach (var block in allBlocks)
        {
            // only consider objects whose tag ends with "block" (0block, 1block, etc.)
            if (!block.tag.EndsWith("block"))
                continue;

            float dx = Mathf.Abs(block.transform.position.x - targetStick.position.x);
            if (dx > xTolerance)
            {
                allOnStick = false;
                break;
            }
        }

        if (!allOnStick)
            return;

        // 🎉 WIN
        hasWon = true;
        Debug.Log("[HanoiWinManager] WIN! Tower complete.");

        // fill win panel info
        if (winPanel != null)
        {
            winPanel.SetActive(true);

            // copy time from HUD timer to win panel
            if (liveTimerText != null && winTimeText != null)
                winTimeText.text = liveTimerText.text;

            // show moves
            if (moveCounter != null && winMovesText != null)
                winMovesText.text = "Moves: " + moveCounter.CurrentMoves;
        }
        else
        {
            Debug.LogError("[HanoiWinManager] winPanel not assigned!");
        }

        if (pauseOnWin)
            Time.timeScale = 0f;
    }

    // 🔁 Called from Restart button
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    // 🏠 Called from Menu button
    public void GoToMenu()
    {
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(menuSceneName))
        {
            SceneManager.LoadScene(menuSceneName);
        }
        else
        {
            Debug.LogError("[HanoiWinManager] menuSceneName not set!");
        }
    }
}
