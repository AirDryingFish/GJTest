using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Yzz
{
    /// <summary>
    /// 结局场景流程：先播放视频指定秒数，再显示图片1指定秒数，再显示图片2指定秒数，最后加载 BeginScene。
    /// 切换时带渐隐/渐显；视频阶段结束后会 deactivate 视频容器内的 RawImage。
    /// 挂到 FinalScene 中任意物体（如 Canvas 或空物体），在 Inspector 里绑定视频容器、两张图片的 GameObject 和时长。
    /// </summary>
    public class FinalSceneController : MonoBehaviour
    {
        [Header("渐变")]
        [Tooltip("每次切换时渐隐/渐显的时长（秒）")]
        [SerializeField] private float fadeDuration = 0.6f;

        [Header("视频阶段")]
        [Tooltip("包含 RawImage + VideoPlayer 的容器，流程开始时激活并播放")]
        [SerializeField] private GameObject videoContainer;
        [SerializeField] private VideoPlayer videoPlayer;
        [Tooltip("视频显示秒数（到时后进入下一阶段；若勾选“等视频播完”则取二者较大值）")]
        [SerializeField] private float videoDuration = 10f;
        [Tooltip("勾选则至少等到视频播完再进入下一阶段")]
        [SerializeField] private bool waitForVideoEnd = false;

        [Header("图片1")]
        [Tooltip("第一张图片的 GameObject（如 Image 或 RawImage 所在节点）")]
        [SerializeField] private GameObject image1Panel;
        [Tooltip("图片1显示秒数")]
        [SerializeField] private float image1Duration = 5f;

        [Header("图片2")]
        [Tooltip("第二张图片的 GameObject")]
        [SerializeField] private GameObject image2Panel;
        [Tooltip("图片2显示秒数")]
        [SerializeField] private float image2Duration = 5f;

        [Header("返回")]
        [Tooltip("流程结束后加载的场景名")]
        [SerializeField] private string beginSceneName = "BeginScene";

        private void Start()
        {
            if (image1Panel != null) image1Panel.SetActive(false);
            if (image2Panel != null) image2Panel.SetActive(false);
            StartCoroutine(PlaySequence());
        }

        private IEnumerator PlaySequence()
        {
            // 1. 视频阶段：渐显 -> 播放 -> 渐隐 -> deactivate 容器与 RawImage
            if (videoContainer != null)
            {
                videoContainer.SetActive(true);
                yield return FadeToAlpha(videoContainer, 0f, 0f);
                yield return FadeToAlpha(videoContainer, 1f, fadeDuration);
            }

            if (videoPlayer != null)
            {
                videoPlayer.Play();
                if (waitForVideoEnd)
                {
                    float elapsed = 0f;
                    while ((videoPlayer.isPlaying || elapsed < videoDuration) && elapsed < 600f)
                    {
                        elapsed += Time.deltaTime;
                        yield return null;
                    }
                }
                else
                {
                    yield return new WaitForSeconds(videoDuration);
                }
            }
            else
            {
                yield return new WaitForSeconds(videoDuration);
            }

            HideRawImageInPanel(videoContainer); // 先隐藏 RawImage，避免透出到下一段
            yield return FadeToAlpha(videoContainer, 0f, fadeDuration);
            DeactivatePanelAndRawImage(videoContainer);

            // 2. 图片1：渐显 -> 显示 -> 渐隐 -> deactivate
            if (image1Panel != null)
            {
                image1Panel.SetActive(true);
                yield return FadeToAlpha(image1Panel, 0f, 0f);
                yield return FadeToAlpha(image1Panel, 1f, fadeDuration);
            }
            yield return new WaitForSeconds(image1Duration);
            HideRawImageInPanel(image1Panel);
            yield return FadeToAlpha(image1Panel, 0f, fadeDuration);
            DeactivatePanelAndRawImage(image1Panel);

            // 3. 图片2：渐显 -> 显示 -> 渐隐 -> deactivate
            if (image2Panel != null)
            {
                image2Panel.SetActive(true);
                yield return FadeToAlpha(image2Panel, 0f, 0f);
                yield return FadeToAlpha(image2Panel, 1f, fadeDuration);
            }
            yield return new WaitForSeconds(image2Duration);
            HideRawImageInPanel(image2Panel);
            yield return FadeToAlpha(image2Panel, 0f, fadeDuration);
            DeactivatePanelAndRawImage(image2Panel);

            // 4. 关掉 BGM 再回到 BeginScene，直到主界面开场播完再由 OpeningVideoPlayer 播主界面 BGM
            if (MusicManager.Instance != null)
                MusicManager.Instance.StopBGM();
            if (!string.IsNullOrEmpty(beginSceneName))
                SceneManager.LoadScene(beginSceneName);
        }

        /// <summary> 渐变到目标透明度；支持 CanvasGroup 或 Graphic(Image/RawImage)。 </summary>
        private IEnumerator FadeToAlpha(GameObject panel, float targetAlpha, float duration)
        {
            if (panel == null || duration <= 0f)
            {
                SetAlpha(panel, targetAlpha);
                yield break;
            }
            float from = GetAlpha(panel);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                SetAlpha(panel, Mathf.Lerp(from, targetAlpha, t));
                yield return null;
            }
            SetAlpha(panel, targetAlpha);
        }

        private float GetAlpha(GameObject go)
        {
            if (go == null) return 1f;
            var cg = go.GetComponent<CanvasGroup>();
            if (cg != null) return cg.alpha;
            var graphic = go.GetComponent<Graphic>();
            if (graphic != null) return graphic.color.a;
            graphic = go.GetComponentInChildren<Graphic>(true);
            return graphic != null ? graphic.color.a : 1f;
        }

        private void SetAlpha(GameObject go, float alpha)
        {
            if (go == null) return;
            var cg = go.GetComponent<CanvasGroup>();
            if (cg != null) { cg.alpha = alpha; return; }
            var graphic = go.GetComponent<Graphic>();
            if (graphic != null)
            {
                var c = graphic.color;
                c.a = alpha;
                graphic.color = c;
                return;
            }
            graphic = go.GetComponentInChildren<Graphic>(true);
            if (graphic != null)
            {
                var c = graphic.color;
                c.a = alpha;
                graphic.color = c;
            }
        }

        /// <summary> 切换前先隐藏面板内的 RawImage，避免已播完的内容透出到下一段。 </summary>
        private void HideRawImageInPanel(GameObject panel)
        {
            if (panel == null) return;
            var raw = panel.GetComponentInChildren<RawImage>(true);
            if (raw != null) raw.gameObject.SetActive(false);
        }

        /// <summary> 关闭面板并显式 deactivate 其下的 RawImage。 </summary>
        private void DeactivatePanelAndRawImage(GameObject panel)
        {
            if (panel == null) return;
            var raw = panel.GetComponentInChildren<RawImage>(true);
            if (raw != null) raw.gameObject.SetActive(false);
            panel.SetActive(false);
        }
    }
}
