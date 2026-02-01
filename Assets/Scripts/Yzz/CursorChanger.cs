using UnityEngine;

namespace Yzz
{
    /// <summary>
    /// 运行游戏时把鼠标指针换成指定图片。挂到场景里任意常驻物体（如 Canvas 或 DontDestroyOnLoad 的物体）上，
    /// 在 Inspector 里指定 Cursor 图片和热点位置即可。
    /// </summary>
    public class CursorChanger : MonoBehaviour
    {
        [Header("指针图片")]
        [Tooltip("指针贴图，建议尺寸 32×32 或 64×64，带透明通道；也可拖 Sprite")]
        [SerializeField] private Texture2D cursorTexture;
        [Tooltip("若用 Sprite 可拖这里，会优先于上面的 Texture2D")]
        [SerializeField] private Sprite cursorSprite;

        [Header("热点（点击对准的点，单位：像素）")]
        [Tooltip("例如 (0,0)=左上角，(16,16)=32×32 图中心")]
        [SerializeField] private Vector2 hotspot = new Vector2(0f, 0f);

        [Header("可选")]
        [Tooltip("勾选可在部分平台上提高自定义指针兼容性")]
        [SerializeField] private bool forceSoftwareCursor = false;

        private void Awake()
        {
            ApplyCursor();
        }

        private void OnEnable()
        {
            ApplyCursor();
        }

        private void OnDisable()
        {
            // 可选：离开时恢复系统默认指针
            // Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        /// <summary> 使用当前 Inspector 设置应用自定义指针。 </summary>
        public void ApplyCursor()
        {
            Texture2D tex = cursorTexture;
            if (tex == null && cursorSprite != null)
                tex = cursorSprite.texture;

            if (tex == null)
                return;

            CursorMode mode = forceSoftwareCursor ? CursorMode.ForceSoftware : CursorMode.Auto;
            Cursor.SetCursor(tex, hotspot, mode);
        }

        /// <summary> 恢复系统默认指针。 </summary>
        public void ResetToDefault()
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
}
