using UnityEngine;
using System.Collections.Generic;

namespace Yzz
{
    /// <summary>
    /// 物块绕指定点的2D旋转脚本。
    /// 挂到物块上，指定旋转中心（Transform）。
    /// 旋转半径自动计算为物块到旋转中心的初始距离。
    /// 参考 MoveBetweenPoints 的参数设计逻辑。
    /// </summary>
    public class RotateAroundPoint : MonoBehaviour
    {
        [Header("Rotation Center")]
        [SerializeField] private Transform rotationCenter;
        [Tooltip("如果为空，则旋转中心为该 Transform 的位置")]
        [SerializeField] private bool useLocalCenter = false;

        [Header("Rotation")]
        [SerializeField] private float rotationSpeed = 90f; // 度/秒
        [SerializeField] private bool clockwise = true;

        [Header("Feel")]
        [Tooltip("旋转是否循环（true=一直转，false=来回转）")]
        [SerializeField] private bool isLooping = true;
        [Tooltip("如果不循环，设定旋转的角度范围（0-360）")]
        [SerializeField] private float rotationRange = 180f;

        private float _currentAngle = 0f;
        private float _radius;
        private Vector3 _centerPosition;
        private float _direction = 1f;
        private List<Rigidbody2D> _trackedObjects = new List<Rigidbody2D>();
        private Vector3 _lastPosition;
        private Vector3 _platformVelocity;

        private void Start()
        {
            if (rotationCenter == null)
            {
                // 如果没指定旋转中心，则使用该脚本所在物体的位置
                rotationCenter = transform;
            }

            _centerPosition = rotationCenter.position;
            _lastPosition = transform.position;

            // 计算初始旋转半径：物块到旋转中心的距离
            Vector3 diff = transform.position - _centerPosition;
            _radius = diff.magnitude;

            // 计算初始角度
            _currentAngle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;

            if (!clockwise) _direction = -1f;
        }

        private void FixedUpdate()
        {
            if (rotationCenter == null || _radius <= 0f) return;

            _centerPosition = rotationCenter.position;

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

            // 3. 根据角度计算新的位置
            float angleRad = _currentAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(
                Mathf.Cos(angleRad) * _radius,
                Mathf.Sin(angleRad) * _radius,
                0f
            );
            Vector3 targetPos = _centerPosition + offset;

            // 4. 计算本帧位移向量
            _platformVelocity = targetPos - transform.position;

            // 5. 移动物块
            transform.position = targetPos;

            // 6. 带动附着的对象（类似 MoveBetweenPoints）
            foreach (var rb in _trackedObjects)
            {
                if (rb != null)
                {
                    rb.position += (Vector2)_platformVelocity;
                }
            }
        }

        // --- 碰撞检测逻辑：检测上面的对象 ---

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // 判断碰撞点是否在物块上方
            if (collision.contactCount > 0 && collision.contacts[0].normal.y < -0.5f)
            {
                Rigidbody2D rb = collision.collider.GetComponent<Rigidbody2D>();
                if (rb != null && !_trackedObjects.Contains(rb))
                {
                    _trackedObjects.Add(rb);
                }
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            Rigidbody2D rb = collision.collider.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                _trackedObjects.Remove(rb);
            }
        }

        // --- 公开方法 ---

        /// <summary>
        /// 改变旋转中心
        /// </summary>
        public void SetRotationCenter(Transform newCenter)
        {
            if (newCenter != null)
            {
                rotationCenter = newCenter;
                _centerPosition = rotationCenter.position;
            }
        }

        /// <summary>
        /// 改变旋转速度（度/秒）
        /// </summary>
        public void SetRotationSpeed(float newSpeed)
        {
            rotationSpeed = newSpeed;
        }

        /// <summary>
        /// 获取当前旋转角度
        /// </summary>
        public float GetCurrentAngle()
        {
            return _currentAngle;
        }

        /// <summary>
        /// 获取旋转半径
        /// </summary>
        public float GetRadius()
        {
            return _radius;
        }
    }
}
