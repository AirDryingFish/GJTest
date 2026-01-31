using UnityEngine;
using UnityEngine.UI;

namespace Yzz
{
    /// <summary>
    /// 挂到任意物体上，在 Inspector 里指定 Button；点击后隐藏一个 Canvas，显示另一个（关卡选择界面）。
    /// </summary>
    public class InitSceneLoader : MonoBehaviour
    {
        /// <summary>
        /// 从游戏中返回时设为 true，BeginScene 加载后会自动调用 ShowLevelSelect() 并重置为 false。
        /// </summary>
        public static bool ShowLevelSelectOnLoad;

        [SerializeField] private Button button;
        [Tooltip("点击按钮后隐藏的 Canvas（如主菜单）")]
        [SerializeField] private Canvas canvasToHide;
        [Tooltip("点击按钮后显示的 Canvas（如关卡选择）")]
        [SerializeField] private Canvas canvasToShow;

        private void Awake()
        {
            if (button != null)
                button.onClick.AddListener(ShowLevelSelect);
            if (ShowLevelSelectOnLoad)
            {
                ShowLevelSelectOnLoad = false;
                ShowLevelSelect();
            }
        }

        private void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(ShowLevelSelect);
        }

        private void ShowLevelSelect()
        {
            if (canvasToHide != null)
                canvasToHide.gameObject.SetActive(false);
            if (canvasToShow != null)
                canvasToShow.gameObject.SetActive(true);
        }

        public void BackFromGame()
        {
            ShowLevelSelect();
        }
    }
}
