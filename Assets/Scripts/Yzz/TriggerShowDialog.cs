using UnityEngine;

namespace Yzz
{
    /// <summary>
    /// 挂到带 Trigger 的 Collider2D 物体上；有物体进入 trigger 时调用 IngameUIConqtroller.ShowDialog()。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class TriggerShowDialog : MonoBehaviour
    {
        [Tooltip("若为空则场景中查找")]
        [SerializeField] private GameObject uiprefab;
        private IngameUIConqtroller uiController;

        private void Awake()
        {
            
            if (uiController == null)
                uiController = FindObjectOfType<IngameUIConqtroller>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (uiController == null) return;
            uiController.ShowDialog();
        }
    }
}