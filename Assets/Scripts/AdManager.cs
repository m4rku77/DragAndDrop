using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AdManager : MonoBehaviour
{
    public AdsInitializer adsInitializer;
    public InterstitialAd interstitialAd;
    [SerializeField] bool turnOffInterstitialAd = false;
    private bool firstAdShown = false;

    public RewardedAds rewardedAds;
    [SerializeField] bool turnOffRewardedAds = false;

    public BannerAd BannerAd;
    [SerializeField] bool turnOffBannerAds = false;


    public static AdManager Instance { get; private set; }


    private void Awake()
    {
        if (adsInitializer == null)
            adsInitializer = FindFirstObjectByType<AdsInitializer>();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        adsInitializer.OnAdsInitialized += HandleAdsInitialized;
    }

    private void HandleAdsInitialized()
    {
        if (!turnOffInterstitialAd)
        {
            interstitialAd.OnInterstitialAdReady += HandleInterstitialReady;
            interstitialAd.LoadAd();
        }

        if (!turnOffRewardedAds)
        {
            rewardedAds.LoadAd();
        }

        if (!turnOffBannerAds)
        {
            BannerAd.LoadBanner();
        }
    }

    private void HandleInterstitialReady()
    {
        if (!firstAdShown)
        {
            Debug.Log("Showing first time interstitial ad automatically!");
            interstitialAd.ShowAd();
            firstAdShown = true;

        }
        else
        {
            Debug.Log("Next interstitial ad is ready for manual show!");
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private bool firstSceneLoad = false;
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (interstitialAd == null)
            interstitialAd = FindFirstObjectByType<InterstitialAd>();

        Button interstitialButton =
            GameObject.FindGameObjectWithTag("InterstitialAdButton").GetComponent<Button>();

        if (interstitialAd != null && interstitialButton != null)
        {
            interstitialAd.SetButton(interstitialButton);
        }


        if (rewardedAds == null)
            rewardedAds = FindFirstObjectByType<RewardedAds>();

        if (BannerAd == null)
            BannerAd = FindFirstObjectByType<BannerAd>();


        Button rewardedAdButton =
            GameObject.FindGameObjectWithTag("RewardedButton").GetComponent<Button>();

        if (rewardedAds != null && rewardedAdButton != null)
            rewardedAds.SetButton(rewardedAdButton);

        Button bannerButton = GameObject.FindGameObjectWithTag("BannerButton").GetComponent<Button>();
        if (BannerAd != null && bannerButton != null)
        {
            BannerAd.SetButton(bannerButton);
        }

        if (!firstSceneLoad)
        {
            firstSceneLoad = true;
            Debug.Log("First time scene loaded!");
            return;
        }

        Debug.Log("Scene loaded!");
        HandleAdsInitialized();

    }
}