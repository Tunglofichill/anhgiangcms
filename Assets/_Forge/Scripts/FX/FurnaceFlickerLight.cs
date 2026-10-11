using UnityEngine;

namespace Forge.FX
{
    /// <summary>
    /// Simulates a realistic warm hearth flame flicker inside the 3D stone furnace.
    /// Uses smooth multi-octave Perlin noise for natural intensity and subtle color shifts.
    /// </summary>
    public class FurnaceFlickerLight : MonoBehaviour
    {
        [Header("Light Reference")]
        public Light targetLight;

        [Header("Flicker Settings")]
        public float minIntensity = 1.6f;
        public float maxIntensity = 2.4f;
        public float flickerSpeed = 3.5f;

        [Header("Micro Jitter")]
        public float jitterAmount = 0.15f;
        public float jitterSpeed = 18f;

        [Header("Color Warmth Variation")]
        public Color deepOrange = new Color(1.0f, 0.42f, 0.08f);
        public Color blazingGold = new Color(1.0f, 0.68f, 0.22f);

        private float _baseIntensity = 2.0f;
        private Vector3 _initialLocalPos;
        private float _timeSeed;

        private void Awake()
        {
            if (targetLight == null) targetLight = GetComponent<Light>();
            if (targetLight != null) _baseIntensity = targetLight.intensity;
            _initialLocalPos = transform.localPosition;
            _timeSeed = Random.Range(0f, 100f);
        }

        private void Update()
        {
            if (targetLight == null) return;

            float t = Time.time * flickerSpeed + _timeSeed;
            float slowWave = Mathf.PerlinNoise(t, 0f);
            float fastWave = Mathf.PerlinNoise(Time.time * jitterSpeed + _timeSeed * 2f, 0f) * jitterAmount;

            float intensityFactor = Mathf.Clamp01(slowWave + fastWave);
            targetLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, intensityFactor);

            // Subtle color temperature breathing
            targetLight.color = Color.Lerp(deepOrange, blazingGold, slowWave);

            // Subtle positional flame dance
            float offsetX = (Mathf.PerlinNoise(t * 1.5f, 5f) - 0.5f) * 0.04f;
            float offsetY = (Mathf.PerlinNoise(t * 1.5f, 15f) - 0.5f) * 0.03f;
            transform.localPosition = _initialLocalPos + new Vector3(offsetX, offsetY, 0f);
        }

        public void SetBaseIntensity(float newBase)
        {
            _baseIntensity = newBase;
            minIntensity = _baseIntensity * 0.8f;
            maxIntensity = _baseIntensity * 1.25f;
        }
    }
}
