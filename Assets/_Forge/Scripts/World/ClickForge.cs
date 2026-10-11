using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Forge
{
    [RequireComponent(typeof(BoxCollider))]
    public class ClickForge : MonoBehaviour
    {
        [Header("References")]
        public CameraPunch cameraPunch;
        public ForgeLightController lightController;
        public AudioService audioService;
        public WorldView worldView;
        public FurnaceVisual furnaceVisual;

        private BoxCollider _boxCollider;
        private Camera _mainCamera;
        private int _lastClickFrame = -1;

        private void Awake()
        {
            _boxCollider = GetComponent<BoxCollider>();
            _mainCamera = Camera.main != null ? Camera.main : Object.FindAnyObjectByType<Camera>();
        }

        private void Update()
        {
            // Direct raycast click detection in Update - 100% responsive and accurate
            if (Input.GetMouseButtonDown(0))
            {
                TryClickRaycast(Input.mousePosition);
            }
        }

        private void TryClickRaycast(Vector3 screenPos)
        {
            if (MainMenuView.Instance != null && MainMenuView.Instance.IsOpen) return;
            if (IsPointerOverInteractiveUI()) return;

            if (_mainCamera == null)
            {
                _mainCamera = Camera.main != null ? Camera.main : Object.FindAnyObjectByType<Camera>();
            }
            if (_mainCamera == null) return;

            Ray ray = _mainCamera.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                if (hit.collider == _boxCollider || hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    HandleClick(hit.point);
                }
            }
        }

        private bool IsPointerOverInteractiveUI()
        {
            if (EventSystem.current == null) return false;

            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var result in results)
            {
                // Tutorial hints or decorative panels do NOT block clicks
                if (result.gameObject.GetComponentInParent<TutorialHintController>() != null)
                {
                    continue;
                }

                // Actual interactive UI buttons (Shop, Upgrade, Close) block click to prevent misclick
                if (result.gameObject.GetComponentInParent<UnityEngine.UI.Selectable>() != null)
                {
                    return true;
                }
            }
            return false;
        }

        private void OnMouseDown()
        {
            // Fallback for standard Unity mouse event
            if (IsPointerOverInteractiveUI()) return;
            HandleClick(transform.position + Vector3.up * 0.45f);
        }

        public void HandleClick(Vector3 hitPos)
        {
            // Frame guard prevents accidental duplicate triggers in the same frame
            if (Time.frameCount == _lastClickFrame) return;
            _lastClickFrame = Time.frameCount;

            double tapPower = 1.0;
            bool isCrit = false;
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.economyService != null)
                {
                    tapPower = GameManager.Instance.economyService.CalculateFinalTapPower(out isCrit);
                }
                GameManager.Instance.OnFurnaceClicked(tapPower);
            }

            // Visual feedback
            if (furnaceVisual != null) furnaceVisual.PlayTapPunch();
            if (cameraPunch != null) cameraPunch.Punch();
            if (lightController != null) lightController.Pulse();
            if (audioService != null) audioService.PlayHit();
            if (worldView != null) worldView.PlayHitSpark(hitPos);

            // Floating Numbers (+1 Ore or CRIT)
            if (FloatingTextSpawner.Instance != null)
            {
                FloatingTextSpawner.Instance.Spawn(hitPos, tapPower, isCrit);
            }

            // Tutorial Hint progression
            if (TutorialHintController.Instance != null)
            {
                TutorialHintController.Instance.OnPlayerTappedFurnace();
            }
        }
    }
}
