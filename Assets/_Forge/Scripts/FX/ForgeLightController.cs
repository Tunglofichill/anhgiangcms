using System.Collections;
using UnityEngine;

namespace Forge
{
    public class ForgeLightController : MonoBehaviour
    {
        [Header("Furnace Point Light")]
        public Light furnacePointLight;
        public float defaultBaseIntensity = 1.8f;
        public float upgradedBaseIntensity = 2.4f;

        [Header("Decor Lamp (M1)")]
        public Light lampPointLight;

        private float _currentBaseIntensity = 1.8f;
        private Coroutine _pulseCoroutine;

        private void Awake()
        {
            _currentBaseIntensity = defaultBaseIntensity;
            if (furnacePointLight != null)
            {
                furnacePointLight.intensity = _currentBaseIntensity;
            }
        }

        public void SetG3Active(bool active)
        {
            _currentBaseIntensity = active ? upgradedBaseIntensity : defaultBaseIntensity;
            if (_pulseCoroutine == null && furnacePointLight != null)
            {
                furnacePointLight.intensity = _currentBaseIntensity;
            }
        }

        public void UpdateFurnaceLevel(int furnaceLevel)
        {
            float bonus = (Mathf.Max(1, furnaceLevel) - 1) * 0.22f;
            _currentBaseIntensity = defaultBaseIntensity + bonus;
            if (furnacePointLight != null)
            {
                furnacePointLight.range = 2.2f + (furnaceLevel - 1) * 0.12f;
                if (_pulseCoroutine == null)
                {
                    furnacePointLight.intensity = _currentBaseIntensity;
                }
            }
        }

        public void Pulse()
        {
            if (furnacePointLight == null) return;
            if (_pulseCoroutine != null)
            {
                StopCoroutine(_pulseCoroutine);
            }
            _pulseCoroutine = StartCoroutine(PulseRoutine());
        }

        private IEnumerator PulseRoutine()
        {
            float targetIntensity = _currentBaseIntensity * 1.8f;
            float duration = 0.08f;
            float elapsed = 0f;

            // Flash up to target
            furnacePointLight.intensity = targetIntensity;

            // Lerp back down to base
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                furnacePointLight.intensity = Mathf.Lerp(targetIntensity, _currentBaseIntensity, elapsed / duration);
                yield return null;
            }

            furnacePointLight.intensity = _currentBaseIntensity;
            _pulseCoroutine = null;
        }
    }
}
