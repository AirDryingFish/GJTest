using UnityEngine;
using UnityEngine.SceneManagement;

namespace Yzz
{
    /// <summary>
    /// 通关点：挂到带 Trigger 的 Collider2D 物体上。当 Tag 为 "Player" 的物体进入 trigger 时，保存通关数据并加载下一关场景。
    /// 不触发时请检查：1) 本物体 Collider2D 勾选 Is Trigger  2) 至少一方有 Rigidbody2D（通常玩家有）
    /// 3) 玩家物体的 Tag 设为 "Player"
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class LevelFinishPoint : MonoBehaviour
    {
        [Tooltip("当前关卡数（1～3），通关后解锁下一关")]
        [SerializeField] private int levelIndex = 1;
        [Tooltip("下一关的场景名，需在 Build Settings 中勾选")]
        [SerializeField] private string nextLevelSceneName = "";

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;

            int nextLevel = Mathf.Clamp(levelIndex + 1, 1, 3);
            LevelProgress.UnlockNextLevel(nextLevel);

            if (!string.IsNullOrEmpty(nextLevelSceneName))
                SceneManager.LoadScene(nextLevelSceneName);
        }
    }
}
