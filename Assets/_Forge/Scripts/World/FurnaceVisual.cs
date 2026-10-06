using System.Collections;
using UnityEngine;

namespace Forge
{
    public class FurnaceVisual : MonoBehaviour
    {
        [Header("Visual Components")]
        public SpriteRenderer spriteRenderer;
        public Transform visualTransform;

        private Vector3 _baseScale = Vector3.one;
        private Coroutine _punchCoroutine;

        private void Awake()
        {
            if (visualTransform == null) visualTransform = transform;
            _baseScale = visualTransform.localScale;
        }

        private void Update()
        {
            // Subtle idle breathing pulse
            if (_punchCoroutine == null && visualTransform != null)
            {
                float breath = 1f + Mathf.Sin(Time.time * 2.2f) * 0.015f;
                visualTransform.localScale = new Vector3(_baseScale.x * breath, _baseScale.y * (2f - breath), _baseScale.z);
            }
        }

        public void PlayTapPunch()
        {
            if (_punchCoroutine != null) StopCoroutine(_punchCoroutine);
            _punchCoroutine = StartCoroutine(PunchRoutine());
        }

        private IEnumerator PunchRoutine()
        {
            // 1. Squash: wider and shorter
            Vector3 squashScale = new Vector3(_baseScale.x * 1.18f, _baseScale.y * 0.85f, _baseScale.z);
            // 2. Stretch: narrower and taller
            Vector3 stretchScale = new Vector3(_baseScale.x * 0.90f, _baseScale.y * 1.12f, _baseScale.z);

            float t = 0f;
            float durSquash = 0.05f;
            while (t < durSquash)
            {
                t += Time.unscaledDeltaTime;
                visualTransform.localScale = Vector3.Lerp(_baseScale, squashScale, t / durSquash);
                yield return null;
            }

            t = 0f;
            float durStretch = 0.08f;
            while (t < durStretch)
            {
                t += Time.unscaledDeltaTime;
                visualTransform.localScale = Vector3.Lerp(squashScale, stretchScale, t / durStretch);
                yield return null;
            }

            t = 0f;
            float durSettle = 0.10f;
            while (t < durSettle)
            {
                t += Time.unscaledDeltaTime;
                visualTransform.localScale = Vector3.Lerp(stretchScale, _baseScale, t / durSettle);
                yield return null;
            }

            visualTransform.localScale = _baseScale;
            _punchCoroutine = null;
        }
    }
}
