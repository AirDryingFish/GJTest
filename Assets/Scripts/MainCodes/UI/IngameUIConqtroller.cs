using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class IngameUIConqtroller : MonoBehaviour
{
    [Tooltip("点击后回到的关卡选择界面所在场景名，需在 Build Settings 中勾选")]
    [SerializeField] private string levelSelectSceneName = "BeginScene";

    public Button btnBack;

    [Header("跟随节点移动的 UI 对话框")]
    [Tooltip("对话框预制体（含 RectTransform），运行时实例化并挂到 parentNode 下")]
    [SerializeField] private GameObject dialogPrefab;
    [Tooltip("对话框挂在这个节点下，节点移动时对话框跟随（需在 Canvas 下或 World Space Canvas）")]
    [SerializeField] private Transform parentNode;
    [Tooltip("挂上去后的本地偏移")]
    [SerializeField] private Vector2 localOffset = Vector2.zero;

    private GameObject _dialogInstance;

    private void Start()
    {
        if (btnBack != null)
            btnBack.onClick.AddListener(OnBtnBackClick);
    }

    private void OnDestroy()
    {
        if (btnBack != null)
            btnBack.onClick.RemoveListener(OnBtnBackClick);
        HideDialog();
    }

    private void OnBtnBackClick()
    {
        if (string.IsNullOrEmpty(levelSelectSceneName)) return;
        Yzz.InitSceneLoader.ShowLevelSelectOnLoad = true;
        SceneManager.LoadScene(levelSelectSceneName);
    }

    /// <summary>
    /// 在 parentNode 下创建并显示对话框，对话框会随节点移动。若已存在则只显示。
    /// </summary>
    public void ShowDialog()
    {
        if (parentNode == null) return;
        if (_dialogInstance != null)
        {
            _dialogInstance.SetActive(true);
            return;
        }
        if (dialogPrefab == null) return;
        _dialogInstance = Instantiate(dialogPrefab, parentNode);
        var rect = _dialogInstance.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = localOffset;
            rect.localScale = Vector3.one;
        }
        else
            _dialogInstance.transform.localPosition = localOffset;
    }

    /// <summary>
    /// 指定挂载节点并显示对话框（可覆盖 Inspector 的 parentNode）。
    /// </summary>
    public void ShowDialogUnder(Transform node)
    {
        if (node == null) return;
        if (_dialogInstance != null)
        {
            _dialogInstance.transform.SetParent(node, false);
            var rect = _dialogInstance.GetComponent<RectTransform>();
            if (rect != null) rect.anchoredPosition = localOffset;
            else _dialogInstance.transform.localPosition = localOffset;
            _dialogInstance.SetActive(true);
            return;
        }
        if (dialogPrefab == null) return;
        _dialogInstance = Instantiate(dialogPrefab, node);
        var r = _dialogInstance.GetComponent<RectTransform>();
        if (r != null) { r.anchoredPosition = localOffset; r.localScale = Vector3.one; }
        else _dialogInstance.transform.localPosition = localOffset;
    }

    /// <summary>
    /// 隐藏并保留对话框实例，下次 ShowDialog 会直接显示。
    /// </summary>
    public void HideDialog()
    {
        if (_dialogInstance != null)
            _dialogInstance.SetActive(false);
    }

    /// <summary>
    /// 销毁对话框实例。
    /// </summary>
    public void DestroyDialog()
    {
        if (_dialogInstance != null)
        {
            Destroy(_dialogInstance);
            _dialogInstance = null;
        }
    }
}
