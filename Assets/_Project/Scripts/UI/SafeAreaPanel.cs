// Source: D-15 + RESEARCH §Pattern 4 (UI Canvas with Safe Area).
// Updates RectTransform anchors to Screen.safeArea each time the safe-area or
// resolution changes. Drop on a child of UICanvas with anchors stretched 0..1.
//
// Self-healing offsets: Apply() also resets offsetMin/offsetMax to Vector2.zero,
// so the runtime contract "rect equals Screen.safeArea" holds regardless of any
// edit-time RectTransform offset values. Operators do NOT need to manually set
// offsetMin/offsetMax to (0,0) in the Inspector — Apply() enforces it every frame
// the safe area changes.
using UnityEngine;

namespace WM.UI
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaPanel : MonoBehaviour
    {
        private RectTransform _rt;
        private Rect _lastApplied;
        private Vector2Int _lastScreen;

        private void Awake() => _rt = GetComponent<RectTransform>();

        private void OnEnable() => Apply(Screen.safeArea);

        private void Update()
        {
            if (Screen.safeArea != _lastApplied ||
                Screen.width != _lastScreen.x || Screen.height != _lastScreen.y)
            {
                Apply(Screen.safeArea);
            }
        }

        private void Apply(Rect safeArea)
        {
            if (safeArea == _lastApplied) return;
            if (Screen.width == 0 || Screen.height == 0) return;

            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= Screen.width;  anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;  anchorMax.y /= Screen.height;

            _rt.anchorMin = anchorMin;
            _rt.anchorMax = anchorMax;
            _rt.offsetMin = Vector2.zero; // self-healing: enforce rect == safeArea
            _rt.offsetMax = Vector2.zero;

            _lastApplied = safeArea;
            _lastScreen = new Vector2Int(Screen.width, Screen.height);
        }
    }
}
