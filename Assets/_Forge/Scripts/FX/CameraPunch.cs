using System.Collections;
using UnityEngine;

namespace Forge
{
    public class CameraPunch : MonoBehaviour
    {
        private Vector3 _originalLocalPos;
        private Coroutine _punchCoroutine;

        private void Awake()
        {
            _originalLocalPos = transform.localPosition;
        }

        public void Punch()
        {
            // Camera shake is disabled for 100% stable, smooth view
            return;
        }

        private IEnumerator PunchRoutine()
        {
            Vector3 offset = Random.insideUnitSphere * 0.018f;
            Vector3 targetPos = _originalLocalPos + offset;

            // Punch out in 0.05s
            float punchDuration = 0.05f;
            float elapsed = 0f;
            while (elapsed < punchDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                transform.localPosition = Vector3.Lerp(_originalLocalPos, targetPos, elapsed / punchDuration);
                yield return null;
            }

            // Lerp back in 0.12s
            float returnDuration = 0.12f;
            elapsed = 0f;
            while (elapsed < returnDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                transform.localPosition = Vector3.Lerp(targetPos, _originalLocalPos, elapsed / returnDuration);
                yield return null;
            }

            transform.localPosition = _originalLocalPos;
            _punchCoroutine = null;
        }
    }
}
