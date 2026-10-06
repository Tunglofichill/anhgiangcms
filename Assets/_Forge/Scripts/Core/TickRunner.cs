using System.Collections;
using UnityEngine;

namespace Forge
{
    public class TickRunner : MonoBehaviour
    {
        [SerializeField] private float tickInterval = 0.1f;
        private Coroutine _tickCoroutine;

        public void StartTicking(float interval)
        {
            tickInterval = interval;
            if (_tickCoroutine != null)
            {
                StopCoroutine(_tickCoroutine);
            }
            _tickCoroutine = StartCoroutine(TickRoutine());
        }

        private IEnumerator TickRoutine()
        {
            var wait = new WaitForSeconds(tickInterval);
            while (true)
            {
                yield return wait;
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.Tick(tickInterval);
                }
            }
        }

        private void OnDisable()
        {
            if (_tickCoroutine != null)
            {
                StopCoroutine(_tickCoroutine);
                _tickCoroutine = null;
            }
        }
    }
}
