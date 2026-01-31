using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class IngameUIConqtroller : MonoBehaviour
{
    [Tooltip("点击后回到的关卡选择界面所在场景名，需在 Build Settings 中勾选")]
    [SerializeField] private string levelSelectSceneName = "BeginScene";

    public Button btnBack;

    private void Start()
    {
        if (btnBack != null)
            btnBack.onClick.AddListener(OnBtnBackClick);
    }

    private void OnDestroy()
    {
        if (btnBack != null)
            btnBack.onClick.RemoveListener(OnBtnBackClick);
    }

    private void OnBtnBackClick()
    {
        if (string.IsNullOrEmpty(levelSelectSceneName)) return;
        Yzz.InitSceneLoader.ShowLevelSelectOnLoad = true;
        SceneManager.LoadScene(levelSelectSceneName);
    }
}
