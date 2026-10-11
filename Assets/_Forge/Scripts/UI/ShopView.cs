using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Forge
{
    public class ShopView : MonoBehaviour
    {
        public enum ShopTab
        {
            Machines,
            Upgrades
        }

        [Header("Shop Panel References")]
        public GameObject panelShop;
        public Button btnOpenShop;
        public Button btnCloseShop;
        public Text txtShopTitle;

        [Header("Tabs")]
        public Button btnTabMachines;
        public Button btnTabUpgrades;
        public GameObject containerMachines;
        public GameObject containerUpgrades;

        [Header("Shop Rows (Row_G1..Row_G5)")]
        public List<ShopRowView> shopRows = new List<ShopRowView>();

        [Header("Tool & Furnace Upgrade Rows")]
        public List<ShopRowView> upgradeRows = new List<ShopRowView>();

        private Action<string> _buyCallback;
        private ShopTab _currentTab = ShopTab.Machines;

        private static readonly Color ColorTabActiveBg = new Color(0.72f, 0.40f, 0.12f, 1f);
        private static readonly Color ColorTabInactiveBg = new Color(0.18f, 0.22f, 0.30f, 0.9f);
        private static readonly Color ColorTabActiveText = new Color(1.0f, 0.95f, 0.80f, 1f);
        private static readonly Color ColorTabInactiveText = new Color(0.65f, 0.72f, 0.82f, 1f);

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

            if (btnTabMachines != null)
            {
                var txt = btnTabMachines.GetComponentInChildren<Text>();
                if (txt != null) txt.text = LocalizationService.Get("SHOP_TAB_MACHINES");
            }

            if (btnTabUpgrades != null)
            {
                var txt = btnTabUpgrades.GetComponentInChildren<Text>();
                if (txt != null) txt.text = LocalizationService.Get("SHOP_TAB_UPGRADES");
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

            if (btnTabMachines != null)
            {
                btnTabMachines.onClick.RemoveAllListeners();
                btnTabMachines.onClick.AddListener(() => SwitchTab(ShopTab.Machines));
            }

            if (btnTabUpgrades != null)
            {
                btnTabUpgrades.onClick.RemoveAllListeners();
                btnTabUpgrades.onClick.AddListener(() => SwitchTab(ShopTab.Upgrades));
            }

            string[] genIds = new string[] { "G1", "G2", "G3", "G4", "G5" };
            for (int i = 0; i < shopRows.Count && i < genIds.Length; i++)
            {
                if (shopRows[i] != null)
                {
                    shopRows[i].Setup(genIds[i], OnRowBuyClicked);
                }
            }

            string[] upIds = new string[] { "UP_HAMMER", "UP_FURNACE", "UP_GLOVES", "UP_OIL" };
            for (int i = 0; i < upgradeRows.Count && i < upIds.Length; i++)
            {
                if (upgradeRows[i] != null)
                {
                    upgradeRows[i].Setup(upIds[i], OnRowBuyClicked);
                }
            }

            SwitchTab(ShopTab.Machines);
            UpdateTexts();
            CloseShop();
        }

        public void SwitchTab(ShopTab tab)
        {
            _currentTab = tab;

            if (containerMachines != null) containerMachines.SetActive(tab == ShopTab.Machines);
            if (containerUpgrades != null) containerUpgrades.SetActive(tab == ShopTab.Upgrades);

            ApplyTabVisual(btnTabMachines, tab == ShopTab.Machines);
            ApplyTabVisual(btnTabUpgrades, tab == ShopTab.Upgrades);
        }

        private void ApplyTabVisual(Button btn, bool isActive)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null)
            {
                img.color = isActive ? ColorTabActiveBg : ColorTabInactiveBg;
            }

            var txt = btn.GetComponentInChildren<Text>();
            if (txt != null)
            {
                txt.color = isActive ? ColorTabActiveText : ColorTabInactiveText;
            }
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

        private void OnRowBuyClicked(string id)
        {
            _buyCallback?.Invoke(id);
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

            foreach (var row in upgradeRows)
            {
                if (row != null)
                {
                    row.Refresh(economy, currentOre, totalEarned);
                }
            }
        }
    }
}
