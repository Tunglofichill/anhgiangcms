using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Forge
{
    public class TutorialHintController : MonoBehaviour
    {
        public static TutorialHintController Instance { get; private set; }

        [Header("UI Elements")]
        public RectTransform hintContainer;
        public CanvasGroup canvasGroup;
        public RectTransform handIconRect;
        public Text txtInstruction;

        [Header("Settings")]
        public int requiredTapsToDismiss = 3;

        private int _tapCount = 0;
        private bool _isDismissed = false;
        private Vector3 _originalHandPos;
        private Coroutine _fadeCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (handIconRect != null)
            {
                _originalHandPos = handIconRect.anchoredPosition;
            }

            LocalizationService.OnLanguageChanged += UpdateHintText;
            UpdateHintText();
        }

        private void OnDestroy()
        {
            LocalizationService.OnLanguageChanged -= UpdateHintText;
        }

        private void UpdateHintText()
        {
            if (txtInstruction != null && !_isDismissed)
            {
                if (_tapCount == 0)
                {
                    txtInstruction.text = LocalizationService.Get("TUTORIAL_INITIAL");
                }
                else
                {
                    int remaining = requiredTapsToDismiss - _tapCount;
                    txtInstruction.text = LocalizationService.Get("TUTORIAL_PROGRESS", remaining);
                }
            }
        }

        private void Update()
        {
            if (_isDismissed || handIconRect == null) return;

            // Bobbing bounce animation for the hand pointer
            float bobOffset = Mathf.Sin(Time.unscaledTime * 5.5f) * 14f;
            handIconRect.anchoredPosition = _originalHandPos + new Vector3(0f, bobOffset, 0f);
        }

        public void OnPlayerTappedFurnace()
        {
            if (_isDismissed) return;

            _tapCount++;

            // Visual feedback on tap: quick punch
            if (handIconRect != null)
            {
                handIconRect.localScale = Vector3.one * 1.35f;
                StartCoroutine(PunchHandRoutine());
            }

            if (_tapCount >= requiredTapsToDismiss)
            {
                Dismiss();
            }
            else
            {
                UpdateHintText();
            }
        }

        private IEnumerator PunchHandRoutine()
        {
            float elapsed = 0f;
            float dur = 0.15f;
            while (elapsed < dur)
            {
                elapsed += Time.unscaledDeltaTime;
                if (handIconRect != null)
                {
                    handIconRect.localScale = Vector3.Lerp(Vector3.one * 1.35f, Vector3.one, elapsed / dur);
                }
                yield return null;
            }
            if (handIconRect != null) handIconRect.localScale = Vector3.one;
        }

        public void Dismiss()
        {
            if (_isDismissed) return;
            _isDismissed = true;

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeOutRoutine());
        }

        private IEnumerator FadeOutRoutine()
        {
            if (canvasGroup == null)
            {
                if (hintContainer != null) hintContainer.gameObject.SetActive(false);
                yield break;
            }

            float dur = 0.45f;
            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;

            while (elapsed < dur)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / dur);
                yield return null;
            }

            canvasGroup.alpha = 0f;
            if (hintContainer != null)
            {
                hintContainer.gameObject.SetActive(false);
            }
        }
    }
}
