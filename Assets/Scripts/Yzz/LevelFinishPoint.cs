using System;
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
        public Transform[] keeps;


        public Vector3 curEnd;
        public Vector3 nextStart;
        public DraggableMask curMask;
        private Vector3 curMaskPos;
        private Vector3 curMaskEuler;
        private Vector3 curMaskScale;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;

            int nextLevel = Mathf.Clamp(levelIndex + 1, 1, 3);
            LevelProgress.UnlockNextLevel(nextLevel);

            if (!string.IsNullOrEmpty(nextLevelSceneName))
                foreach (var k in keeps)
                {
                    DontDestroyOnLoad(k);
                }
                recordMask();
                SceneManager.sceneLoaded += OnSceneLoad;
                SceneManager.LoadScene(nextLevelSceneName);
            
        }

        private void recordMask()
        {
            curMaskPos = curMask.transform.position;
            curMaskEuler = curMask.transform.eulerAngles;
            curMaskScale = curMask.transform.localScale;
        }

        private void syncMask()
        {
            DraggableMask nextmask = FindFirstObjectByType<DraggableMask>();
            nextmask.transform.position = curMaskPos;
            nextmask.transform.eulerAngles = curMaskEuler;
            nextmask.transform.localScale = curMaskScale;
        }

        private void OnSceneLoad(Scene s, LoadSceneMode m)
        {
            SceneManager.sceneLoaded -= OnSceneLoad;
            syncMask();
            foreach (var item in keeps)
            {
                item.transform.position +=  nextStart-curEnd;
            }
        }

    }
}
