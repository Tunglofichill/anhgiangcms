using System;
using System.Globalization;
using UnityEngine;

namespace Forge
{
    public class OfflineService : MonoBehaviour
    {
        public delegate void OfflineProgressDelegate(string durationStr, double gain);
        public event OfflineProgressDelegate OnOfflineProgressCalculated;

        public double CalculateOfflineProgress(
            string lastSaveUtc,
            double dpsAtLoad,
            float maxOfflineSeconds,
            out string formattedDuration,
            out double gain)
        {
            formattedDuration = "";
            gain = 0.0;

            if (string.IsNullOrEmpty(lastSaveUtc))
            {
                return 0.0;
            }

            if (!DateTime.TryParse(lastSaveUtc, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime lastUtc))
            {
                return 0.0;
            }

            DateTime nowUtc = DateTime.UtcNow;
            double rawSeconds = (nowUtc - lastUtc).TotalSeconds;

            if (rawSeconds < 5.0)
            {
                // Under 5 seconds, do not show popup
                return 0.0;
            }

            float elapsed = Mathf.Clamp((float)rawSeconds, 0f, maxOfflineSeconds);
            gain = dpsAtLoad * elapsed;

            TimeSpan timeSpan = TimeSpan.FromSeconds(elapsed);
            if (timeSpan.TotalHours >= 1.0)
            {
                formattedDuration = LocalizationService.Get("TIME_HOURS_MINUTES", (int)timeSpan.TotalHours, timeSpan.Minutes);
            }
            else if (timeSpan.TotalMinutes >= 1.0)
            {
                formattedDuration = LocalizationService.Get("TIME_MINUTES_SECONDS", timeSpan.Minutes, timeSpan.Seconds);
            }
            else
            {
                formattedDuration = LocalizationService.Get("TIME_SECONDS", timeSpan.Seconds);
            }

            return gain;
        }

        public void NotifyOfflinePopup(string durationStr, double gain)
        {
            if (gain > 0.0)
            {
                OnOfflineProgressCalculated?.Invoke(durationStr, gain);
            }
        }
    }
}
