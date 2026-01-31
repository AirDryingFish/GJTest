using UnityEngine;
using UnityEngine.SceneManagement;

namespace Yzz
{
    /// <summary>
    /// 通关点：挂到带 Trigger 的 Collider2D 物体上。当 Layer 为 "w1" 的物体进入 trigger 时，保存通关数据并加载下一关场景。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class LevelFinishPoint : MonoBehaviour
    {
        [Tooltip("当前关卡数（1～3），通关后解锁下一关")]
        [SerializeField] private int levelIndex = 1;
        [Tooltip("下一关的场景名，需在 Build Settings 中勾选")]
        [SerializeField] private string nextLevelSceneName = "";

        private static readonly int W1Layer = LayerMask.NameToLayer("w1");

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (W1Layer < 0)
                return;
            if (other.gameObject.layer != W1Layer)
                return;

            int nextLevel = Mathf.Clamp(levelIndex + 1, 1, 3);
            LevelProgress.UnlockNextLevel(nextLevel);

            if (!string.IsNullOrEmpty(nextLevelSceneName))
                SceneManager.LoadScene(nextLevelSceneName);
        }
    }
}
