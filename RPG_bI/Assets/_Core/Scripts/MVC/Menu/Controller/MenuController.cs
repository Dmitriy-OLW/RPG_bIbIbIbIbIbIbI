using System;
using MVC.Menu.View;
using EntryPoint.Interface;

namespace MVC.Menu.Controller
{
    public class MenuController : IDisposable
    {
        private MenuView _view;
        private ISceneLoader _sceneLoader;
        private IGameSaveService _saveService;
        private AudioSettingsController _audioSettingsController;
        private GraphicsSettingsController _graphicsSettingsController;

        private const string GAMEPLAY_SCENE = "GameScene";
        
        public MenuController(
            MenuView view,
            ISceneLoader sceneLoader,
            IGameSaveService saveService,
            AudioSettingsController audioSettingsController,
            GraphicsSettingsController graphicsSettingsController)
        {
            _view = view;
            _sceneLoader = sceneLoader;
            _saveService = saveService;
            _audioSettingsController = audioSettingsController;
            _graphicsSettingsController = graphicsSettingsController;

            SubscribeToEvents();
            CheckContinueAvailability();
        }

        private void SubscribeToEvents()
        {
            if (_view != null)
            {
                _view.OnNewGameClicked += HandleNewGame;
                _view.OnContinueClicked += HandleContinue;
                _view.OnAudioSettingsClicked += ShowAudioSettings;
                _view.OnGraphicsSettingsClicked += ShowGraphicsSettings;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (_view != null)
            {
                _view.OnNewGameClicked -= HandleNewGame;
                _view.OnContinueClicked -= HandleContinue;
                _view.OnAudioSettingsClicked -= ShowAudioSettings;
                _view.OnGraphicsSettingsClicked -= ShowGraphicsSettings;
            }
        }

        private void CheckContinueAvailability()
        {
            bool hasSave = _saveService.HasKey("player", "GameScene");
            _view?.SetContinueButtonActive(hasSave);
        }

        private void HandleNewGame()
        {
            _saveService.ClearSceneSaves(GAMEPLAY_SCENE);

            _sceneLoader.LoadSceneWithLoadingScreen(GAMEPLAY_SCENE);
        }

        private void HandleContinue()
        {
            _sceneLoader.LoadSceneWithLoadingScreen(GAMEPLAY_SCENE);
        }

        private void ShowAudioSettings()
        {
            _audioSettingsController?.Show();
        }

        private void ShowGraphicsSettings()
        {
            _graphicsSettingsController?.Show();
        }

        public void Dispose()
        {
            UnsubscribeFromEvents();
        }
    }
}