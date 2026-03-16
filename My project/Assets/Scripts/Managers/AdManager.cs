using UnityEngine;
using System;

namespace ColorDrive
{
    /// <summary>
    /// Mock Ad Manager — ready for Unity Ads / AdMob SDK integration.
    /// Replace SimulateAd methods with real SDK calls.
    /// </summary>
    public class AdManager : MonoBehaviour
    {
        public static AdManager Instance { get; private set; }

        [Header("Ad Settings")]
        [SerializeField] private int interstitialFrequency = 3; // Show every N level completions
        private int levelCompletionCount = 0;
        private bool noAdsUnlocked = false;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            noAdsUnlocked = PlayerPrefs.GetInt("NoAds", 0) == 1;
        }

        // --- Rewarded Ads ---

        public void ShowRewardedAd(string placement, Action<bool> onComplete)
        {
            Debug.Log($"[AdManager] ShowRewardedAd: {placement}");
            // TODO: Replace with Unity Ads SDK call
            // Advertisement.Show(placement, new RewardedAdListener(onComplete));
            SimulateRewardedAd(onComplete);
        }

        private void SimulateRewardedAd(Action<bool> onComplete)
        {
            // In real implementation, this is triggered by ad SDK callback
            Debug.Log("[AdManager] [MOCK] Rewarded ad completed — reward granted");
            onComplete?.Invoke(true);
        }

        // --- Interstitial Ads ---

        public void OnLevelComplete()
        {
            if (noAdsUnlocked) return;
            levelCompletionCount++;
            if (levelCompletionCount >= interstitialFrequency)
            {
                levelCompletionCount = 0;
                ShowInterstitial();
            }
        }

        private void ShowInterstitial()
        {
            Debug.Log("[AdManager] [MOCK] Showing interstitial ad");
            // TODO: Advertisement.Show("Interstitial");
        }

        // --- Banner Ads ---

        public void ShowBanner()
        {
            if (noAdsUnlocked) return;
            Debug.Log("[AdManager] [MOCK] Banner shown");
            // TODO: Advertisement.Banner.Show("Banner");
        }

        public void HideBanner()
        {
            Debug.Log("[AdManager] [MOCK] Banner hidden");
            // TODO: Advertisement.Banner.Hide();
        }

        // --- IAP Integration ---

        public void UnlockNoAds()
        {
            noAdsUnlocked = true;
            PlayerPrefs.SetInt("NoAds", 1);
            HideBanner();
            Debug.Log("[AdManager] No Ads unlocked.");
        }

        public bool IsNoAdsUnlocked() => noAdsUnlocked;
    }
}
