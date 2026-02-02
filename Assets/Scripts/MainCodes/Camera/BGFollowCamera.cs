using UnityEngine;

namespace Yzz
{
    /// <summary>
    /// 背景跟着相机移动（位置同步 + 视差），适合 SpriteRenderer Draw Mode = Tiled、Size 拉大的无限背景。
    /// 不碰 UV，不抖、不漂移；相机怎么动都不会露边。
    /// </summary>
    public class BGFollowCamera : MonoBehaviour
    {
        [Header("Camera")]
        [Tooltip("不指定则用 Main Camera")]
        [SerializeField] private Camera cam;

        [Header("Parallax")]
        [Tooltip("0=远景几乎不动，1=和相机 1:1 跟随")]
        [Range(0f, 1f)]
        [SerializeField] private float parallaxX = 0.3f;
        [Range(0f, 1f)]
        [SerializeField] private float parallaxY = 0.2f;

        [Header("Optional: 起始偏移")]
        [Tooltip("背景相对世界的初始偏移，一般 (0,0,0) 即可")]
        [SerializeField] private Vector3 startOffset = Vector3.zero;

        private Vector3 _startPos;

        private void Start()
        {
            if (cam == null) cam = Camera.main;
            _startPos = transform.position + startOffset;
        }

        private void LateUpdate()
        {
            if (cam == null) return;

            Vector3 camPos = cam.transform.position;
            transform.position = new Vector3(
                _startPos.x + camPos.x * parallaxX,
                _startPos.y + camPos.y * parallaxY,
                _startPos.z
            );
        }
    }
}
