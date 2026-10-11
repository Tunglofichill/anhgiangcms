using System.Collections;
using UnityEngine;

namespace Forge
{
    public class HammerVisual3D : MonoBehaviour
    {
        public static HammerVisual3D Instance { get; private set; }

        [Header("Animation Settings")]
        public Transform hammerPivot;
        public Vector3 restLocalPosition = Vector3.zero;
        public Vector3 restLocalEuler = new Vector3(0f, 0f, 0f);
        public Vector3 strikeLocalEuler = new Vector3(-65f, 15f, -10f);

        private Coroutine _strikeRoutine;
        private Vector3 _initialLocalPos;
        private Quaternion _initialLocalRot;

        private void Awake()
        {
            Instance = this;
            if (hammerPivot == null) hammerPivot = transform;
            _initialLocalPos = hammerPivot.localPosition;
            _initialLocalRot = hammerPivot.localRotation;
        }

        private void Update()
        {
            // Subtle idle breathing motion
            if (_strikeRoutine == null && hammerPivot != null)
            {
                float breathe = Mathf.Sin(Time.time * 2.0f) * 0.005f;
                hammerPivot.localPosition = _initialLocalPos + new Vector3(0f, breathe, 0f);
            }
        }

        public void PlayStrike()
        {
            if (_strikeRoutine != null)
            {
                StopCoroutine(_strikeRoutine);
            }
            _strikeRoutine = StartCoroutine(StrikeRoutine());
        }

        private IEnumerator StrikeRoutine()
        {
            Quaternion startRot = hammerPivot.localRotation;
            Quaternion windupRot = Quaternion.Euler(restLocalEuler + new Vector3(25f, -10f, 5f));
            Quaternion strikeRot = Quaternion.Euler(strikeLocalEuler);

            // 1. Wind up (0.04s)
            float t = 0f;
            float windupTime = 0.04f;
            while (t < windupTime)
            {
                t += Time.unscaledDeltaTime;
                hammerPivot.localRotation = Quaternion.Slerp(startRot, windupRot, t / windupTime);
                yield return null;
            }

            // 2. Smash down fast (0.05s)
            t = 0f;
            float strikeTime = 0.05f;
            while (t < strikeTime)
            {
                t += Time.unscaledDeltaTime;
                hammerPivot.localRotation = Quaternion.Slerp(windupRot, strikeRot, t / strikeTime);
                yield return null;
            }

            // 3. Rebound bounce (0.08s)
            Quaternion reboundRot = Quaternion.Euler(restLocalEuler + new Vector3(8f, 0f, 0f));
            t = 0f;
            float reboundTime = 0.08f;
            while (t < reboundTime)
            {
                t += Time.unscaledDeltaTime;
                hammerPivot.localRotation = Quaternion.Slerp(strikeRot, reboundRot, t / reboundTime);
                yield return null;
            }

            // 4. Return to rest (0.12s)
            t = 0f;
            float returnTime = 0.12f;
            while (t < returnTime)
            {
                t += Time.unscaledDeltaTime;
                hammerPivot.localRotation = Quaternion.Slerp(reboundRot, _initialLocalRot, t / returnTime);
                yield return null;
            }

            hammerPivot.localRotation = _initialLocalRot;
            hammerPivot.localPosition = _initialLocalPos;
            _strikeRoutine = null;
        }
    }
}
