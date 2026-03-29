using UnityEngine;
using UnityEngine.UI;
using System;

namespace MVC.Menu.View
{
    public class MenuView : MonoBehaviour
    {
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button audioSettingsButton;
        [SerializeField] private Button graphicsSettingsButton;
    
        public event Action OnNewGameClicked;
        public event Action OnContinueClicked;
        public event Action OnAudioSettingsClicked;
        public event Action OnGraphicsSettingsClicked;
    
        private void Awake()
        {
            if (newGameButton != null)
                newGameButton.onClick.AddListener(() => OnNewGameClicked?.Invoke());
            
            if (continueButton != null)
                continueButton.onClick.AddListener(() => OnContinueClicked?.Invoke());
            
            if (audioSettingsButton != null)
                audioSettingsButton.onClick.AddListener(() => OnAudioSettingsClicked?.Invoke());
            
            if (graphicsSettingsButton != null)
                graphicsSettingsButton.onClick.AddListener(() => OnGraphicsSettingsClicked?.Invoke());
        }
    
        public void SetContinueButtonActive(bool isActive)
        {
            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(isActive);
                continueButton.interactable = isActive;
            }
        }
    
        private void OnDestroy()
        {
            if (newGameButton != null)
                newGameButton.onClick.RemoveAllListeners();
            if (continueButton != null)
                continueButton.onClick.RemoveAllListeners();
            if (audioSettingsButton != null)
                audioSettingsButton.onClick.RemoveAllListeners();
            if (graphicsSettingsButton != null)
                graphicsSettingsButton.onClick.RemoveAllListeners();
        }
    }
}