using System;
using UnityEngine;
using UnityEngine.UI;

namespace Forge
{
    public class SettingsView : MonoBehaviour
    {
        public static SettingsView Instance { get; private set; }

        [Header("Main Dialog References")]
        public GameObject panelSettings;
        public Button btnCloseSettings;
        public Button btnOpenSettings;

        [Header("Title")]
        public Text txtTitle;

        [Header("Orientation Controls")]
        public Text txtOrientationLabel;
        public Button btnLandscape;
        public Image imgLandscape;
        public Text txtLandscape;
        public Button btnPortrait;
        public Image imgPortrait;
        public Text txtPortrait;

        [Header("Audio Controls")]
        public Text txtSFXLabel;
        public Button btnToggleSFX;
        public Text txtSFXState;
        public Slider sliderSFX;

        public Text txtBGMLabel;
        public Button btnToggleBGM;
        public Text txtBGMState;
        public Slider sliderBGM;

        [Header("FX & Haptics Controls")]
        public Text txtVFXLabel;
        public Button btnToggleVFX;
        public Text txtVFXState;

        public Text txtHapticsLabel;
        public Button btnToggleHaptics;
        public Text txtHapticsState;

        [Header("Language Controls")]
        public Text txtLangLabel;
        public Button btnLangVi;
        public Image imgLangVi;
        public Text txtLangVi;
        public Button btnLangEn;
        public Image imgLangEn;
        public Text txtLangEn;

        [Header("Main Menu Navigation")]
        public Button btnReturnMainMenu;
        public Text txtReturnMainMenu;

        [Header("Reset Game Controls")]
        public Button btnResetData;
        public Text txtResetData;

        [Header("Reset Confirmation Modal")]
        public GameObject panelResetConfirm;
        public Text txtConfirmTitle;
        public Text txtConfirmMsg;
        public Button btnConfirmReset;
        public Text txtConfirmReset;
        public Button btnCancelReset;
        public Text txtCancelReset;

        private readonly Color COLOR_ACTIVE_BTN = new Color(0.95f, 0.70f, 0.25f, 1f);
        private readonly Color COLOR_INACTIVE_BTN = new Color(0.25f, 0.28f, 0.35f, 0.9f);
        private readonly Color COLOR_ON = new Color(0.20f, 0.75f, 0.35f);
        private readonly Color COLOR_OFF = new Color(0.70f, 0.25f, 0.25f);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Initialize()
        {
            if (btnOpenSettings != null)
            {
                btnOpenSettings.onClick.RemoveAllListeners();
                btnOpenSettings.onClick.AddListener(Open);
            }

            if (btnCloseSettings != null)
            {
                btnCloseSettings.onClick.RemoveAllListeners();
                btnCloseSettings.onClick.AddListener(Close);
            }

            // Orientation (Bỏ chế độ Dọc theo yêu cầu, khóa chuẩn Landscape)
            if (txtOrientationLabel != null && txtOrientationLabel.transform.parent != null)
            {
                txtOrientationLabel.transform.parent.gameObject.SetActive(false);
            }
            if (btnPortrait != null)
            {
                btnPortrait.gameObject.SetActive(false);
            }
            if (btnLandscape != null)
            {
                btnLandscape.onClick.RemoveAllListeners();
                btnLandscape.onClick.AddListener(() =>
                {
                    SettingsService.Instance?.SetOrientation(ScreenOrientationMode.Landscape);
                    RefreshUI();
                });
            }

            // Audio SFX
            if (btnToggleSFX != null)
            {
                btnToggleSFX.onClick.RemoveAllListeners();
                btnToggleSFX.onClick.AddListener(() =>
                {
                    if (SettingsService.Instance != null)
                    {
                        SettingsService.Instance.SetSFX(!SettingsService.Instance.SFXEnabled, SettingsService.Instance.SFXVolume);
                        RefreshUI();
                    }
                });
            }

            if (sliderSFX != null)
            {
                sliderSFX.onValueChanged.RemoveAllListeners();
                sliderSFX.onValueChanged.AddListener((val) =>
                {
                    if (SettingsService.Instance != null)
                    {
                        SettingsService.Instance.SetSFX(SettingsService.Instance.SFXEnabled, val);
                    }
                });
            }

            // Audio BGM
            if (btnToggleBGM != null)
            {
                btnToggleBGM.onClick.RemoveAllListeners();
                btnToggleBGM.onClick.AddListener(() =>
                {
                    if (SettingsService.Instance != null)
                    {
                        SettingsService.Instance.SetBGM(!SettingsService.Instance.BGMEnabled, SettingsService.Instance.BGMVolume);
                        RefreshUI();
                    }
                });
            }

            if (sliderBGM != null)
            {
                sliderBGM.onValueChanged.RemoveAllListeners();
                sliderBGM.onValueChanged.AddListener((val) =>
                {
                    if (SettingsService.Instance != null)
                    {
                        SettingsService.Instance.SetBGM(SettingsService.Instance.BGMEnabled, val);
                    }
                });
            }

            // VFX
            if (btnToggleVFX != null)
            {
                btnToggleVFX.onClick.RemoveAllListeners();
                btnToggleVFX.onClick.AddListener(() =>
                {
                    if (SettingsService.Instance != null)
                    {
                        SettingsService.Instance.SetVFX(!SettingsService.Instance.VFXEnabled);
                        RefreshUI();
                    }
                });
            }

            // Haptics
            if (btnToggleHaptics != null)
            {
                btnToggleHaptics.onClick.RemoveAllListeners();
                btnToggleHaptics.onClick.AddListener(() =>
                {
                    if (SettingsService.Instance != null)
                    {
                        SettingsService.Instance.SetHaptics(!SettingsService.Instance.HapticsEnabled);
                        RefreshUI();
                    }
                });
            }

            // Language
            if (btnLangVi != null)
            {
                btnLangVi.onClick.RemoveAllListeners();
                btnLangVi.onClick.AddListener(() =>
                {
                    LocalizationService.SetLanguage(GameLanguage.Vietnamese);
                    RefreshUI();
                });
            }

            if (btnLangEn != null)
            {
                btnLangEn.onClick.RemoveAllListeners();
                btnLangEn.onClick.AddListener(() =>
                {
                    LocalizationService.SetLanguage(GameLanguage.English);
                    RefreshUI();
                });
            }

            // Main Menu Navigation
            if (btnReturnMainMenu != null)
            {
                btnReturnMainMenu.onClick.RemoveAllListeners();
                btnReturnMainMenu.onClick.AddListener(() =>
                {
                    Close();
                    GameManager.Instance?.ReturnToMainMenu();
                });
            }

            // Reset Save
            if (btnResetData != null)
            {
                btnResetData.onClick.RemoveAllListeners();
                btnResetData.onClick.AddListener(ShowResetConfirm);
            }

            if (btnConfirmReset != null)
            {
                btnConfirmReset.onClick.RemoveAllListeners();
                btnConfirmReset.onClick.AddListener(DoResetSave);
            }

            if (btnCancelReset != null)
            {
                btnCancelReset.onClick.RemoveAllListeners();
                btnCancelReset.onClick.AddListener(HideResetConfirm);
            }

            LocalizationService.OnLanguageChanged += RefreshUI;

            HideResetConfirm();
            Close();
            RefreshUI();
        }

        private void OnDestroy()
        {
            LocalizationService.OnLanguageChanged -= RefreshUI;
        }

        public void Open()
        {
            if (panelSettings != null)
            {
                panelSettings.transform.SetAsLastSibling();
                panelSettings.SetActive(true);
                RefreshUI();
            }
        }

        public void Close()
        {
            if (panelSettings != null)
            {
                panelSettings.SetActive(false);
            }
            HideResetConfirm();
        }

        public bool IsOpen => panelSettings != null && panelSettings.activeSelf;

        public void RefreshUI()
        {
            // 1. Localize Strings
            if (txtTitle != null) txtTitle.text = LocalizationService.Get("SETTINGS_TITLE");
            if (txtOrientationLabel != null) txtOrientationLabel.text = LocalizationService.Get("SETTING_ORIENTATION");
            if (txtLandscape != null) txtLandscape.text = LocalizationService.Get("ORIENTATION_LANDSCAPE");
            if (txtPortrait != null) txtPortrait.text = LocalizationService.Get("ORIENTATION_PORTRAIT");
            if (txtSFXLabel != null) txtSFXLabel.text = LocalizationService.Get("SETTING_SFX");
            if (txtBGMLabel != null) txtBGMLabel.text = LocalizationService.Get("SETTING_BGM");
            if (txtVFXLabel != null) txtVFXLabel.text = LocalizationService.Get("SETTING_VFX");
            if (txtHapticsLabel != null) txtHapticsLabel.text = LocalizationService.Get("SETTING_HAPTICS");
            if (txtLangLabel != null) txtLangLabel.text = LocalizationService.Get("SETTING_LANG");
            if (txtLangVi != null) txtLangVi.text = LocalizationService.Get("LANG_VI");
            if (txtLangEn != null) txtLangEn.text = LocalizationService.Get("LANG_EN");
            if (txtReturnMainMenu != null) txtReturnMainMenu.text = LocalizationService.Get("MENU_BTN_RETURN_MAIN");
            if (txtResetData != null) txtResetData.text = LocalizationService.Get("BTN_RESET_SAVE");

            if (txtConfirmTitle != null) txtConfirmTitle.text = LocalizationService.Get("RESET_CONFIRM_TITLE");
            if (txtConfirmMsg != null) txtConfirmMsg.text = LocalizationService.Get("RESET_CONFIRM_MSG");
            if (txtConfirmReset != null) txtConfirmReset.text = LocalizationService.Get("BTN_CONFIRM");
            if (txtCancelReset != null) txtCancelReset.text = LocalizationService.Get("BTN_CANCEL");

            var settings = SettingsService.Instance;
            if (settings == null) return;

            // 2. Orientation buttons
            bool isLandscape = settings.CurrentOrientation == ScreenOrientationMode.Landscape;
            if (imgLandscape != null) imgLandscape.color = isLandscape ? COLOR_ACTIVE_BTN : COLOR_INACTIVE_BTN;
            if (imgPortrait != null) imgPortrait.color = !isLandscape ? COLOR_ACTIVE_BTN : COLOR_INACTIVE_BTN;

            // 3. Audio States
            if (txtSFXState != null)
            {
                txtSFXState.text = settings.SFXEnabled ? LocalizationService.Get("STATE_ON") : LocalizationService.Get("STATE_OFF");
                txtSFXState.color = settings.SFXEnabled ? COLOR_ON : COLOR_OFF;
            }
            if (sliderSFX != null) sliderSFX.value = settings.SFXVolume;

            if (txtBGMState != null)
            {
                txtBGMState.text = settings.BGMEnabled ? LocalizationService.Get("STATE_ON") : LocalizationService.Get("STATE_OFF");
                txtBGMState.color = settings.BGMEnabled ? COLOR_ON : COLOR_OFF;
            }
            if (sliderBGM != null) sliderBGM.value = settings.BGMVolume;

            // 4. VFX & Haptics States
            if (txtVFXState != null)
            {
                txtVFXState.text = settings.VFXEnabled ? LocalizationService.Get("STATE_ON") : LocalizationService.Get("STATE_OFF");
                txtVFXState.color = settings.VFXEnabled ? COLOR_ON : COLOR_OFF;
            }

            if (txtHapticsState != null)
            {
                txtHapticsState.text = settings.HapticsEnabled ? LocalizationService.Get("STATE_ON") : LocalizationService.Get("STATE_OFF");
                txtHapticsState.color = settings.HapticsEnabled ? COLOR_ON : COLOR_OFF;
            }

            // 5. Language buttons
            bool isVi = LocalizationService.CurrentLanguage == GameLanguage.Vietnamese;
            if (imgLangVi != null) imgLangVi.color = isVi ? COLOR_ACTIVE_BTN : COLOR_INACTIVE_BTN;
            if (imgLangEn != null) imgLangEn.color = !isVi ? COLOR_ACTIVE_BTN : COLOR_INACTIVE_BTN;
        }

        private void ShowResetConfirm()
        {
            if (panelResetConfirm != null) panelResetConfirm.SetActive(true);
        }

        private void HideResetConfirm()
        {
            if (panelResetConfirm != null) panelResetConfirm.SetActive(false);
        }

        private void DoResetSave()
        {
            HideResetConfirm();
            Close();
            SettingsService.Instance?.ResetGameSave();
        }
    }
}
