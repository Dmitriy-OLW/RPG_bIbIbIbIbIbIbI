using UnityEngine;
using EntryPoint.Interface;
using SceneManagement;
using EntryPoint.Services;

namespace EntryPoint.Entrypoints
{
    public class GameSceneEntrypoint : MonoBehaviour
    {
        [Header("Scene References")]
        /*[SerializeField] private GameSceneController _sceneController;
        [SerializeField] private GameSaveView _saveView;*/
        [SerializeField] private SceneController _sceneSettingsController;
        
        //private GameSaveController _saveController;
        
        private void Start()
        {
            var globalEntrypoint = GlobalEntrypoint.Instance;
            if (globalEntrypoint == null) return;
            
            var settingsService = globalEntrypoint.SettingsSaveService;
            var gameSaveService = globalEntrypoint.GameSaveService;
            var sceneLoader = globalEntrypoint.SceneLoader;
            
            if (_sceneSettingsController != null && settingsService != null)
            {
                _sceneSettingsController.Initialize(settingsService.PostProcessing, settingsService.SplitScreen);
            }
            
            /*if (_sceneController != null)
            {
                string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                _sceneController.Initialize(gameSaveService, sceneName);
                
                if (_saveView != null)
                {
                    _saveController = new GameSaveController(_saveView, _sceneController, gameSaveService, sceneName);
                }
                
                // Загружаем сцену с учетом сохранения
                if (sceneLoader is EntryPoint.Services.SceneLoader loader && loader.ShouldLoadFromSave())
                {
                    if (_sceneController.HasSaveData())
                    {
                        _sceneController.LoadScene();
                    }
                    else
                    {
                        _sceneController.LoadScene(); 
                    }
                }
                else
                {
                    _sceneController.LoadScene(); 
                }
            }*/
        }
        
        private void OnDestroy()
        {
            //_saveController?.Dispose();
        }
    }
}