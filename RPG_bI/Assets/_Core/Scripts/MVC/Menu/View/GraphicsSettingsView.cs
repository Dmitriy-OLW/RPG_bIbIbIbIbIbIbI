using UnityEngine;
using UnityEngine.UI;
using System;

namespace MVC.Menu.View
{
    public class GraphicsSettingsView : MonoBehaviour
    {
        [Header("Graphics Settings")]
        [SerializeField] private Toggle postProcessingToggle;
        [SerializeField] private Slider shadowDistanceSlider;
        [SerializeField] private Slider renderScaleSlider;
        [SerializeField] private Toggle splitScreenToggle;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private GameObject settingsPanel;
        
        public event Action<bool> OnPostProcessingChanged;
        public event Action<float> OnShadowDistanceChanged;
        public event Action<float> OnRenderScaleChanged;
        public event Action<bool> OnSplitScreenChanged;
        public event Action OnSaveClicked;
        public event Action OnCloseClicked;

        private void Awake()
        {
            if (postProcessingToggle != null)
                postProcessingToggle.onValueChanged.AddListener(value => OnPostProcessingChanged?.Invoke(value));

            if (shadowDistanceSlider != null)
                shadowDistanceSlider.onValueChanged.AddListener(value => OnShadowDistanceChanged?.Invoke(value));

            if (renderScaleSlider != null)
                renderScaleSlider.onValueChanged.AddListener(value => OnRenderScaleChanged?.Invoke(value));

            if (splitScreenToggle != null)
                splitScreenToggle.onValueChanged.AddListener(value => OnSplitScreenChanged?.Invoke(value));

            if (saveButton != null)
                saveButton.onClick.AddListener(() => OnSaveClicked?.Invoke());

            if (closeButton != null)
                closeButton.onClick.AddListener(() => OnCloseClicked?.Invoke());
        }

        public void SetPostProcessing(bool value) => postProcessingToggle?.SetIsOnWithoutNotify(value);
        public void SetShadowDistance(float value) => shadowDistanceSlider?.SetValueWithoutNotify(value);
        public void SetRenderScale(float value) => renderScaleSlider?.SetValueWithoutNotify(value);
        public void SetSplitScreen(bool value) => splitScreenToggle?.SetIsOnWithoutNotify(value);

        private void OnDestroy()
        {
            if (postProcessingToggle != null)
                postProcessingToggle.onValueChanged.RemoveAllListeners();
            if (shadowDistanceSlider != null)
                shadowDistanceSlider.onValueChanged.RemoveAllListeners();
            if (renderScaleSlider != null)
                renderScaleSlider.onValueChanged.RemoveAllListeners();
            if (splitScreenToggle != null)
                splitScreenToggle.onValueChanged.RemoveAllListeners();
            if (saveButton != null)
                saveButton.onClick.RemoveAllListeners();
            if (closeButton != null)
                closeButton.onClick.RemoveAllListeners();
        }
    }
}