using UnityEngine;

namespace Yzz
{
    /// <summary>
    /// 挂到 Canvas 下的 RectTransform 节点上，让该节点在屏幕上跟随一个世界坐标的 Transform（如 2D 角色）。
    /// 用于对话框等 UI 跟随 2D 物体。
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class UIFollowWorldTarget : MonoBehaviour
    {
        [Tooltip("要跟随的世界坐标物体（如玩家、NPC）")]
        [SerializeField] private Transform worldTarget;
        [Tooltip("屏幕上的偏移")]
        [SerializeField] private Vector2 screenOffset = Vector2.zero;

        private RectTransform _rect;
        private Canvas _canvas;
        private Camera _cam;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
            _cam = _canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? _canvas.worldCamera
                : Camera.main;
        }

        private void LateUpdate()
        {
            if (worldTarget == null || _rect == null || _canvas == null) return;
            if (_cam == null) _cam = Camera.main;
            if (_cam == null) return;

            Vector3 worldPos = worldTarget.position;
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(_cam, worldPos);
            screenPoint += screenOffset;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.GetComponent<RectTransform>(), screenPoint, _cam, out Vector2 localPoint))
            {
                _rect.anchoredPosition = localPoint;
            }
        }

        /// <summary> 设置要跟随的目标，可为 null 停止跟随。 </summary>
        public void SetTarget(Transform target)
        {
            worldTarget = target;
        }
    }
}
