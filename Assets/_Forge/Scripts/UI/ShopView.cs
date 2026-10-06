using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Forge
{
    public class ShopView : MonoBehaviour
    {
        [Header("Shop Panel References")]
        public GameObject panelShop;
        public Button btnOpenShop;
        public Button btnCloseShop;
        public Text txtShopTitle;

        [Header("Shop Rows (Row_G1..Row_G5)")]
        public List<ShopRowView> shopRows = new List<ShopRowView>();

        private Action<string> _buyCallback;

        private void Start()
        {
            LocalizationService.OnLanguageChanged += UpdateTexts;
            UpdateTexts();
        }

        private void OnDestroy()
        {
            LocalizationService.OnLanguageChanged -= UpdateTexts;
        }

        public void UpdateTexts()
        {
            if (txtShopTitle != null) txtShopTitle.text = LocalizationService.Get("SHOP_TITLE");
            if (btnOpenShop != null)
            {
                var txt = btnOpenShop.GetComponentInChildren<Text>();
                if (txt != null) txt.text = LocalizationService.Get("SHOP_BUTTON");
            }
        }

        public void Initialize(Action<string> onBuyRequested)
        {
            _buyCallback = onBuyRequested;

            if (btnOpenShop != null)
            {
                btnOpenShop.onClick.RemoveAllListeners();
                btnOpenShop.onClick.AddListener(OpenShop);
            }

            if (btnCloseShop != null)
            {
                btnCloseShop.onClick.RemoveAllListeners();
                btnCloseShop.onClick.AddListener(CloseShop);
            }

            string[] genIds = new string[] { "G1", "G2", "G3", "G4", "G5" };
            for (int i = 0; i < shopRows.Count && i < genIds.Length; i++)
            {
                if (shopRows[i] != null)
                {
                    shopRows[i].Setup(genIds[i], OnRowBuyClicked);
                }
            }

            UpdateTexts();
            CloseShop();
        }

        public void OpenShop()
        {
            if (panelShop != null) panelShop.SetActive(true);
        }

        public void CloseShop()
        {
            if (panelShop != null) panelShop.SetActive(false);
        }

        public void Open() => OpenShop();
        public void Close() => CloseShop();

        public bool IsOpen => panelShop != null && panelShop.activeSelf;

        private void OnRowBuyClicked(string genId)
        {
            _buyCallback?.Invoke(genId);
        }

        public void RefreshShop(EconomyService economy, double currentOre, double totalEarned)
        {
            foreach (var row in shopRows)
            {
                if (row != null)
                {
                    row.Refresh(economy, currentOre, totalEarned);
                }
            }
        }
    }
}
