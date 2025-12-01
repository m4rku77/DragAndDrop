using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RewardedAds : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] string _androidAdUnitId = "Rewarded_Android";
    string _adUnitId;

    [SerializeField] Button _rewardedAdButton;
    public FlyingObjectManager flyingObjectManager;

    void Awake()
    {
        _adUnitId = _androidAdUnitId;

        // Persist this object across scenes
        DontDestroyOnLoad(gameObject);

        // First lookup (for the initial scene)
        if (flyingObjectManager == null)
            flyingObjectManager = FindFirstObjectByType<FlyingObjectManager>();

        // Subscribe to scene loaded so we can refresh references
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Called every time a new scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Re-find FlyingObjectManager in the new scene
        flyingObjectManager = FindFirstObjectByType<FlyingObjectManager>();
        Debug.Log($"[RewardedAds] Scene loaded: {scene.name}, FlyingObjectManager found: { (flyingObjectManager != null) }");
    }

    public void LoadAd()
    {
        if (!Advertisement.isInitialized)
        {
            Debug.LogWarning("Tried to load rewarded ad before Unity ads was initialized.");
            return;
        }

        Debug.Log("Loading rewarded ad.");
        Advertisement.Load(_adUnitId, this);
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        Debug.Log("Rewarded ad loaded!");

        if (placementId.Equals(_adUnitId) && _rewardedAdButton != null)
        {
            _rewardedAdButton.interactable = true;
        }
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogWarning($"Failed to load rewarded ad! {error}: {message}");
        StartCoroutine(WaitAndLoad(5f));
    }

    public IEnumerator WaitAndLoad(float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadAd();
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogWarning($"Failed to show rewarded ad! {error}: {message}");
        StartCoroutine(WaitAndLoad(5f));
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        Time.timeScale = 0f;
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log("User clicked on rewarded ad");
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        Debug.Log($"Rewarded ad completed! State: {showCompletionState}, Scene: {SceneManager.GetActiveScene().name}");

        // ✅ HANOI: give -5 moves when fully watched
        if (SceneManager.GetActiveScene().name == "HanojasTornis" &&
            showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            if (MoveCounter.Instance != null)
            {
                MoveCounter.Instance.RemoveMoves(5);
                Debug.Log("Removed 5 moves from MoveCounter");
            }
            else
            {
                Debug.LogWarning("MoveCounter.Instance is null in HanojasTornis!");
            }
        }

        // ✅ Destroy flying objects in ANY scene that has a FlyingObjectManager
        if (flyingObjectManager == null)
        {
            // Try to re-find just in case
            flyingObjectManager = FindFirstObjectByType<FlyingObjectManager>();
        }

        if (flyingObjectManager != null)
        {
            Debug.Log("[RewardedAds] Destroying all flying objects via FlyingObjectManager");
            flyingObjectManager.DestroyAllFlyingObjects();
        }
        else
        {
            Debug.LogWarning("[RewardedAds] No FlyingObjectManager found in this scene to destroy objects!");
        }

        if (_rewardedAdButton != null)
            _rewardedAdButton.interactable = false;

        StartCoroutine(WaitAndLoad(10f));

        Time.timeScale = 1f;
    }

    public void SetButton(Button button)
    {
        if (button == null)
        {
            return;
        }
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(ShowAd);
        _rewardedAdButton = button;
        _rewardedAdButton.interactable = false;
    }

    public void ShowAd()
    {
        if (_rewardedAdButton != null)
            _rewardedAdButton.interactable = false;

        Advertisement.Show(_adUnitId, this);
    }
}
