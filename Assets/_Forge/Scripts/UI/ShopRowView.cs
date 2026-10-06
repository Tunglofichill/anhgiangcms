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
                    var tBuy = btnBuy.GetComponentInChildren<Text>();
                    if (tBuy != null) tBuy.text = LocalizationService.Get("UPGRADE_BTN");
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
                var tBuy = btnBuy.GetComponentInChildren<Text>();
                if (tBuy != null) tBuy.text = LocalizationService.Get("UPGRADE_BTN");
            }
        }
    }
}
