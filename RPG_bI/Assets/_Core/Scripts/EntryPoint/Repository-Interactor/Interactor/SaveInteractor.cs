using System;
using System.Collections.Generic;
using SaveSystem.Repository;

namespace SaveSystem.Interactor
{
    public class SaveInteractor : ISaveInteractor
    {
        private readonly IGameSaveRepository _repository;

        public SaveInteractor(IGameSaveRepository repository)
        {
            _repository = repository;
        }

        public void SaveScene(string sceneName, List<PlayerSaveData> players, List<EnemySaveData> enemies)
        {
            var data = new SceneSaveData
            {
                sceneName = sceneName,
                players = players,
                enemies = enemies,
                saveTime = DateTime.Now
            };
            _repository.SaveToFile(sceneName, data);
        }

        public SceneSaveData LoadScene(string sceneName) => _repository.LoadFromFile<SceneSaveData>(sceneName);
        public bool HasSave(string sceneName) => _repository.Exists(sceneName);
        public void DeleteAllSaves() => _repository.ClearAll();
    }
}