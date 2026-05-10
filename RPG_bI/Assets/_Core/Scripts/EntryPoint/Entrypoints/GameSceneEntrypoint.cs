using UnityEngine;
using System.Collections.Generic;
using EntryPoint.Entrypoints;
using SceneManagement;
using SaveSystem.MVC;
using SaveSystem;
using UnityEngine.SceneManagement;

namespace EntryPoint.Entrypoints
{
    public class GameSceneEntrypoint : MonoBehaviour
    {
        [Header("Scene Configuration")]
        [SerializeField] private SceneController _sceneSettingsController;
        [SerializeField] private PlayerSaveController _playerSaveController;
        [SerializeField] private SpawnerInitializer _spawnerInitializer;

        [Header("UI References")]
        [SerializeField] private SaveView[] _saveViews;

        private List<SaveController> _activeSaveControllers = new List<SaveController>();

        private void Start()
        {
            var global = GlobalEntrypoint.Instance;
            if (global == null) return;

            string sceneName = SceneManager.GetActiveScene().name;
            
            InitializeLevelData(global, sceneName);
            
            if (_sceneSettingsController != null)
            {
                _sceneSettingsController.Initialize(
                    global.SettingsSaveService.PostProcessing, 
                    global.SettingsSaveService.SplitScreen
                );
            }

            InitializeSaveButtons(global, sceneName);
        }

        private void InitializeLevelData(GlobalEntrypoint global, string sceneName)
        {
            if (global.SaveInteractor.HasSave(sceneName))
            {
                SceneSaveData data = global.SaveInteractor.LoadScene(sceneName);
                
                if (_playerSaveController != null)
                    _playerSaveController.ApplyPlayerData(data.players);
                
                if (_spawnerInitializer != null)
                    _spawnerInitializer.InitializeFromSave(data.enemies);
            }
            else
            {
                if (_spawnerInitializer != null)
                    _spawnerInitializer.InitializeDefault();
                
                if (_playerSaveController != null)
                    _playerSaveController.ApplyDefaultHealth();
            }
        }

        private void InitializeSaveButtons(GlobalEntrypoint global, string sceneName)
        {
            if (_saveViews == null) return;

            foreach (var view in _saveViews)
            {
                if (view != null)
                {
                    var controller = new SaveController(
                        view, 
                        global.SaveInteractor, 
                        _playerSaveController, 
                        _spawnerInitializer, 
                        sceneName
                    );
                    _activeSaveControllers.Add(controller);
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var controller in _activeSaveControllers)
            {
                controller.Dispose();
            }
            _activeSaveControllers.Clear();
        }
    }
}