using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class VideoPlayerWebCompatible : MonoBehaviour
{
    public const string VIDEO_PATH = "video";

    public VideoPlayer vp;
    [Header("Compatible Path")]
    [Tooltip("如果使用webgl构建且vp使用clip模式，脚本会将其改为url模式并将视频移动到StreamingAssets下的对应相对路径，如果此路径下已有同名文件，可能会被删除")]
    public string webglCompatiblePath;
#if UNITY_EDITOR || !UNITY_WEBGL
    public VideoClip clip;
    public bool useClip = true;
#endif

    void Awake()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (vp.source == VideoSource.Url && vp.url != null)
        {
            return;
        }
        string urlRoot = Path.Combine(Application.streamingAssetsPath, VIDEO_PATH);
        vp.source = VideoSource.Url;
        vp.url = Path.Combine(urlRoot, webglCompatiblePath.TrimStart('/','\\'));
#else
        if (useClip)
        {
            vp.source = VideoSource.VideoClip;
            vp.clip = clip;
        }
#endif
        ProcessAutoPlay();
    }

    private void ProcessAutoPlay()
    {
        if (!vp.playOnAwake)
        {
            return;
        }
        if (vp.waitForFirstFrame)
        {
            vp.Prepare();
            vp.prepareCompleted += OnVPPrepared;
            return;
        }
        vp.Play();
    }

    private void OnVPPrepared(VideoPlayer source)
    {
        source.prepareCompleted -= OnVPPrepared;
        source.Play();
    }

}
