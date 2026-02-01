using UnityEngine;
using UnityEngine.UI;

namespace Yzz
{
    /// <summary>
    /// 挂到设置面板上，控制 MusicSlider、SoundSlider、MusicToggle、SoundToggle，用 PlayerPrefs 持久化。
    /// </summary>
    public class SettingsPanelController : MonoBehaviour
    {
        private const string KeyMusicVolume = "Settings_MusicVolume";
        private const string KeySoundVolume = "Settings_SoundVolume";
        private const string KeyMusicMute = "Settings_MusicMute";
        private const string KeySoundMute = "Settings_SoundMute";

        [Header("UI 引用")]
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider soundSlider;
        [SerializeField] private Toggle musicToggle;
        [SerializeField] private Toggle soundToggle;
        [Tooltip("设置面板内的返回按钮，点击后关闭本面板")]
        [SerializeField] private Button settingBackButton;

        [Header("音频（可选）")]
        [Tooltip("BGM 的 AudioSource，不填则只保存数值")]
        [SerializeField] private AudioSource musicSource;

        private void Awake()
        {
            LoadSoundSettingsFromPrefs();
        }

        private void OnEnable()
        {
            LoadAndApply();
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (musicSlider != null) musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
            if (soundSlider != null) soundSlider.onValueChanged.AddListener(OnSoundSliderChanged);
            if (musicToggle != null) musicToggle.onValueChanged.AddListener(OnMusicToggleChanged);
            if (soundToggle != null) soundToggle.onValueChanged.AddListener(OnSoundToggleChanged);
            if (settingBackButton != null) settingBackButton.onClick.AddListener(OnBackClick);
        }

        private void Unsubscribe()
        {
            if (musicSlider != null) musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);
            if (soundSlider != null) soundSlider.onValueChanged.RemoveListener(OnSoundSliderChanged);
            if (musicToggle != null) musicToggle.onValueChanged.RemoveListener(OnMusicToggleChanged);
            if (soundToggle != null) soundToggle.onValueChanged.RemoveListener(OnSoundToggleChanged);
            if (settingBackButton != null) settingBackButton.onClick.RemoveListener(OnBackClick);
        }

        private void OnBackClick()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 从 PlayerPrefs 加载音效设置到静态属性（在 Awake 调用，避免静态构造函数里用 PlayerPrefs）。
        /// </summary>
        private static void LoadSoundSettingsFromPrefs()
        {
            SoundVolume = PlayerPrefs.GetFloat(KeySoundVolume, 1f);
            SoundMute = PlayerPrefs.GetInt(KeySoundMute, 0) == 1;
        }

        /// <summary>
        /// 从 PlayerPrefs 读取并更新 UI 与音频。
        /// </summary>
        public void LoadAndApply()
        {
            float musicVol = PlayerPrefs.GetFloat(KeyMusicVolume, 1f);
            float soundVol = PlayerPrefs.GetFloat(KeySoundVolume, 1f);
            bool musicMute = PlayerPrefs.GetInt(KeyMusicMute, 0) == 1;
            bool soundMute = PlayerPrefs.GetInt(KeySoundMute, 0) == 1;

            if (musicSlider != null) musicSlider.SetValueWithoutNotify(musicVol);
            if (soundSlider != null) soundSlider.SetValueWithoutNotify(soundVol);
            if (musicToggle != null) musicToggle.SetIsOnWithoutNotify(!musicMute);
            if (soundToggle != null) soundToggle.SetIsOnWithoutNotify(!soundMute);

            ApplyMusic(musicVol, musicMute);
            ApplySound(soundVol, soundMute);
        }

        private void OnMusicSliderChanged(float value)
        {
            PlayerPrefs.SetFloat(KeyMusicVolume, value);
            PlayerPrefs.Save();
            bool mute = musicToggle != null && !musicToggle.isOn;
            ApplyMusic(value, mute);
        }

        private void OnSoundSliderChanged(float value)
        {
            PlayerPrefs.SetFloat(KeySoundVolume, value);
            PlayerPrefs.Save();
            bool mute = soundToggle != null && !soundToggle.isOn;
            ApplySound(value, mute);
        }

        private void OnMusicToggleChanged(bool isOn)
        {
            PlayerPrefs.SetInt(KeyMusicMute, isOn ? 0 : 1);
            PlayerPrefs.Save();
            float vol = musicSlider != null ? musicSlider.value : 1f;
            ApplyMusic(vol, !isOn);
        }

        private void OnSoundToggleChanged(bool isOn)
        {
            PlayerPrefs.SetInt(KeySoundMute, isOn ? 0 : 1);
            PlayerPrefs.Save();
            float vol = soundSlider != null ? soundSlider.value : 1f;
            ApplySound(vol, !isOn);
        }

        private void ApplyMusic(float volume, bool mute)
        {
            if (musicSource != null)
            {
                musicSource.volume = mute ? 0f : volume;
                musicSource.mute = mute;
            }
            MusicManager.Instance?.ApplyMusicSettings();
        }

        private void ApplySound(float volume, bool mute)
        {
            SoundVolume = volume;
            SoundMute = mute;
        }

        /// <summary>
        /// 当前音效音量（0～1），SFX 播放时可用此值乘到 volume 上。
        /// </summary>
        public static float SoundVolume { get; private set; } = 1f;

        /// <summary>
        /// 当前音效是否静音，SFX 播放时若为 true 可不播或 volume=0。
        /// </summary>
        public static bool SoundMute { get; private set; }
    }
}
