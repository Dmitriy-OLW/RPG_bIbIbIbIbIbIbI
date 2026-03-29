using UnityEngine;
using EntryPoint.Interface;
using MVC.Menu.Controller;
using MVC.Menu.View;

namespace EntryPoint.Entrypoints
{
    public class MenuEntrypoint : MonoBehaviour
    {
        [SerializeField] private MenuView menuView;
        [SerializeField] private AudioSettingsView audioSettingsView;
        [SerializeField] private GraphicsSettingsView graphicsSettingsView;

        private MenuController _menuController;
        private AudioSettingsController _audioSettingsController;
        private GraphicsSettingsController _graphicsSettingsController;

        private void Start()
        {

            var gameEntrypoint = GlobalEntrypoint.Instance;

            if (gameEntrypoint == null)
            {
                return;
            }

            var sceneLoader = gameEntrypoint.SceneLoader;
            var saveInteractor = gameEntrypoint.SaveInteractor;
            var audioSaveService = gameEntrypoint.AudioSaveService;
            var settingsSaveService = gameEntrypoint.SettingsSaveService;
            
            
            _audioSettingsController = new AudioSettingsController(
                audioSettingsView,
                audioSaveService
            );

            _graphicsSettingsController = new GraphicsSettingsController(
                graphicsSettingsView,
                settingsSaveService
            );
            
            _menuController = new MenuController(
                menuView,
                sceneLoader,
                saveInteractor, 
                _audioSettingsController,
                _graphicsSettingsController
            );
        }


        private void OnDestroy()
        {
            _menuController?.Dispose();
            _audioSettingsController?.Dispose();
            _graphicsSettingsController?.Dispose();
        }
    }
}