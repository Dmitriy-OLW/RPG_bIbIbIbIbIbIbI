namespace EntryPoint.Interface
{
    public interface IGameSaveService
    {
        void Save<T>(string key, T data, string sceneName = null);
        T Load<T>(string key, T defaultValue, string sceneName = null);
        bool HasKey(string key, string sceneName = null);
        void DeleteSave(string key, string sceneName = null);
        void ClearAllSaves();
        void ClearSceneSaves(string sceneName);
    }
}