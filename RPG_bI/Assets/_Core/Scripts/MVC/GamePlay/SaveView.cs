using UnityEngine;
using UnityEngine.UI;
using System;

namespace SaveSystem.MVC
{
    public class SaveView : MonoBehaviour
    {
        [SerializeField] private Button _saveButton;
        public event Action OnSaveClicked;

        private void Awake() => _saveButton.onClick.AddListener(() => OnSaveClicked?.Invoke());
        public void NotifySaved() => Debug.Log("Game Saved!"); 
    }
}