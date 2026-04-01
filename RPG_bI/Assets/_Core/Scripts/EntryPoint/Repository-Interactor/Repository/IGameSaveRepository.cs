namespace SaveSystem.Repository
{
    public interface IGameSaveRepository
    {
        void SaveToFile<T>(string fileName, T data);
        T LoadFromFile<T>(string fileName);
        bool Exists(string fileName);
        void Delete(string fileName);
        void ClearAll();
    }
}