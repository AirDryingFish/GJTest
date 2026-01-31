using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Yzz
{
    /// <summary>
    /// 挂到任意物体上，在 Inspector 里指定 Button；点击后隐藏一个 Canvas，显示另一个（关卡选择界面）。支持 Setting、Exit 按钮。
    /// </summary>
    public class InitSceneLoader : MonoBehaviour
    {
        /// <summary>
        /// 从游戏中返回时设为 true，BeginScene 加载后会自动调用 ShowLevelSelect() 并重置为 false。
        /// </summary>
        public static bool ShowLevelSelectOnLoad;

        public Button buttonBack;

        [SerializeField] private Button button;
        [Tooltip("点击按钮后隐藏的 Canvas（如主菜单）")]
        [SerializeField] private Canvas canvasToHide;
        [Tooltip("点击按钮后显示的 Canvas（如关卡选择）")]
        [SerializeField] private Canvas canvasToShow;

        [Header("Setting / Exit")]
        [Tooltip("设置按钮，点击后显示设置面板")]
        [SerializeField] private Button settingButton;
        [Tooltip("设置面板（Canvas 或 Panel），显示后可在其内加“返回”按钮并调用 CloseSettings()")]
        [SerializeField] private GameObject settingsPanel;
        [Tooltip("退出按钮，点击后退出应用")]
        [SerializeField] private Button exitButton;

        private void Awake()
        {
            if (button != null)
                button.onClick.AddListener(Toggle);
            if (ShowLevelSelectOnLoad)
            {
                ShowLevelSelectOnLoad = false;
                ShowLevelSelect();
            }
            if (buttonBack != null)
                buttonBack.onClick.AddListener(Toggle);
            if (settingButton != null)
                settingButton.onClick.AddListener(OnSettingClick);
            if (exitButton != null)
                exitButton.onClick.AddListener(OnExitClick);
        }

        private void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(Toggle);
            if (buttonBack != null)
                buttonBack.onClick.RemoveListener(Toggle);
            if (settingButton != null)
                settingButton.onClick.RemoveListener(OnSettingClick);
            if (exitButton != null)
                exitButton.onClick.RemoveListener(OnExitClick);
        }

        private void OnSettingClick()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(true);
        }

        /// <summary>
        /// 关闭设置面板，可在设置面板内的“返回”按钮上调用。
        /// </summary>
        public void CloseSettings()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }

        private void OnExitClick()
        {
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// 在两个 Canvas（主菜单 / 关卡选择）之间切换。主菜单按钮或关卡选择里的“返回”按钮都可调用。
        /// </summary>
        public void Toggle()
        {
            bool levelSelectVisible = canvasToShow != null && canvasToShow.gameObject.activeSelf;
            if (levelSelectVisible)
            {
                if (canvasToShow != null) canvasToShow.gameObject.SetActive(false);
                if (canvasToHide != null) canvasToHide.gameObject.SetActive(true);
            }
            else
            {
                if (canvasToHide != null) canvasToHide.gameObject.SetActive(false);
                if (canvasToShow != null) canvasToShow.gameObject.SetActive(true);
            }
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
