using UnityEngine;
using System;
using System.Collections.Generic;

namespace ColorDrive
{
    public enum IAPProductType { Consumable, NonConsumable, Subscription }

    [Serializable]
    public class IAPProduct
    {
        public string productId;
        public string displayName;
        public string price;
        public IAPProductType type;
    }

    /// <summary>
    /// Mock IAP Manager — ready for Unity IAP SDK integration.
    /// Replace SimulatePurchase with real IStoreController calls.
    /// </summary>
    public class IAPManager : MonoBehaviour
    {
        public static IAPManager Instance { get; private set; }

        public static readonly string PRODUCT_NO_ADS       = "com.colordrive.no_ads";
        public static readonly string PRODUCT_HINT_PACK    = "com.colordrive.hint_pack_5";
        public static readonly string PRODUCT_EXTRA_MOVES  = "com.colordrive.extra_moves_10";
        public static readonly string PRODUCT_COIN_S       = "com.colordrive.coins_1000";
        public static readonly string PRODUCT_COIN_M       = "com.colordrive.coins_6000";
        public static readonly string PRODUCT_COIN_L       = "com.colordrive.coins_15000";
        public static readonly string PRODUCT_THEME_SILVER = "com.colordrive.theme_silver";
        public static readonly string PRODUCT_THEME_BLACK  = "com.colordrive.theme_amgblack";
        public static readonly string PRODUCT_STARTER_PACK = "com.colordrive.starter_pack";
        public static readonly string PRODUCT_VIP_WEEKLY   = "com.colordrive.vip_weekly";
        public static readonly string PRODUCT_VIP_MONTHLY  = "com.colordrive.vip_monthly";

        public event Action<string, bool> OnPurchaseComplete;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Purchase(string productId, Action<bool> onResult = null)
        {
            Debug.Log($"[IAPManager] Purchase initiated: {productId}");
            // TODO: Unity IAP: m_StoreController.InitiatePurchase(productId);
            SimulatePurchase(productId, onResult);
        }

        private void SimulatePurchase(string productId, Action<bool> onResult)
        {
            // MOCK: always succeeds in editor
            ProcessPurchase(productId);
            onResult?.Invoke(true);
            OnPurchaseComplete?.Invoke(productId, true);
        }

        private void ProcessPurchase(string productId)
        {
            switch (productId)
            {
                case var id when id == PRODUCT_NO_ADS:
                    AdManager.Instance?.UnlockNoAds();
                    break;
                case var id when id == PRODUCT_HINT_PACK:
                    AddHints(5);
                    break;
                case var id when id == PRODUCT_EXTRA_MOVES:
                    AddBoosters("ExtraMoves", 10);
                    break;
                case var id when id == PRODUCT_COIN_S:
                    GameManager.Instance?.AddCoins(1000);
                    break;
                case var id when id == PRODUCT_COIN_M:
                    GameManager.Instance?.AddCoins(6000);
                    break;
                case var id when id == PRODUCT_COIN_L:
                    GameManager.Instance?.AddCoins(15000);
                    break;
                case var id when id == PRODUCT_THEME_SILVER:
                    UnlockTheme("Mercedes");
                    break;
                case var id when id == PRODUCT_THEME_BLACK:
                    UnlockTheme("AMGBlack");
                    break;
                case var id when id == PRODUCT_STARTER_PACK:
                    GameManager.Instance?.AddCoins(5000);
                    AddHints(10);
                    UnlockTheme("Mercedes");
                    break;
                case var id when id == PRODUCT_VIP_WEEKLY || id == PRODUCT_VIP_MONTHLY:
                    AdManager.Instance?.UnlockNoAds();
                    PlayerPrefs.SetInt("VIPActive", 1);
                    break;
                default:
                    Debug.LogWarning($"[IAPManager] Unknown product: {productId}");
                    break;
            }
        }

        public void AddHints(int count)
        {
            int current = PlayerPrefs.GetInt("Hints", 0);
            PlayerPrefs.SetInt("Hints", current + count);
        }

        public int GetHints() => PlayerPrefs.GetInt("Hints", 0);

        public void UseHint()
        {
            int hints = GetHints();
            if (hints > 0) PlayerPrefs.SetInt("Hints", hints - 1);
        }

        public void AddBoosters(string type, int count)
        {
            int current = PlayerPrefs.GetInt($"Booster_{type}", 0);
            PlayerPrefs.SetInt($"Booster_{type}", current + count);
        }

        public int GetBoosters(string type) => PlayerPrefs.GetInt($"Booster_{type}", 0);

        public void UnlockTheme(string themeName)
        {
            PlayerPrefs.SetInt($"Theme_{themeName}_Unlocked", 1);
            ThemeManager.Instance?.LoadThemeByName(themeName);
        }

        public bool IsThemeUnlocked(string themeName) =>
            PlayerPrefs.GetInt($"Theme_{themeName}_Unlocked", 0) == 1;
    }
}
