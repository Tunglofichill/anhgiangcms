using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Forge
{
    public class OrientationManager : MonoBehaviour
    {
        public static OrientationManager Instance { get; private set; }

        [Header("Camera Rig")]
        public Camera mainCamera;
        public Transform cameraTransform;

        [Header("Canvas & Scaler")]
        public CanvasScaler canvasScaler;

        [Header("UI RectTransforms to Adapt")]
        public RectTransform rtCardCurrency;
        public RectTransform rtCardRank;
        public RectTransform rtBtnSettings;
        public RectTransform rtBtnShop;
        public RectTransform rtTutorialHint;
        public RectTransform rtPanelShop;
        public RectTransform rtPanelSettings;

        private Coroutine _cameraTransitionRoutine;
        private ScreenOrientationMode _currentMode = ScreenOrientationMode.Landscape;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (mainCamera == null) mainCamera = Camera.main != null ? Camera.main : Object.FindAnyObjectByType<Camera>();
            if (cameraTransform == null && mainCamera != null) cameraTransform = mainCamera.transform;
        }

        private void Start()
        {
            if (SettingsService.Instance != null)
            {
                SettingsService.Instance.OnOrientationChanged += ApplyOrientation;
                ApplyOrientation(SettingsService.Instance.CurrentOrientation);
            }
            else
            {
                ApplyOrientation(ScreenOrientationMode.Landscape);
            }
        }

        private void OnDestroy()
        {
            if (SettingsService.Instance != null)
            {
                SettingsService.Instance.OnOrientationChanged -= ApplyOrientation;
            }
        }

        public void ApplyOrientation(ScreenOrientationMode mode)
        {
            _currentMode = mode;

            if (mode == ScreenOrientationMode.Landscape)
            {
#if UNITY_ANDROID || UNITY_IOS
                Screen.orientation = ScreenOrientation.LandscapeLeft;
#endif
                ApplyLandscapeLayout();
            }
            else
            {
#if UNITY_ANDROID || UNITY_IOS
                Screen.orientation = ScreenOrientation.Portrait;
#endif
                ApplyPortraitLayout();
            }
        }

        private void ApplyLandscapeLayout()
        {
            // 1. Camera framing for 16:9
            Vector3 targetCamPos = new Vector3(0.00f, 2.15f, -2.55f);
            Quaternion targetCamRot = Quaternion.Euler(28f, 0f, 0f);
            float targetFov = 38f;

            if (cameraTransform != null)
            {
                if (_cameraTransitionRoutine != null) StopCoroutine(_cameraTransitionRoutine);
                _cameraTransitionRoutine = StartCoroutine(TransitionCamera(targetCamPos, targetCamRot, targetFov));
            }

            // 2. Canvas Scaler for Landscape
            if (canvasScaler != null)
            {
                canvasScaler.referenceResolution = new Vector2(1920, 1080);
                canvasScaler.matchWidthOrHeight = 0.5f;
            }

            // 3. UI Placement
            if (rtCardCurrency != null)
            {
                rtCardCurrency.anchorMin = new Vector2(0f, 1f);
                rtCardCurrency.anchorMax = new Vector2(0f, 1f);
                rtCardCurrency.pivot = new Vector2(0f, 1f);
                rtCardCurrency.anchoredPosition = new Vector2(35f, -35f);
                rtCardCurrency.sizeDelta = new Vector2(340f, 85f);
            }

            if (rtCardRank != null)
            {
                rtCardRank.anchorMin = new Vector2(1f, 1f);
                rtCardRank.anchorMax = new Vector2(1f, 1f);
                rtCardRank.pivot = new Vector2(1f, 1f);
                rtCardRank.anchoredPosition = new Vector2(-105f, -35f);
                rtCardRank.sizeDelta = new Vector2(280f, 55f);
            }

            if (rtBtnSettings != null)
            {
                rtBtnSettings.anchorMin = new Vector2(1f, 1f);
                rtBtnSettings.anchorMax = new Vector2(1f, 1f);
                rtBtnSettings.pivot = new Vector2(1f, 1f);
                rtBtnSettings.anchoredPosition = new Vector2(-35f, -35f);
                rtBtnSettings.sizeDelta = new Vector2(55f, 55f);
            }

            if (rtBtnShop != null)
            {
                rtBtnShop.anchorMin = new Vector2(0.5f, 0f);
                rtBtnShop.anchorMax = new Vector2(0.5f, 0f);
                rtBtnShop.pivot = new Vector2(0.5f, 0f);
                rtBtnShop.anchoredPosition = new Vector2(0f, 35f);
                rtBtnShop.sizeDelta = new Vector2(300f, 65f);
            }

            if (rtTutorialHint != null)
            {
                rtTutorialHint.anchorMin = new Vector2(0.5f, 0f);
                rtTutorialHint.anchorMax = new Vector2(0.5f, 0f);
                rtTutorialHint.pivot = new Vector2(0.5f, 0f);
                rtTutorialHint.anchoredPosition = new Vector2(0f, 115f);
            }

            if (rtPanelShop != null)
            {
                rtPanelShop.anchorMin = new Vector2(0.5f, 0f);
                rtPanelShop.anchorMax = new Vector2(0.5f, 0f);
                rtPanelShop.pivot = new Vector2(0.5f, 0f);
                rtPanelShop.anchoredPosition = new Vector2(0f, 115f);
                rtPanelShop.sizeDelta = new Vector2(1150f, 480f);
            }

            if (rtPanelSettings != null)
            {
                rtPanelSettings.anchorMin = new Vector2(0.5f, 0.5f);
                rtPanelSettings.anchorMax = new Vector2(0.5f, 0.5f);
                rtPanelSettings.pivot = new Vector2(0.5f, 0.5f);
                rtPanelSettings.anchoredPosition = Vector2.zero;
                rtPanelSettings.sizeDelta = new Vector2(860f, 520f);
            }
        }

        private void ApplyPortraitLayout()
        {
            // 1. Camera framing pulled back for vertical 9:16 aspect
            Vector3 targetCamPos = new Vector3(0.00f, 3.10f, -4.85f);
            Quaternion targetCamRot = Quaternion.Euler(26f, 0f, 0f);
            float targetFov = 42f;

            if (cameraTransform != null)
            {
                if (_cameraTransitionRoutine != null) StopCoroutine(_cameraTransitionRoutine);
                _cameraTransitionRoutine = StartCoroutine(TransitionCamera(targetCamPos, targetCamRot, targetFov));
            }

            // 2. Canvas Scaler for Portrait
            if (canvasScaler != null)
            {
                canvasScaler.referenceResolution = new Vector2(1080, 1920);
                canvasScaler.matchWidthOrHeight = 0.5f;
            }

            // 3. UI Placement for Mobile Portrait
            if (rtCardCurrency != null)
            {
                rtCardCurrency.anchorMin = new Vector2(0.5f, 1f);
                rtCardCurrency.anchorMax = new Vector2(0.5f, 1f);
                rtCardCurrency.pivot = new Vector2(0.5f, 1f);
                rtCardCurrency.anchoredPosition = new Vector2(0f, -45f);
                rtCardCurrency.sizeDelta = new Vector2(660f, 95f);
            }

            if (rtCardRank != null)
            {
                rtCardRank.anchorMin = new Vector2(0.5f, 1f);
                rtCardRank.anchorMax = new Vector2(0.5f, 1f);
                rtCardRank.pivot = new Vector2(0.5f, 1f);
                rtCardRank.anchoredPosition = new Vector2(0f, -155f);
                rtCardRank.sizeDelta = new Vector2(440f, 55f);
            }

            if (rtBtnSettings != null)
            {
                rtBtnSettings.anchorMin = new Vector2(1f, 1f);
                rtBtnSettings.anchorMax = new Vector2(1f, 1f);
                rtBtnSettings.pivot = new Vector2(1f, 1f);
                rtBtnSettings.anchoredPosition = new Vector2(-40f, -45f);
                rtBtnSettings.sizeDelta = new Vector2(65f, 65f);
            }

            if (rtBtnShop != null)
            {
                rtBtnShop.anchorMin = new Vector2(0.5f, 0f);
                rtBtnShop.anchorMax = new Vector2(0.5f, 0f);
                rtBtnShop.pivot = new Vector2(0.5f, 0f);
                rtBtnShop.anchoredPosition = new Vector2(0f, 60f);
                rtBtnShop.sizeDelta = new Vector2(460f, 85f);
            }

            if (rtTutorialHint != null)
            {
                rtTutorialHint.anchorMin = new Vector2(0.5f, 0f);
                rtTutorialHint.anchorMax = new Vector2(0.5f, 0f);
                rtTutorialHint.pivot = new Vector2(0.5f, 0f);
                rtTutorialHint.anchoredPosition = new Vector2(0f, 200f);
            }

            if (rtPanelShop != null)
            {
                rtPanelShop.anchorMin = new Vector2(0.5f, 0f);
                rtPanelShop.anchorMax = new Vector2(0.5f, 0f);
                rtPanelShop.pivot = new Vector2(0.5f, 0f);
                rtPanelShop.anchoredPosition = new Vector2(0f, 160f);
                rtPanelShop.sizeDelta = new Vector2(980f, 780f);
            }

            if (rtPanelSettings != null)
            {
                rtPanelSettings.anchorMin = new Vector2(0.5f, 0.5f);
                rtPanelSettings.anchorMax = new Vector2(0.5f, 0.5f);
                rtPanelSettings.pivot = new Vector2(0.5f, 0.5f);
                rtPanelSettings.anchoredPosition = Vector2.zero;
                rtPanelSettings.sizeDelta = new Vector2(940f, 960f);
            }
        }

        private IEnumerator TransitionCamera(Vector3 targetPos, Quaternion targetRot, float targetFov)
        {
            if (cameraTransform == null) yield break;

            Vector3 startPos = cameraTransform.position;
            Quaternion startRot = cameraTransform.rotation;
            float startFov = mainCamera != null ? mainCamera.fieldOfView : targetFov;

            float elapsed = 0f;
            float duration = 0.35f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

                cameraTransform.position = Vector3.Lerp(startPos, targetPos, t);
                cameraTransform.rotation = Quaternion.Slerp(startRot, targetRot, t);
                if (mainCamera != null) mainCamera.fieldOfView = Mathf.Lerp(startFov, targetFov, t);

                yield return null;
            }

            cameraTransform.position = targetPos;
            cameraTransform.rotation = targetRot;
            if (mainCamera != null) mainCamera.fieldOfView = targetFov;
            _cameraTransitionRoutine = null;
        }
    }
}
