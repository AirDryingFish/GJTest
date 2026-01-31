using UnityEngine;

namespace Yzz
{
    /// <summary>
    /// 关卡进度：用 JSON 存玩家解锁到第几关（1～3，第 1 关默认解锁）。
    /// 通关后可在关卡内调用 LevelProgress.UnlockNextLevel() 或 SetUnlockedLevel(n)。
    /// </summary>
    public static class LevelProgress
    {
        private const string Key = "levelProgress";

        [System.Serializable]
        public class Data
        {
            public int unlockedLevel = 1;
        }

        public static int GetUnlockedLevel()
        {
            if (SaveSystem.TryLoad<Data>(Key, out var data))
                return Mathf.Clamp(data.unlockedLevel, 1, 3);
            return 1;
        }

        public static void SetUnlockedLevel(int level)
        {
            int clamped = Mathf.Clamp(level, 1, 3);
            SaveSystem.Save(Key, new Data { unlockedLevel = clamped });
        }

        /// <summary>
        /// 通关当前关后调用，解锁下一关（例如在关卡 1 结束时传 2）。
        /// </summary>
        public static void UnlockNextLevel(int nextLevel)
        {
            int current = GetUnlockedLevel();
            if (nextLevel > current)
                SetUnlockedLevel(nextLevel);
        }
    }
}
