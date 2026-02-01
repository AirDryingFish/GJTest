using UnityEngine;
using UnityEngine.SceneManagement;

namespace Yzz
{
    /// <summary>
    /// 跨场景 BGM 控制器，DontDestroyOnLoad。支持多段 BGM（按关卡），场景切换时自动播对应关卡 BGM；通过 SettingsPanelController 调节音量/开关。
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class MusicManager : MonoBehaviour
    {
        private const string KeyMusicVolume = "Settings_MusicVolume";
        private const string KeyMusicMute = "Settings_MusicMute";

        public static MusicManager Instance { get; private set; }

        private AudioSource _source;

        [Header("关卡 BGM（3 段对应 3 关）")]
        [Tooltip("按顺序：关卡 1、2、3 的 BGM")]
        [SerializeField] private AudioClip[] levelBgmClips = new AudioClip[3];

        [Header("场景名与关卡对应")]
        [Tooltip("与 levelBgmClips 顺序对应：该场景加载时播第几段 BGM。如 MainGame 对应关卡 1 播 levelBgmClips[0]")]
        [SerializeField] private string[] levelSceneNames = new string[] { "MainGame1", "MainGame2", "MainGame3" };

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _source = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (Instance == this)
                Instance = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (levelSceneNames == null || levelBgmClips == null) return;
            string name = scene.name;
            for (int i = 0; i < levelSceneNames.Length && i < levelBgmClips.Length; i++)
            {
                if (levelSceneNames[i] == name)
                {
                    PlayBGMForLevel(i + 1);
                    return;
                }
            }
        }

        /// <summary>
        /// 从 PlayerPrefs 读取音乐设置并应用到 BGM AudioSource（与 SettingsPanelController 共用同一 key）。
        /// </summary>
        public void ApplyMusicSettings()
        {
            if (_source == null) return;
            float vol = PlayerPrefs.GetFloat(KeyMusicVolume, 1f);
            bool mute = PlayerPrefs.GetInt(KeyMusicMute, 0) == 1;
            _source.volume = mute ? 0f : vol;
            _source.mute = mute;
        }

        /// <summary>
        /// 播放 BGM 并循环；clip 可为 null 则用当前 clip。会先 ApplyMusicSettings 再 Play。
        /// </summary>
        public void PlayBGM(AudioClip clip = null)
        {
            if (_source == null) return;
            if (clip != null)
                _source.clip = clip;
            _source.loop = true;
            ApplyMusicSettings();
            _source.Play();
        }

        /// <summary>
        /// 播放指定关卡的 BGM（levelIndex 1～3 对应 levelBgmClips[0]～[2]），场景切换时会自动按场景名调用。
        /// </summary>
        public void PlayBGMForLevel(int levelIndex)
        {
            if (levelBgmClips == null || _source == null) return;
            int i = levelIndex - 1;
            if (i < 0 || i >= levelBgmClips.Length) return;
            AudioClip clip = levelBgmClips[i];
            if (clip == null) return;
            PlayBGM(clip);
        }

        /// <summary>
        /// 停止 BGM。
        /// </summary>
        public void StopBGM()
        {
            if (_source != null)
                _source.Stop();
        }
    }
}
