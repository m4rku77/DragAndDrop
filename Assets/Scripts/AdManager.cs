using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AdManager : MonoBehaviour
{
    public AdsInitializer adsInitializer;
    public InterstitialAd interstitialAd;
    [SerializeField] bool turnOffInterstitialAd = false;
    private bool firstSceneLoad = true;

    public static AdManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (adsInitializer == null)
            adsInitializer = FindFirstObjectByType<AdsInitializer>();

        if (adsInitializer != null)
            adsInitializer.OnAdsInitialized += HandleAdsInitialized;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void HandleAdsInitialized()
    {
        if (turnOffInterstitialAd) return;

        if (interstitialAd == null)
            interstitialAd = FindFirstObjectByType<InterstitialAd>();

        if (interstitialAd != null)
        {
            interstitialAd.OnInterstitialAdReady += OnInterstitialReady;
            interstitialAd.LoadAd();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (turnOffInterstitialAd) return;

        if (interstitialAd == null)
            interstitialAd = FindFirstObjectByType<InterstitialAd>();

        if (interstitialAd == null)
        {
            Debug.LogWarning("InterstitialAd not found in scene!");
            return;
        }

        // Try linking the button if it exists
        GameObject adButtonObj = GameObject.FindGameObjectWithTag("InterstitialAdButton");
        if (adButtonObj != null)
        {
            Button interstitialButton = adButtonObj.GetComponent<Button>();
            interstitialAd.SetButton(interstitialButton);
        }

        // Skip showing ad for very first scene load (startup)
        if (firstSceneLoad)
        {
            firstSceneLoad = false;
            return;
        }

        //  Show interstitial ad when moving to a new scene
        ShowInterstitialBetweenScenes();
    }

    private void ShowInterstitialBetweenScenes()
    {
        if (interstitialAd != null)
        {
            if (interstitialAd.isReady)
            {
                Debug.Log("Showing interstitial ad on scene transition!");
                interstitialAd.ShowAd();
            }
            else
            {
                Debug.Log("Interstitial not ready yet, preloading for next scene...");
                interstitialAd.LoadAd();
            }
        }
    }

    private void OnInterstitialReady()
    {
        Debug.Log("Interstitial ad ready for next scene transition!");
    }
}
