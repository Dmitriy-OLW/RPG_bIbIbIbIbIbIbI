using SaveSystem;
using System.Collections.Generic;

namespace SaveSystem.Interactor
{
    public interface ISaveInteractor
    {
        void SaveScene(string sceneName, List<PlayerSaveData> players, List<EnemySaveData> enemies);
        SceneSaveData LoadScene(string sceneName);
        bool HasSave(string sceneName);
        void DeleteAllSaves();
    }
}