using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Forge
{
    public class FloatingTextSpawner : MonoBehaviour
    {
        public static FloatingTextSpawner Instance { get; private set; }

        [Header("References")]
        public RectTransform canvasRectTransform;
        public Camera worldCamera;
        public Sprite oreIconSprite;
        public Font font;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                if (transform.parent != null || GetComponent<Canvas>() != null)
                {
                    Destroy(this);
                }
                else
                {
                    Destroy(gameObject);
                }
                return;
            }
            Instance = this;

            if (worldCamera == null)
            {
                worldCamera = Camera.main != null ? Camera.main : Object.FindFirstObjectByType<Camera>();
            }
        }

        public void Spawn(Vector3 worldPosition, double amount)
        {
            if (canvasRectTransform == null)
            {
                Canvas c = GetComponentInParent<Canvas>();
                if (c != null) canvasRectTransform = c.GetComponent<RectTransform>();
            }

            if (worldCamera == null)
            {
                worldCamera = Camera.main != null ? Camera.main : Object.FindFirstObjectByType<Camera>();
            }

            // Convert world pos to screen pos
            Vector3 screenPos = worldCamera != null 
                ? worldCamera.WorldToScreenPoint(worldPosition) 
                : new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);

            // Jitter X slightly for natural fountain effect
            screenPos.x += Random.Range(-35f, 35f);
            screenPos.y += Random.Range(10f, 30f);

            StartCoroutine(FloatingItemRoutine(screenPos, amount));
        }

        private IEnumerator FloatingItemRoutine(Vector3 startScreenPos, double amount)
        {
            // Create root container
            GameObject go = new GameObject("FloatingTextItem", typeof(RectTransform), typeof(CanvasGroup));
            go.transform.SetParent(transform, false);

            RectTransform rt = go.GetComponent<RectTransform>();
            CanvasGroup cg = go.GetComponent<CanvasGroup>();
            rt.position = startScreenPos;
            rt.sizeDelta = new Vector2(240f, 60f);

            // Layout Horizontal
            HorizontalLayoutGroup hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 8f;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // Icon
            if (oreIconSprite != null)
            {
                GameObject goIcon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                goIcon.transform.SetParent(go.transform, false);
                RectTransform rtIcon = goIcon.GetComponent<RectTransform>();
                rtIcon.sizeDelta = new Vector2(36f, 36f);
                Image img = goIcon.GetComponent<Image>();
                img.sprite = oreIconSprite;
                img.preserveAspect = true;
            }

            // Text
            GameObject goTxt = new GameObject("Text", typeof(RectTransform), typeof(Text), typeof(Outline));
            goTxt.transform.SetParent(go.transform, false);
            RectTransform rtTxt = goTxt.GetComponent<RectTransform>();
            rtTxt.sizeDelta = new Vector2(160f, 48f);

            Text txt = goTxt.GetComponent<Text>();
            Font chosenFont = font;
            if (chosenFont == null)
            {
                try { chosenFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
                if (chosenFont == null)
                {
                    try { chosenFont = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { }
                }
                if (chosenFont == null)
                {
                    try { chosenFont = Font.CreateDynamicFontFromOSFont("Arial", 32); } catch { }
                }
            }
            txt.font = chosenFont;
            txt.fontSize = 32;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleLeft;
            txt.color = new Color(1.0f, 0.88f, 0.35f, 1.0f); // Bright gold
            txt.text = $"+{HudView.FormatNumber(amount)}";

            Outline outline = goTxt.GetComponent<Outline>();
            outline.effectColor = new Color(0.12f, 0.08f, 0.02f, 0.95f);
            outline.effectDistance = new Vector2(2f, -2f);

            // Animation
            float duration = 0.75f;
            float elapsed = 0f;
            Vector3 startPos = rt.position;
            Vector3 targetPos = startPos + new Vector3(0f, 95f, 0f);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Ease out position
                float easeOut = 1f - Mathf.Pow(1f - t, 3f);
                rt.position = Vector3.Lerp(startPos, targetPos, easeOut);

                // Punch scale (pop up to 1.25 then settle)
                float scale = 1f;
                if (t < 0.2f)
                {
                    scale = Mathf.Lerp(0.6f, 1.25f, t / 0.2f);
                }
                else if (t < 0.4f)
                {
                    scale = Mathf.Lerp(1.25f, 1.0f, (t - 0.2f) / 0.2f);
                }
                rt.localScale = Vector3.one * scale;

                // Fade alpha during last 40%
                if (t > 0.6f)
                {
                    cg.alpha = Mathf.Lerp(1f, 0f, (t - 0.6f) / 0.4f);
                }
                else
                {
                    cg.alpha = 1f;
                }

                yield return null;
            }

            Destroy(go);
        }
    }
}
