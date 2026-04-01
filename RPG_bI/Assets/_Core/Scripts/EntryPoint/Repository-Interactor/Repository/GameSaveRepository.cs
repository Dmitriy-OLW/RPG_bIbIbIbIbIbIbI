using System.IO;
using UnityEngine;

namespace SaveSystem.Repository
{
    public class GameSaveRepository : IGameSaveRepository
    {
        private readonly string _basePath;

        public GameSaveRepository(string basePath)
        {
            _basePath = Path.Combine(basePath, "GameSaves");
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        public void SaveToFile<T>(string fileName, T data)
        {
            string path = Path.Combine(_basePath, $"{fileName}.json");
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, json);
        }

        public T LoadFromFile<T>(string fileName)
        {
            string path = Path.Combine(_basePath, $"{fileName}.json");
            if (!File.Exists(path)) return default;
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<T>(json);
        }

        public bool Exists(string fileName) => File.Exists(Path.Combine(_basePath, $"{fileName}.json"));

        public void Delete(string fileName)
        {
            string path = Path.Combine(_basePath, $"{fileName}.json");
            if (File.Exists(path)) File.Delete(path);
        }

        public void ClearAll()
        {
            if (Directory.Exists(_basePath))
            {
                Directory.Delete(_basePath, true);
                Directory.CreateDirectory(_basePath);
            }
        }
    }
}