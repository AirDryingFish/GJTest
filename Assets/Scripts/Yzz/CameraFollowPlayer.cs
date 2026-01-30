using UnityEngine;

namespace Yzz
{
    /// <summary>
    /// 2D 相机根据玩家移动意图跟随：往右走时相机目标偏右（多看到右侧），往左走时偏左，不固定死区。
    /// 挂到 Main Camera 上，Target 需带 Rigidbody2D 以读取速度方向。
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraFollowPlayer : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        [Tooltip("若未指定 Target，会尝试用 Tag 查找")]
        [SerializeField] private string targetTag = "Player";

        [Header("Lead (根据移动意图偏移)")]
        [Tooltip("水平方向：按移动方向偏移目标，0=不偏，约 0.2~0.4 较自然")]
        [Range(0f, 0.6f)]
        [SerializeField] private float leadAmountX = 0.3f;
        [Tooltip("垂直方向：向上/下落时略微前导，0=不偏")]
        [Range(0f, 0.4f)]
        [SerializeField] private float leadAmountY = 0.15f;
        [Tooltip("速度超过此值才应用前导，避免站立微抖")]
        [SerializeField] private float minSpeedForLead = 0.5f;
        [Tooltip("前导偏移的平滑时间，越小越跟手")]
        [SerializeField] private float leadSmoothTime = 0.08f;

        [Header("Follow")]
        [SerializeField] private Vector2 offset = new Vector2(0f, 0f);
        [Tooltip("相机位置平滑时间；越小越跟手，约 0.08~0.12 不易落后")]
        [SerializeField] private float smoothTime = 0.1f;
        [Tooltip(">0 时限制相机最大跟随速度；0 不限制")]
        [SerializeField] private float maxFollowSpeed = 0f;

        private Camera _cam;
        private Rigidbody2D _targetRb;
        private Vector3 _velocity;
        private float _currentLeadX;
        private float _currentLeadY;
        private float _leadVelX;
        private float _leadVelY;
        private int _lastMoveDirX; // 上一帧水平方向：-1/0/1，用于检测突然反向

         private void Start()
        {
            _cam = GetComponent<Camera>();
            if (target == null && !string.IsNullOrEmpty(targetTag))
            {
                var go = GameObject.FindWithTag(targetTag);
                if (go != null) target = go.transform;
            }
            if (target != null)
                _targetRb = target.GetComponent<Rigidbody2D>();
        }

        private void LateUpdate()
        {
            if (target == null) return;

            float halfW = 0f;
            float halfH = 0f;
            if (_cam != null && _cam.orthographic)
            {
                halfH = _cam.orthographicSize;
                halfW = halfH * _cam.aspect;
            }
            else
            {
                halfW = 5f;
                halfH = 5f;
            }

            float targetLeadX = 0f;
            float targetLeadY = 0f;
            int moveDirX = 0;
            if (_targetRb != null)
            {
                Vector2 v = _targetRb.velocity;
                if (Mathf.Abs(v.x) > minSpeedForLead)
                {
                    moveDirX = (int)Mathf.Sign(v.x);
                    targetLeadX = moveDirX * leadAmountX * halfW;
                }
                if (Mathf.Abs(v.y) > minSpeedForLead)
                    targetLeadY = Mathf.Sign(v.y) * leadAmountY * halfH;
            }

            // 突然反向（右→左或左→右）时立刻清零水平前导，相机目标回到玩家附近，避免始终慢一拍
            if (moveDirX != 0 && _lastMoveDirX != 0 && moveDirX != _lastMoveDirX)
            {
                _currentLeadX = 0f;
                _leadVelX = 0f;
            }
            _lastMoveDirX = moveDirX;

            _currentLeadX = Mathf.SmoothDamp(_currentLeadX, targetLeadX, ref _leadVelX, leadSmoothTime);
            _currentLeadY = Mathf.SmoothDamp(_currentLeadY, targetLeadY, ref _leadVelY, leadSmoothTime);

            Vector3 desired = new Vector3(
                target.position.x + offset.x + _currentLeadX,
                target.position.y + offset.y + _currentLeadY,
                transform.position.z
            );

            float maxSpeed = maxFollowSpeed > 0f ? maxFollowSpeed : float.MaxValue;
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desired,
                ref _velocity,
                smoothTime,
                maxSpeed
            );
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            _targetRb = target != null ? target.GetComponent<Rigidbody2D>() : null;
        }
    }
}
