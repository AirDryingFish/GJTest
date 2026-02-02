using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Yzz
{
    /// <summary>
    /// 关卡选择界面：读 JSON 进度，第 1 关默认解锁；未解锁的关卡显示锁图并不可点击，解锁后点击 Button 进入对应场景。共 3 关。
    /// </summary>
    public class LevelSelectController : MonoBehaviour
    {
        [System.Serializable]
        public class LevelEntry
        {
            [Tooltip("该关卡的按钮，解锁后可点击进入场景")]
            public Button button;
            [Tooltip("该关卡对应的场景名，需在 Build Settings 中勾选")]
            public string sceneName;
            [Tooltip("未解锁时设为可见的锁图（一个 Image）；解锁后隐藏")]
            public Image lockImage;
        }

        [Tooltip("共 3 关，按顺序：第 1 关默认解锁")]
        [SerializeField] private LevelEntry[] levels = new LevelEntry[3];

        private void Start()
        {
            int unlockedLevel = LevelProgress.GetUnlockedLevel();

            for (int i = 0; i < levels.Length && i < 3; i++)
            {
                LevelEntry entry = levels[i];
                int levelIndex = i + 1; // 1-based
                bool unlocked = levelIndex <= unlockedLevel;

                if (entry.lockImage != null)
                    entry.lockImage.gameObject.SetActive(!unlocked);

                if (entry.button != null)
                {
                    entry.button.interactable = unlocked;
                    if (unlocked)
                    {
                        string sceneName = entry.sceneName;
                        entry.button.onClick.AddListener(() =>
                        {
                            if (!string.IsNullOrEmpty(sceneName))
                                SceneManager.LoadScene(sceneName);
                        });
                    }
                }
            }
        }
    }
}
