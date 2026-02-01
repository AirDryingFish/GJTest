using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Yzz
{
    /// <summary>
    /// 开场视频控制器：两组（每组 RawImage + VideoPlayer），先激活第一组、指定秒数后 deactivate，再激活第二组、指定秒数后 deactivate，最后触发回调。
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

        [Header("可选")]
        [Tooltip("播开场期间不激活；两段都播完后设为 active。不填则不处理。")]
        [SerializeField] private GameObject openingContainer;
        [Tooltip("两段都播完后通过 MusicManager 播放此 BGM（loop），受设置面板音乐音量/开关控制；不填则不播放。")]
        [SerializeField] private AudioClip bgmClip;

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

            // 第一组：active -> 播放 -> 指定秒数 -> deactive
            if (rawImage1 != null) rawImage1.gameObject.SetActive(true);
            if (videoPlayer1 != null)
            {
                videoPlayer1.gameObject.SetActive(true);
                videoPlayer1.Play();
            }
            yield return new WaitForSeconds(duration1);
            if (rawImage1 != null) rawImage1.gameObject.SetActive(false);
            if (videoPlayer1 != null) videoPlayer1.gameObject.SetActive(false);

            // 第二组：active -> 播放 -> 指定秒数 -> deactive
            if (rawImage2 != null) rawImage2.gameObject.SetActive(true);
            if (videoPlayer2 != null)
            {
                videoPlayer2.gameObject.SetActive(true);
                videoPlayer2.Play();
            }
            yield return new WaitForSeconds(duration2);
            if (rawImage2 != null) rawImage2.gameObject.SetActive(false);
            if (videoPlayer2 != null) videoPlayer2.gameObject.SetActive(false);

            if (openingContainer != null)
                openingContainer.SetActive(true);

            if (bgmClip != null && MusicManager.Instance != null)
                MusicManager.Instance.PlayBGM(bgmClip);

            onComplete?.Invoke();
        }
    }
}
