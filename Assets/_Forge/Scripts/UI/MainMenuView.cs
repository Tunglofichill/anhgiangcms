using System;
using UnityEngine;
using UnityEngine.UI;

namespace Forge
{
    public class MainMenuView : MonoBehaviour
    {
        public static MainMenuView Instance { get; private set; }

        private const string STUDIO_URL = "https://pawfoliostudio.vercel.app/";

        [Header("Main Menu Root")]
        public GameObject panelMainMenu;

        [Header("Title & Branding")]
        public Text txtGameTitle;
        public Text txtTagline;
        public Text txtVersion;
        public Text txtProgressInfo;

        [Header("Main Actions")]
        public Button btnPlayOrContinue;
        public Text txtBtnPlay;
        public Button btnSettings;
        public Text txtBtnSettings;
        public Button btnCredits;
        public Text txtBtnCredits;
        public Button btnQuit;
        public Text txtBtnQuit;

        [Header("Footer Credit")]
        public Text txtFooterCredit;
        public Button btnFooterLink;

        [Header("Credits / About Modal")]
        public GameObject panelCreditsModal;
        public Text txtCreditsTitle;
        public Text txtCreditsBody;
        public Button btnVisitStudio;
        public Text txtBtnVisitStudio;
        public Button btnCloseCredits;

        private Action _onPlayRequested;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Initialize(Action onPlay)
        {
            _onPlayRequested = onPlay;

            if (btnPlayOrContinue != null)
            {
                btnPlayOrContinue.onClick.RemoveAllListeners();
                btnPlayOrContinue.onClick.AddListener(OnPlayClicked);
            }

            if (btnSettings != null)
            {
                btnSettings.onClick.RemoveAllListeners();
                btnSettings.onClick.AddListener(OnSettingsClicked);
            }

            if (btnCredits != null)
            {
                btnCredits.onClick.RemoveAllListeners();
                btnCredits.onClick.AddListener(ShowCredits);
            }

            if (btnQuit != null)
            {
                btnQuit.onClick.RemoveAllListeners();
                btnQuit.onClick.AddListener(OnQuitClicked);
            }

            if (btnFooterLink != null)
            {
                btnFooterLink.onClick.RemoveAllListeners();
                btnFooterLink.onClick.AddListener(OpenStudioWebsite);
            }

            if (btnVisitStudio != null)
            {
                btnVisitStudio.onClick.RemoveAllListeners();
                btnVisitStudio.onClick.AddListener(OpenStudioWebsite);
            }

            if (btnCloseCredits != null)
            {
                btnCloseCredits.onClick.RemoveAllListeners();
                btnCloseCredits.onClick.AddListener(HideCredits);
            }

            LocalizationService.OnLanguageChanged += RefreshUI;

            HideCredits();
            RefreshUI();
        }

        private void OnDestroy()
        {
            LocalizationService.OnLanguageChanged -= RefreshUI;
        }

        public bool IsOpen => panelMainMenu != null && panelMainMenu.activeSelf;

        public void Open()
        {
            if (panelMainMenu != null)
            {
                panelMainMenu.SetActive(true);
            }
            HideCredits();
            RefreshUI();
        }

        public void Close()
        {
            if (panelMainMenu != null)
            {
                panelMainMenu.SetActive(false);
            }
            HideCredits();
        }

        public void RefreshUI()
        {
            // 1. Static Localized Texts
            if (txtGameTitle != null) txtGameTitle.text = LocalizationService.Get("MENU_TITLE");
            if (txtTagline != null) txtTagline.text = LocalizationService.Get("MENU_TAGLINE");
            if (txtVersion != null) txtVersion.text = LocalizationService.Get("MENU_VERSION");
            if (txtBtnSettings != null) txtBtnSettings.text = LocalizationService.Get("MENU_BTN_SETTINGS");
            if (txtBtnQuit != null) txtBtnQuit.text = LocalizationService.Get("MENU_BTN_QUIT");

            // User requested to remove Credits button from 3-button stack
            if (btnCredits != null)
            {
                btnCredits.gameObject.SetActive(false);
            }
            if (panelCreditsModal != null)
            {
                panelCreditsModal.SetActive(false);
            }

            // 2. Main Title Button: Always "BẮT ĐẦU CHƠI" / "START GAME" (True Main Menu)
            if (txtBtnPlay != null)
            {
                txtBtnPlay.text = LocalizationService.Get("MENU_BTN_PLAY");
            }

            // Hide progress info box and footer credit if present
            if (txtProgressInfo != null)
            {
                txtProgressInfo.gameObject.SetActive(false);
            }
            if (txtFooterCredit != null)
            {
                txtFooterCredit.gameObject.SetActive(false);
            }
            if (btnFooterLink != null)
            {
                btnFooterLink.gameObject.SetActive(false);
            }
        }

        private void OnPlayClicked()
        {
            PlayMenuClickSound();
            Close();
            _onPlayRequested?.Invoke();
        }

        private void OnSettingsClicked()
        {
            PlayMenuClickSound();
            var sv = SettingsView.Instance != null ? SettingsView.Instance : FindAnyObjectByType<SettingsView>(FindObjectsInactive.Include);
            if (sv != null)
            {
                sv.Open();
            }
        }

        public void ShowCredits()
        {
            if (panelCreditsModal != null)
            {
                panelCreditsModal.SetActive(true);
            }
        }

        public void HideCredits()
        {
            if (panelCreditsModal != null)
            {
                panelCreditsModal.SetActive(false);
            }
        }

        public void OpenStudioWebsite()
        {
            Application.OpenURL(STUDIO_URL);
        }

        private void OnQuitClicked()
        {
            PlayMenuClickSound();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void PlayMenuClickSound()
        {
            if (AudioService.Instance != null)
            {
                AudioService.Instance.PlayHit();
            }
            else if (GameManager.Instance != null && GameManager.Instance.audioService != null)
            {
                GameManager.Instance.audioService.PlayHit();
            }
        }
    }
}
