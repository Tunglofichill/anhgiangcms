using System;
using UnityEngine;
using UnityEngine.UI;

namespace Forge
{
    public class ShopRowView : MonoBehaviour
    {
        [Header("Components")]
        public Text txtName;
        public Text txtLevel;
        public Text txtOutput;
        public Text txtCost;
        public Button btnBuy;
        public Image imgIcon;

        private string _generatorId;
        private Action<string> _onBuyClicked;

        private void Awake()
        {
            if (txtCost != null)
            {
                txtCost.alignment = TextAnchor.MiddleRight;
                txtCost.resizeTextForBestFit = true;
                txtCost.resizeTextMinSize = 13;
                txtCost.resizeTextMaxSize = 22;
                txtCost.horizontalOverflow = HorizontalWrapMode.Wrap;

                var rtCost = txtCost.rectTransform;
                rtCost.anchorMin = new Vector2(1f, 0.5f);
                rtCost.anchorMax = new Vector2(1f, 0.5f);
                rtCost.pivot = new Vector2(1f, 0.5f);
                rtCost.anchoredPosition = new Vector2(-190f, 0f);
                rtCost.sizeDelta = new Vector2(390f, 44f);
            }

            if (txtOutput != null)
            {
                var rtOut = txtOutput.rectTransform;
                rtOut.anchorMin = new Vector2(0f, 0.5f);
                rtOut.anchorMax = new Vector2(0f, 0.5f);
                rtOut.pivot = new Vector2(0f, 0.5f);
                rtOut.anchoredPosition = new Vector2(350f, 0f);
                rtOut.sizeDelta = new Vector2(160f, 32f);
            }
        }

        public void Setup(string genId, Action<string> onBuy)
        {
            _generatorId = genId;
            _onBuyClicked = onBuy;

            if (btnBuy != null)
            {
                btnBuy.onClick.RemoveAllListeners();
                btnBuy.onClick.AddListener(() => _onBuyClicked?.Invoke(_generatorId));
            }
        }

        public void Refresh(EconomyService economy, double currentOre, double totalEarned)
        {
            if (economy == null || string.IsNullOrEmpty(_generatorId)) return;

            if (_generatorId.StartsWith("UP_"))
            {
                RefreshToolUpgrade(economy, currentOre);
                return;
            }

            var def = economy.GetDefinition(_generatorId);
            if (def == null) return;

            bool isUnlocked = economy.IsGeneratorUnlocked(_generatorId, totalEarned);
            int currentLevel = economy.GetGeneratorLevel(_generatorId);

            if (imgIcon != null && def.icon != null)
            {
                imgIcon.sprite = def.icon;
            }

            string machineName = LocalizationService.GetGeneratorName(_generatorId);

            if (!isUnlocked)
            {
                string unlockReq = "";
                if (def.unlockOre > 0)
                {
                    unlockReq = LocalizationService.Get("LOCKED_ORE", HudView.FormatNumber(def.unlockOre));
                }
                else if (!string.IsNullOrEmpty(def.unlockGenId) && def.unlockLevel > 0)
                {
                    string prereqName = LocalizationService.GetGeneratorName(def.unlockGenId);
                    unlockReq = LocalizationService.Get("LOCKED_GEN", prereqName, def.unlockLevel);
                }

                if (txtName != null) txtName.text = $"{machineName} {LocalizationService.Get("LOCKED_SUFFIX")}";
                if (txtLevel != null) txtLevel.text = LocalizationService.Get("LEVEL_LABEL", 0);
                if (txtOutput != null) txtOutput.text = LocalizationService.Get("RATE_LABEL", 0);
                if (txtCost != null)
                {
                    txtCost.text = unlockReq;
                    txtCost.color = new Color(1.0f, 0.50f, 0.45f); // Warm salmon red for locked condition
                }

                if (btnBuy != null)
                {
                    btnBuy.interactable = false;
                    var img = btnBuy.GetComponent<Image>();
                    if (img != null) img.color = new Color(0.22f, 0.22f, 0.26f, 0.85f);
                    var tBuy = btnBuy.GetComponentInChildren<Text>();
                    if (tBuy != null) { tBuy.text = "🔒 Khóa"; tBuy.color = new Color(0.6f, 0.6f, 0.65f); }
                }
                return;
            }

            double cost = economy.GetGeneratorCost(_generatorId);
            double currentOutput = economy.GetGeneratorCurrentOutput(_generatorId);
            bool canAfford = economy.CanAfford(_generatorId, currentOre);

            if (txtName != null) txtName.text = machineName;
            if (txtLevel != null) txtLevel.text = LocalizationService.Get("LEVEL_LABEL", currentLevel);
            if (txtOutput != null) txtOutput.text = LocalizationService.Get("RATE_LABEL", HudView.FormatNumber(currentOutput));
            if (txtCost != null)
            {
                txtCost.text = LocalizationService.Get("COST_LABEL", HudView.FormatNumber(cost));
                txtCost.color = new Color(1.0f, 0.88f, 0.35f); // Bright gold for cost
            }

            if (btnBuy != null)
            {
                btnBuy.interactable = canAfford;
                var img = btnBuy.GetComponent<Image>();
                var tBuy = btnBuy.GetComponentInChildren<Text>();
                if (canAfford)
                {
                    if (img != null) img.color = new Color(0.18f, 0.65f, 0.30f, 1f); // Vibrant Emerald Green
                    if (tBuy != null) { tBuy.text = LocalizationService.Get("UPGRADE_BTN"); tBuy.color = Color.white; }
                }
                else
                {
                    if (img != null) img.color = new Color(0.30f, 0.30f, 0.35f, 0.85f); // Soft Slate Gray
                    if (tBuy != null) { tBuy.text = "Chưa Đủ"; tBuy.color = new Color(0.75f, 0.75f, 0.80f); }
                }
            }
        }

        private void RefreshToolUpgrade(EconomyService economy, double currentOre)
        {
            int currentLevel = economy.GetUpgradeLevel(_generatorId);
            bool isMax = economy.IsUpgradeMaxLevel(_generatorId);
            double cost = economy.GetUpgradeCost(_generatorId);
            bool canAfford = economy.CanAffordUpgrade(_generatorId, currentOre);

            string title = LocalizationService.Get(_generatorId + "_NAME");
            string statBonus = "";

            switch (_generatorId)
            {
                case "UP_HAMMER":
                    int hLv = Mathf.Max(1, currentLevel);
                    double tap = 1.0 + (hLv - 1) * 2.0 + (hLv / 5) * 5.0;
                    statBonus = LocalizationService.Get("UP_HAMMER_DESC", HudView.FormatNumber(tap));
                    break;
                case "UP_FURNACE":
                    int fBonus = (Mathf.Max(1, currentLevel) - 1) * 15;
                    statBonus = LocalizationService.Get("UP_FURNACE_DESC", fBonus);
                    break;
                case "UP_GLOVES":
                    int critRate = currentLevel * 5;
                    statBonus = LocalizationService.Get("UP_GLOVES_DESC", critRate);
                    break;
                case "UP_OIL":
                    int dpsBonus = currentLevel * 15;
                    statBonus = LocalizationService.Get("UP_OIL_DESC", dpsBonus);
                    break;
            }

            if (txtName != null) txtName.text = title;
            if (txtLevel != null)
            {
                txtLevel.text = isMax ? LocalizationService.Get("UPGRADE_MAX") : LocalizationService.Get("LEVEL_LABEL", currentLevel);
            }
            if (txtOutput != null)
            {
                txtOutput.text = statBonus;
                txtOutput.color = new Color(1.0f, 0.75f, 0.30f); // Fiery Amber
            }
            if (txtCost != null)
            {
                if (isMax)
                {
                    txtCost.text = LocalizationService.Get("UPGRADE_MAX");
                    txtCost.color = new Color(0.6f, 0.9f, 0.6f);
                }
                else
                {
                    txtCost.text = LocalizationService.Get("COST_LABEL", HudView.FormatNumber(cost));
                    txtCost.color = new Color(1.0f, 0.88f, 0.35f);
                }
            }

            if (btnBuy != null)
            {
                btnBuy.interactable = !isMax && canAfford;
                var img = btnBuy.GetComponent<Image>();
                var tBuy = btnBuy.GetComponentInChildren<Text>();
                if (isMax)
                {
                    if (img != null) img.color = new Color(0.20f, 0.45f, 0.25f, 0.85f);
                    if (tBuy != null) { tBuy.text = LocalizationService.Get("UPGRADE_MAX"); tBuy.color = new Color(0.8f, 1f, 0.8f); }
                }
                else if (canAfford)
                {
                    if (img != null) img.color = new Color(0.18f, 0.65f, 0.30f, 1f);
                    if (tBuy != null) { tBuy.text = LocalizationService.Get("UPGRADE_BTN"); tBuy.color = Color.white; }
                }
                else
                {
                    if (img != null) img.color = new Color(0.30f, 0.30f, 0.35f, 0.85f);
                    if (tBuy != null) { tBuy.text = "Chưa Đủ"; tBuy.color = new Color(0.75f, 0.75f, 0.80f); }
                }
            }
        }
    }
}
