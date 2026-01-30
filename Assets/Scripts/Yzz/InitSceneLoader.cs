using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Yzz
{
    /// <summary>
    /// 挂到任意物体上，在 Inspector 里指定 Button；点击后进入 MainGame 场景。
    /// </summary>
    public class InitSceneLoader : MonoBehaviour
    {
        [SerializeField] private Button button;
        [Tooltip("要加载的场景名，需在 Build Settings 中勾选")]
        [SerializeField] private string sceneName = "MainGame";

        private void Awake()
        {
            if (button != null)
                button.onClick.AddListener(LoadMainGame);
        }

        private void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(LoadMainGame);
        }

        private void LoadMainGame()
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
