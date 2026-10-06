using UnityEngine;

namespace Forge
{
    public class AudioService : MonoBehaviour
    {
        public static AudioService Instance { get; private set; }

        [Header("Audio Sources")]
        public AudioSource sfxSource;
        public AudioSource bgmSource;

        [Header("Audio Clips")]
        public AudioClip hitClip;
        public AudioClip buyClip;
        public AudioClip unlockClip;
        public AudioClip bgmClip;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }

            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
                bgmSource.loop = true;
                bgmSource.volume = 0.25f; // ~ -12dB
                bgmSource.playOnAwake = false;
            }

            // Procedural fallback clips if none assigned
            if (hitClip == null) hitClip = GenerateProceduralHitClip();
            if (buyClip == null) buyClip = GenerateProceduralBuyClip();
            if (unlockClip == null) unlockClip = GenerateProceduralUnlockClip();
            if (bgmClip == null) bgmClip = GenerateProceduralBgmClip();

            if (bgmClip != null && !bgmSource.isPlaying)
            {
                bgmSource.clip = bgmClip;
                bgmSource.Play();
            }

            if (SettingsService.Instance != null)
            {
                SettingsService.Instance.OnSettingsChanged += UpdateAudioSettings;
                UpdateAudioSettings();
            }
        }

        private void OnDestroy()
        {
            if (SettingsService.Instance != null)
            {
                SettingsService.Instance.OnSettingsChanged -= UpdateAudioSettings;
            }
        }

        public void UpdateAudioSettings()
        {
            var settings = SettingsService.Instance;
            if (settings == null) return;

            if (bgmSource != null)
            {
                bgmSource.mute = !settings.BGMEnabled;
                bgmSource.volume = 0.25f * settings.BGMVolume;
                if (settings.BGMEnabled && !bgmSource.isPlaying && bgmClip != null)
                {
                    bgmSource.Play();
                }
            }

            if (sfxSource != null)
            {
                sfxSource.mute = !settings.SFXEnabled;
            }
        }

        public void PlayHit()
        {
            if (sfxSource == null || hitClip == null) return;
            if (SettingsService.Instance != null && !SettingsService.Instance.SFXEnabled) return;
            float vol = SettingsService.Instance != null ? SettingsService.Instance.SFXVolume : 1.0f;
            sfxSource.pitch = Random.Range(0.92f, 1.08f);
            sfxSource.PlayOneShot(hitClip, 0.8f * vol);
        }

        public void PlayBuy()
        {
            if (sfxSource == null || buyClip == null) return;
            if (SettingsService.Instance != null && !SettingsService.Instance.SFXEnabled) return;
            float vol = SettingsService.Instance != null ? SettingsService.Instance.SFXVolume : 1.0f;
            sfxSource.pitch = 1.0f;
            sfxSource.PlayOneShot(buyClip, 0.9f * vol);
        }

        public void PlayUnlock()
        {
            if (sfxSource == null || unlockClip == null) return;
            if (SettingsService.Instance != null && !SettingsService.Instance.SFXEnabled) return;
            float vol = SettingsService.Instance != null ? SettingsService.Instance.SFXVolume : 1.0f;
            sfxSource.pitch = 1.05f;
            sfxSource.PlayOneShot(unlockClip, 1.0f * vol);
        }

        // Procedural audio generators for zero-dependency standalone operation
        private AudioClip GenerateProceduralHitClip()
        {
            int sampleRate = 44100;
            int sampleCount = (int)(sampleRate * 0.15f);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float decay = Mathf.Exp(-t * 28f);
                float f1 = Mathf.Sin(2f * Mathf.PI * 880f * t);
                float f2 = Mathf.Sin(2f * Mathf.PI * 1320f * t);
                samples[i] = (f1 * 0.6f + f2 * 0.4f) * decay;
            }

            AudioClip clip = AudioClip.Create("Procedural_Hit", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateProceduralBuyClip()
        {
            int sampleRate = 44100;
            int sampleCount = (int)(sampleRate * 0.22f);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float decay = Mathf.Exp(-t * 14f);
                float freq = t < 0.1f ? 523.25f : 659.25f; // C5 to E5
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * decay * 0.6f;
            }

            AudioClip clip = AudioClip.Create("Procedural_Buy", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateProceduralUnlockClip()
        {
            int sampleRate = 44100;
            int sampleCount = (int)(sampleRate * 0.45f);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float decay = Mathf.Exp(-t * 7f);
                float freq = 440f + (t * 880f); // Rising pitch
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * decay * 0.5f;
            }

            AudioClip clip = AudioClip.Create("Procedural_Unlock", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateProceduralBgmClip()
        {
            int sampleRate = 44100;
            int sampleCount = sampleRate * 4; // 4s seamless ambient drone
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float drone1 = Mathf.Sin(2f * Mathf.PI * 110f * t) * 0.15f; // A2
                float drone2 = Mathf.Sin(2f * Mathf.PI * 164.81f * t) * 0.10f; // E3
                float pulse = 0.8f + 0.2f * Mathf.Sin(2f * Mathf.PI * 0.5f * t);
                samples[i] = (drone1 + drone2) * pulse;
            }

            AudioClip clip = AudioClip.Create("Procedural_Bgm", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
