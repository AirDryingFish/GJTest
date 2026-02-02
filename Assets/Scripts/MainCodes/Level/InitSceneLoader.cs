using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Yzz
{
    /// <summary>
    /// 挂到任意物体上，在 Inspector 里指定 Button；点击后隐藏一个 Canvas，显示另一个（关卡选择界面）。支持开场视频控制器、Setting、Exit 按钮。
    /// </summary>
    public class InitSceneLoader : MonoBehaviour
    {
        /// <summary>
        /// 从游戏中返回时设为 true，BeginScene 加载后会自动调用 ShowLevelSelect() 并重置为 false。
        /// </summary>
        public static bool ShowLevelSelectOnLoad;

        /// <summary>
        /// 本局是否已播过开场视频（从关卡返回不播，首次进 BeginScene 只播一次）。
        /// </summary>
        private static bool _hasPlayedOpening;

        public Button buttonBack;

        [SerializeField] private Button button;
        [Tooltip("点击按钮后隐藏的 Canvas（如主菜单）")]
        [SerializeField] private Canvas canvasToHide;
        [Tooltip("点击按钮后显示的 Canvas（如关卡选择）")]
        [SerializeField] private Canvas canvasToShow;

        [Header("开场视频（可选）")]
        [Tooltip("若拖入，点击开始会先播开场再进关卡选择；不填则直接进关卡选择")]
        [SerializeField] private OpeningVideoPlayer openingVideoPlayer;

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
                button.onClick.AddListener(OnStartButtonClick);
            if (ShowLevelSelectOnLoad)
            {
                ShowLevelSelectOnLoad = false;
                ShowLevelSelect();
                if (openingVideoPlayer != null)
                    openingVideoPlayer.PlayBGMOnly();
            }
            else if (!_hasPlayedOpening && openingVideoPlayer != null)
            {
                // 首次进入 BeginScene（非从关卡返回）：播一次开场
                _hasPlayedOpening = true;
                openingVideoPlayer.PlaySequence(null);
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
                button.onClick.RemoveListener(OnStartButtonClick);
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

        private void OnStartButtonClick()
        {
            if (canvasToHide != null)
                canvasToHide.gameObject.SetActive(false);

            if (openingVideoPlayer != null && !_hasPlayedOpening)
            {
                _hasPlayedOpening = true;
                openingVideoPlayer.PlaySequence(ShowLevelSelect);
            }
            else
                ShowLevelSelect();
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
