using UnityEngine;

namespace Yzz
{
    /// <summary>
    /// 挂到要移动的物体（如 square）上，传入两个 GameObject 作为路径点，物体在两点之间左右来回循环移动。
    /// </summary>
    public class MoveBetweenPoints : MonoBehaviour
    {
        [Header("Path Points")]
        [Tooltip("路径点 A")]
        [SerializeField] private Transform pointA;
        [Tooltip("路径点 B")]
        [SerializeField] private Transform pointB;

        [Header("Movement")]
        [Tooltip("移动速度（世界单位/秒）")]
        [SerializeField] private float speed = 2f;
        [Tooltip("勾选则匀速往返；不勾选则两端慢、中间快（SmoothStep）")]
        [SerializeField] private bool linear = true;

        private float _t;
        private float _duration;
        private float _direction = 1f;

        private void Start()
        {
            RefreshDuration();
        }

        private void OnValidate()
        {
            RefreshDuration();
        }

        private void RefreshDuration()
        {
            if (pointA != null && pointB != null && speed > 0f)
            {
                float dist = Vector3.Distance(new Vector3(pointA.position.x, transform.position.y, transform.position.z), new Vector3(pointB.position.x, transform.position.y, transform.position.z));
                _duration = dist / speed;
            }
            else
            {
                _duration = 1f;
            }
        }

        private void Update()
        {
            if (pointA == null || pointB == null) return;
            if (_duration <= 0f) return;

            _t += _direction * (Time.deltaTime / _duration);
            if (_t >= 1f) { _t = 1f; _direction = -1f; }
            if (_t <= 0f) { _t = 0f; _direction = 1f; }

            float s = linear ? _t : Mathf.SmoothStep(0f, 1f, _t);
            Vector3 pos = Vector3.Lerp(new Vector3(pointA.position.x, transform.position.y, transform.position.z), new Vector3(pointB.position.x, transform.position.y, transform.position.z), s);
            pos.z = transform.position.z;
            transform.position = pos;
        }

        private void OnDrawGizmosSelected()
        {
            if (pointA != null && pointB != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(pointA.position, pointB.position);
            }
        }
    }
}
