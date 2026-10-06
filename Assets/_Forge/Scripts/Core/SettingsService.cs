using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Forge
{
    public enum ScreenOrientationMode
    {
        Landscape,
        Portrait
    }

    public class SettingsService : MonoBehaviour
    {
        public static SettingsService Instance { get; private set; }

        private const string PREF_ORIENTATION = "Forge_Orientation";
        private const string PREF_SFX_ENABLED = "Forge_SFX_Enabled";
        private const string PREF_SFX_VOL = "Forge_SFX_Vol";
        private const string PREF_BGM_ENABLED = "Forge_BGM_Enabled";
        private const string PREF_BGM_VOL = "Forge_BGM_Vol";
        private const string PREF_VFX_ENABLED = "Forge_VFX_Enabled";
        private const string PREF_HAPTICS_ENABLED = "Forge_Haptics_Enabled";

        public ScreenOrientationMode CurrentOrientation { get; private set; } = ScreenOrientationMode.Landscape;
        public bool SFXEnabled { get; private set; } = true;
        public float SFXVolume { get; private set; } = 1.0f;
        public bool BGMEnabled { get; private set; } = true;
        public float BGMVolume { get; private set; } = 0.4f;
        public bool VFXEnabled { get; private set; } = true;
        public bool HapticsEnabled { get; private set; } = true;

        public event Action<ScreenOrientationMode> OnOrientationChanged;
        public event Action OnSettingsChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            LocalizationService.Initialize();
            LoadSettings();
        }

        private void Start()
        {
            ApplySettings();
        }

        public void LoadSettings()
        {
            string ori = PlayerPrefs.GetString(PREF_ORIENTATION, "Landscape");
            CurrentOrientation = ori == "Portrait" ? ScreenOrientationMode.Portrait : ScreenOrientationMode.Landscape;

            SFXEnabled = PlayerPrefs.GetInt(PREF_SFX_ENABLED, 1) == 1;
            SFXVolume = PlayerPrefs.GetFloat(PREF_SFX_VOL, 1.0f);

            BGMEnabled = PlayerPrefs.GetInt(PREF_BGM_ENABLED, 1) == 1;
            BGMVolume = PlayerPrefs.GetFloat(PREF_BGM_VOL, 0.4f);

            VFXEnabled = PlayerPrefs.GetInt(PREF_VFX_ENABLED, 1) == 1;
            HapticsEnabled = PlayerPrefs.GetInt(PREF_HAPTICS_ENABLED, 1) == 1;
        }

        public void ApplySettings()
        {
            OnOrientationChanged?.Invoke(CurrentOrientation);
            OnSettingsChanged?.Invoke();
        }

        public void SetOrientation(ScreenOrientationMode mode)
        {
            CurrentOrientation = mode;
            PlayerPrefs.SetString(PREF_ORIENTATION, mode == ScreenOrientationMode.Portrait ? "Portrait" : "Landscape");
            PlayerPrefs.Save();
            OnOrientationChanged?.Invoke(mode);
            OnSettingsChanged?.Invoke();
        }

        public void SetSFX(bool enabled, float vol)
        {
            SFXEnabled = enabled;
            SFXVolume = Mathf.Clamp01(vol);
            PlayerPrefs.SetInt(PREF_SFX_ENABLED, SFXEnabled ? 1 : 0);
            PlayerPrefs.SetFloat(PREF_SFX_VOL, SFXVolume);
            PlayerPrefs.Save();
            OnSettingsChanged?.Invoke();
        }

        public void SetBGM(bool enabled, float vol)
        {
            BGMEnabled = enabled;
            BGMVolume = Mathf.Clamp01(vol);
            PlayerPrefs.SetInt(PREF_BGM_ENABLED, BGMEnabled ? 1 : 0);
            PlayerPrefs.SetFloat(PREF_BGM_VOL, BGMVolume);
            PlayerPrefs.Save();
            OnSettingsChanged?.Invoke();
        }

        public void SetVFX(bool enabled)
        {
            VFXEnabled = enabled;
            PlayerPrefs.SetInt(PREF_VFX_ENABLED, VFXEnabled ? 1 : 0);
            PlayerPrefs.Save();
            OnSettingsChanged?.Invoke();
        }

        public void SetHaptics(bool enabled)
        {
            HapticsEnabled = enabled;
            PlayerPrefs.SetInt(PREF_HAPTICS_ENABLED, HapticsEnabled ? 1 : 0);
            PlayerPrefs.Save();
            OnSettingsChanged?.Invoke();
        }

        public void TriggerHapticPunch()
        {
            if (!HapticsEnabled) return;
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }

        public void ResetGameSave()
        {
            Debug.Log("[SettingsService] Resetting all game save data...");
            if (GameManager.Instance != null && GameManager.Instance.saveService != null)
            {
                GameManager.Instance.saveService.DeleteSave();
            }
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
