using System;
using UnityEngine;
using UnityEngine.UI;

namespace Forge
{
    public class HudView : MonoBehaviour
    {
        [Header("UI Text Components (UnityEngine.UI)")]
        public Text txtOre;
        public Text txtDps;
        public Text txtRank;

        [Header("Icons & Cards")]
        public Image imgOreIcon;
        public Image panelCurrencyCard;
        public Image panelRankCard;

        [Header("Shop Notification Badge")]
        public GameObject goShopBadge;
        public Text txtShopBadge;
        public Button btnSettings;

        private double _lastOre;
        private double _lastDps;
        private string _lastRank = "";

        private void Start()
        {
            LocalizationService.OnLanguageChanged += RefreshHUD;
        }

        private void OnDestroy()
        {
            LocalizationService.OnLanguageChanged -= RefreshHUD;
        }

        private void Update()
        {
            // Gentle pulse on shop badge if active
            if (goShopBadge != null && goShopBadge.activeSelf)
            {
                float s = 1f + Mathf.Sin(Time.unscaledTime * 6f) * 0.12f;
                goShopBadge.transform.localScale = new Vector3(s, s, 1f);
            }
        }

        public void UpdateHUD(double ore, double dps, string rank)
        {
            _lastOre = ore;
            _lastDps = dps;
            _lastRank = rank;
            RefreshHUD();
        }

        private void RefreshHUD()
        {
            if (txtOre != null)
            {
                txtOre.text = LocalizationService.Get("ORE_LABEL", FormatNumber(_lastOre));
            }

            if (txtDps != null)
            {
                txtDps.text = LocalizationService.Get("DPS_LABEL", FormatNumber(_lastDps));
            }

            if (txtRank != null)
            {
                txtRank.text = LocalizationService.Get("RANK_LABEL", _lastRank);
            }
        }

        public void SetShopNotification(bool canUpgrade)
        {
            if (goShopBadge != null)
            {
                goShopBadge.SetActive(canUpgrade);
            }
        }

        public static string FormatNumber(double val)
        {
            if (val < 0) val = 0;

            if (val < 1000.0)
            {
                return Math.Floor(val).ToString("N0");
            }
            if (val < 1000000.0)
            {
                return (val / 1000.0).ToString("0.#") + "K";
            }
            if (val < 1000000000.0)
            {
                return (val / 1000000.0).ToString("0.#") + "M";
            }
            if (val < 1000000000000.0)
            {
                return (val / 1000000000.0).ToString("0.#") + "B";
            }
            return (val / 1000000000000.0).ToString("0.#") + "T";
        }
    }
}
