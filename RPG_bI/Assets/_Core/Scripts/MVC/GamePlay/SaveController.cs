using System;
using SaveSystem.Interactor;
using SceneManagement;

namespace SaveSystem.MVC
{
    public class SaveController : IDisposable
    {
        private readonly SaveView _view;
        private readonly ISaveInteractor _interactor;
        private readonly PlayerSaveController _players;
        private readonly SpawnerInitializer _spawner;
        private readonly string _sceneName;

        public SaveController(SaveView view, ISaveInteractor interactor, 
            PlayerSaveController players, SpawnerInitializer spawner, string sceneName)
        {
            _view = view;
            _interactor = interactor;
            _players = players;
            _spawner = spawner;
            _sceneName = sceneName;

            _view.OnSaveClicked += HandleSave;
        }

        private void HandleSave()
        {
            _interactor.SaveScene(_sceneName, _players.GetPlayersData(), _spawner.GetActiveEnemiesData());
            _view.NotifySaved();
        }

        public void Dispose() => _view.OnSaveClicked -= HandleSave;
    }
}