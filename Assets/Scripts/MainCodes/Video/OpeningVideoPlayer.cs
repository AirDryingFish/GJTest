using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Yzz
{
    /// <summary>
    /// 开场视频控制器：两组（每组 RawImage + VideoPlayer），先激活第一组、指定秒数后 deactivate，再激活第二组、指定秒数后 deactivate，最后触发回调。
    /// 若第一组无声音：1) 脚本已改为激活后等一帧再 Play；2) 在 Inspector 里两个 VideoPlayer 的 Audio Output Mode 都设为 Audio Source，并指定同一个或各自的 AudioSource。
    /// </summary>
    public class OpeningVideoPlayer : MonoBehaviour
    {
        [Header("第一组")]
        [SerializeField] private RawImage rawImage1;
        [SerializeField] private VideoPlayer videoPlayer1;
        [Tooltip("第一组显示秒数，到时后 deactivate")]
        [SerializeField] private float duration1 = 5f;

        [Header("第二组")]
        [SerializeField] private RawImage rawImage2;
        [SerializeField] private VideoPlayer videoPlayer2;
        [Tooltip("第二组显示秒数，到时后 deactivate")]
        [SerializeField] private float duration2 = 5f;
        [Tooltip("第二组从开始播放起多少秒后开始渐隐；小于 0 表示不渐隐")]
        [SerializeField] private float fadeOutStartTime = -1f;
        [Tooltip("第二组渐隐时长（秒）")]
        [SerializeField] private float fadeOutDuration = 1.5f;

        [Header("可选")]
        [Tooltip("播开场期间不激活；两段都播完后设为 active。不填则不处理。")]
        [SerializeField] private GameObject openingContainer;
        [Tooltip("两段都播完后通过 MusicManager 播放此 BGM（loop），受设置面板音乐音量/开关控制；不填则不播放。")]
        [SerializeField] private AudioClip bgmClip;

        /// <summary>
        /// 仅播放主界面 BGM（不播开场视频）。从关卡返回 BeginScene 时调用。
        /// </summary>
        public void PlayBGMOnly()
        {
            if (bgmClip != null && MusicManager.Instance != null)
                MusicManager.Instance.PlayBGM(bgmClip);
        }

        /// <summary>
        /// 按顺序播完两组（各指定秒数）后调用 onComplete。
        /// </summary>
        public void PlaySequence(Action onComplete)
        {
            StartCoroutine(PlaySequenceCoroutine(onComplete));
        }

        private IEnumerator PlaySequenceCoroutine(Action onComplete)
        {
            // 播开场期间不激活 openingContainer，只控制两组 RawImage/VideoPlayer

            // 第一组：先激活，等一帧让 AudioSource 就绪后再 Play（否则第一组常无声音）
            if (rawImage1 != null) rawImage1.gameObject.SetActive(true);
            if (videoPlayer1 != null)
            {
                videoPlayer1.gameObject.SetActive(true);
                yield return null; // 等一帧再播，避免刚激活时音频未初始化
                videoPlayer1.Play();
            }
            yield return new WaitForSeconds(duration1);

            // 先激活并播放第二组（盖在上面），再 deactivate 第一组，避免切换时出现白屏
            if (rawImage2 != null)
            {
                rawImage2.gameObject.SetActive(true);
                Color c = rawImage2.color;
                c.a = 1f;
                rawImage2.color = c;
            }
            if (videoPlayer2 != null)
            {
                videoPlayer2.gameObject.SetActive(true);
                videoPlayer2.Play();
            }
            yield return null; // 等一帧让第二组开始渲染
            if (rawImage1 != null) rawImage1.gameObject.SetActive(false);
            if (videoPlayer1 != null) videoPlayer1.gameObject.SetActive(false);

            // 第二组显示指定秒数，从 fadeOutStartTime 起渐隐
            float elapsed = 0f;
            float fadeStart = fadeOutStartTime >= 0f ? fadeOutStartTime : duration2 + 1f;
            while (elapsed < duration2)
            {
                elapsed += Time.deltaTime;
                if (rawImage2 != null && fadeOutDuration > 0f && elapsed >= fadeStart)
                {
                    float t = Mathf.Clamp01((elapsed - fadeStart) / fadeOutDuration);
                    Color c = rawImage2.color;
                    c.a = 1f - t;
                    rawImage2.color = c;
                }
                yield return null;
            }
            if (rawImage2 != null)
            {
                Color c = rawImage2.color;
                c.a = 0f;
                rawImage2.color = c;
                rawImage2.gameObject.SetActive(false);
            }
            if (videoPlayer2 != null) videoPlayer2.gameObject.SetActive(false);

            if (openingContainer != null)
                openingContainer.SetActive(true);

            if (bgmClip != null && MusicManager.Instance != null)
                MusicManager.Instance.PlayBGM(bgmClip);

            onComplete?.Invoke();
        }
    }
}
