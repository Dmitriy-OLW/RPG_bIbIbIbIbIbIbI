using UnityEngine;
using UnityEngine.UI;
using System;

namespace MVC.Menu.View
{
    public class AudioSettingsView : MonoBehaviour
    {
        [Header("Audio Settings")] 
        [SerializeField] private Slider generalVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private GameObject settingsPanel;

        // Audio events
        public event Action<float> OnGeneralVolumeChanged;
        public event Action<float> OnMusicVolumeChanged;
        public event Action<float> OnSfxVolumeChanged;
        public event Action OnSaveClicked;
        public event Action OnCloseClicked;

        private void Awake()
        {
            if (generalVolumeSlider != null)
                generalVolumeSlider.onValueChanged.AddListener(value => OnGeneralVolumeChanged?.Invoke(value));

            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.AddListener(value => OnMusicVolumeChanged?.Invoke(value));

            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.AddListener(value => OnSfxVolumeChanged?.Invoke(value));

            if (saveButton != null)
                saveButton.onClick.AddListener(() => OnSaveClicked?.Invoke());

            if (closeButton != null)
                closeButton.onClick.AddListener(() => OnCloseClicked?.Invoke());
        }

        public void SetGeneralVolume(float value) => generalVolumeSlider?.SetValueWithoutNotify(value);
        public void SetMusicVolume(float value) => musicVolumeSlider?.SetValueWithoutNotify(value);
        public void SetSfxVolume(float value) => sfxVolumeSlider?.SetValueWithoutNotify(value);

        private void OnDestroy()
        {
            if (generalVolumeSlider != null)
                generalVolumeSlider.onValueChanged.RemoveAllListeners();
            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.RemoveAllListeners();
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.RemoveAllListeners();
            if (saveButton != null)
                saveButton.onClick.RemoveAllListeners();
            if (closeButton != null)
                closeButton.onClick.RemoveAllListeners();
        }
    }
}