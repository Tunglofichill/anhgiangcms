using UnityEngine;

namespace Forge
{
    public class CameraParallax3D : MonoBehaviour
    {
        [Header("Parallax Settings")]
        [Tooltip("Max angle rotation in degrees")]
        public float maxPitch = 2.5f;
        public float maxYaw = 3.5f;
        [Tooltip("Max position shift in units")]
        public float maxShiftX = 0.08f;
        public float maxShiftY = 0.05f;
        [Tooltip("Smoothing speed")]
        public float smoothSpeed = 4.0f;

        private Vector3 _initialLocalPos;
        private Quaternion _initialLocalRot;

        private void Awake()
        {
            _initialLocalPos = transform.localPosition;
            _initialLocalRot = transform.localRotation;
        }

        private void Update()
        {
            if (SettingsService.Instance != null && !SettingsService.Instance.VFXEnabled)
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, _initialLocalPos, Time.unscaledDeltaTime * smoothSpeed);
                transform.localRotation = Quaternion.Slerp(transform.localRotation, _initialLocalRot, Time.unscaledDeltaTime * smoothSpeed);
                return;
            }

            // Normalized screen coordinates (-1 to +1)
            float mouseNormX = (Input.mousePosition.x / Screen.width - 0.5f) * 2f;
            float mouseNormY = (Input.mousePosition.y / Screen.height - 0.5f) * 2f;

            mouseNormX = Mathf.Clamp(mouseNormX, -1f, 1f);
            mouseNormY = Mathf.Clamp(mouseNormY, -1f, 1f);

            // Target offset position & rotation
            Vector3 targetPos = _initialLocalPos + new Vector3(mouseNormX * maxShiftX, mouseNormY * maxShiftY, 0f);
            Quaternion targetRot = _initialLocalRot * Quaternion.Euler(-mouseNormY * maxPitch, mouseNormX * maxYaw, 0f);

            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.unscaledDeltaTime * smoothSpeed);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, Time.unscaledDeltaTime * smoothSpeed);
        }
    }
}
