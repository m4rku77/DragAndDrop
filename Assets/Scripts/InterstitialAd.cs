using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InterstitialAd : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [Header("Unity Ads")]
    [SerializeField] private string _androidAdUnitId = "Interstitial_Android";
    private string _adUnitId;

    [Header("UI (optional)")]
    [SerializeField] private Button _interstitialAdButton;

    public event Action OnInterstitialAdReady;
    public bool isReady = false;

    // If a scene changed while ad was not ready, show after load
    private bool showOnNextLoad = false;

    // Optional singleton (prevents duplicates when using DontDestroyOnLoad)
    public static InterstitialAd Instance { get; private set; }

    private void Awake()
    {
        // Singleton pattern so only one persists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _adUnitId = _androidAdUnitId;

        // Persist across scenes
        DontDestroyOnLoad(gameObject);

        // Listen to scene changes
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void Start()
    {
        // Preload first interstitial
        LoadAd();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void Update()
    {
        if (_interstitialAdButton != null)
        {
            _interstitialAdButton.interactable = isReady;
        }
    }

    // Called automatically every time the active scene changes
    private void OnSceneChanged(Scene previous, Scene next)
    {
        Debug.Log($"[InterstitialAd] Scene changed: {previous.name} ➜ {next.name}");

        // Always try to show an interstitial on scene change
        if (isReady)
        {
            Debug.Log("[InterstitialAd] Ad is ready on scene change → showing interstitial!");
            ShowAd();
        }
        else
        {
            Debug.Log("[InterstitialAd] Ad not ready on scene change → will show after load.");
            showOnNextLoad = true;
            LoadAd();
        }
    }

    // --------------------
    // Public API
    // --------------------

    // If you have a UI button hooked up to the interstitial
    public void OnInterstitialAdButtonClicked()
    {
        Debug.Log("[InterstitialAd] Interstitial ad button clicked!");
        ShowInterstitial();
    }

    // Manual call if needed: tries to show if ready, otherwise loads.
    public void ShowInterstitial()
    {
        if (isReady)
        {
            Debug.Log("[InterstitialAd] Showing interstitial ad manually!");
            ShowAd();
        }
        else
        {
            Debug.Log("[InterstitialAd] Interstitial not ready yet, loading again!");
            showOnNextLoad = true; // when loaded, show it
            LoadAd();
        }
    }

    // Optional: call this from other scripts to register a button at runtime
    public void SetButton(Button button)
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnInterstitialAdButtonClicked);
        _interstitialAdButton = button;
        _interstitialAdButton.interactable = false;
    }

    // --------------------
    // Loading + Showing
    // --------------------

    public void LoadAd()
    {
        if (!Advertisement.isInitialized)
        {
            Debug.LogWarning("[InterstitialAd] Tried to load interstitial ad before Unity Ads was initialized!");
            return;
        }

        Debug.Log("[InterstitialAd] Loading interstitial ad...");
        Advertisement.Load(_adUnitId, this);
    }

    public void ShowAd()
    {
        if (isReady)
        {
            Debug.Log("[InterstitialAd] Showing interstitial ad via Advertisement.Show");
            Advertisement.Show(_adUnitId, this);
            isReady = false;

            if (_interstitialAdButton != null)
                _interstitialAdButton.interactable = false;
        }
        else
        {
            Debug.LogWarning("[InterstitialAd] Tried to show interstitial but it's not ready!");
            LoadAd();
        }
    }

    // --------------------
    // IUnityAdsLoadListener
    // --------------------

    public void OnUnityAdsAdLoaded(string placementId)
    {
        Debug.Log("[InterstitialAd] Interstitial ad loaded!");

        isReady = true;

        if (_interstitialAdButton != null)
            _interstitialAdButton.interactable = true;

        OnInterstitialAdReady?.Invoke();

        if (showOnNextLoad)
        {
            showOnNextLoad = false;
            Debug.Log("[InterstitialAd] Scene changed while loading → showing interstitial now!");
            ShowAd();
        }
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogWarning($"[InterstitialAd] Failed to load interstitial ad! {error}: {message}");
        // Optional: you could add a delay here
        // StartCoroutine(WaitAndLoad(5f));
        LoadAd();
    }

    // --------------------
    // IUnityAdsShowListener
    // --------------------

    public void OnUnityAdsShowStart(string placementId)
    {
        Debug.Log("[InterstitialAd] Interstitial is now showing!");
        Time.timeScale = 0f; // Pause game while showing
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log("[InterstitialAd] User clicked on interstitial ad!");
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        Debug.Log($"[InterstitialAd] Interstitial ad completed with state: {showCompletionState}");

        // Restore game speed and preload next ad
        Time.timeScale = 1f;
        LoadAd();
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogWarning($"[InterstitialAd] Error showing interstitial ad! {error}: {message}");
        Time.timeScale = 1f;
        LoadAd();
    }

    // --------------------
    // Optional helper coroutine (currently unused)
    // --------------------

    private IEnumerator SlowDownTimeTemporarily(float seconds)
    {
        Time.timeScale = 0.4f;
        Debug.Log("[InterstitialAd] Time slowed down to 0.4x for " + seconds + " sec");
        yield return new WaitForSeconds(seconds);
        Time.timeScale = 1.0f;
        Debug.Log("[InterstitialAd] Time restored to normal!");
    }
}
