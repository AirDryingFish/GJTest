using UnityEngine;

namespace Yzz
{
    /// <summary>
    /// 物块绕自身Z轴旋转的脚本。
    /// 改变 rotation.z，让物块自旋。
    /// 参考 MoveBetweenPoints 和 RotateAroundPoint 的参数设计逻辑。
    /// </summary>
    public class RotateAroundZ : MonoBehaviour
    {
        [Header("Rotation")]
        [SerializeField] private float rotationSpeed = 180f; // 度/秒
        [SerializeField] private bool clockwise = true;

        [Header("Feel")]
        [Tooltip("旋转是否循环（true=一直转，false=来回转）")]
        [SerializeField] private bool isLooping = true;
        [Tooltip("如果不循环，设定旋转的角度范围（0-360）")]
        [SerializeField] private float rotationRange = 180f;

        private float _currentAngle = 0f;
        private float _direction = 1f;

        private void Start()
        {
            // 获取初始旋转角度
            _currentAngle = transform.eulerAngles.z;

            if (!clockwise) _direction = -1f;
        }

        private void FixedUpdate()
        {
            // 1. 更新旋转角度
            float angularVelocity = rotationSpeed * _direction * Time.fixedDeltaTime;
            _currentAngle += angularVelocity;

            // 2. 如果不循环，限制角度范围
            if (!isLooping)
            {
                if (_currentAngle >= rotationRange)
                {
                    _currentAngle = rotationRange;
                    _direction = -1f;
                }
                if (_currentAngle <= 0f)
                {
                    _currentAngle = 0f;
                    _direction = 1f;
                }
            }
            else
            {
                // 循环模式下，正常化角度到 0-360
                _currentAngle = _currentAngle % 360f;
            }

            // 3. 应用旋转到物体
            Vector3 eulerAngles = transform.eulerAngles;
            eulerAngles.z = _currentAngle;
            transform.eulerAngles = eulerAngles;
        }

        // --- 公开方法 ---

        /// <summary>
        /// 改变旋转速度（度/秒）
        /// </summary>
        public void SetRotationSpeed(float newSpeed)
        {
            rotationSpeed = newSpeed;
        }

        /// <summary>
        /// 改变旋转方向
        /// </summary>
        public void SetClockwise(bool newClockwise)
        {
            _direction = newClockwise ? 1f : -1f;
            clockwise = newClockwise;
        }

        /// <summary>
        /// 获取当前旋转角度
        /// </summary>
        public float GetCurrentAngle()
        {
            return _currentAngle;
        }

        /// <summary>
        /// 设置当前旋转角度
        /// </summary>
        public void SetCurrentAngle(float newAngle)
        {
            _currentAngle = newAngle;
            Vector3 eulerAngles = transform.eulerAngles;
            eulerAngles.z = _currentAngle;
            transform.eulerAngles = eulerAngles;
        }
    }
}