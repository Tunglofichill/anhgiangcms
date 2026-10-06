using System;
using UnityEngine;
using UnityEngine.UI;

namespace Forge
{
    public class OfflinePopupView : MonoBehaviour
    {
        [Header("Components")]
        public GameObject panelPopup;
        public Text txtTitle;
        public Text txtOfflineGain;
        public Button btnClaim;

        private Action _onClaimCallback;

        public void Initialize(Action onClaim)
        {
            _onClaimCallback = onClaim;
            if (btnClaim != null)
            {
                btnClaim.onClick.RemoveAllListeners();
                btnClaim.onClick.AddListener(OnClaimClicked);
            }
            Hide();
        }

        public void Show(string durationStr, double gain)
        {
            if (panelPopup != null)
            {
                panelPopup.SetActive(true);
            }

            if (txtTitle != null)
            {
                txtTitle.text = LocalizationService.Get("OFFLINE_TITLE");
            }

            if (txtOfflineGain != null)
            {
                txtOfflineGain.text = LocalizationService.Get("OFFLINE_BODY", durationStr, HudView.FormatNumber(gain));
            }

            if (btnClaim != null)
            {
                var txt = btnClaim.GetComponentInChildren<Text>();
                if (txt != null) txt.text = LocalizationService.Get("OFFLINE_CLAIM");
            }
        }

        public void Hide()
        {
            if (panelPopup != null)
            {
                panelPopup.SetActive(false);
            }
        }

        public void Close() => Hide();

        private void OnClaimClicked()
        {
            Hide();
            _onClaimCallback?.Invoke();
        }
    }
}
