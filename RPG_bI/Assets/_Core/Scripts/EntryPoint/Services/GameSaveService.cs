using System;
using System.IO;
using UnityEngine;
using EntryPoint.Interface;

namespace EntryPoint.Services
{
    public class GameSaveService : IGameSaveService
    {
        private string _baseSavePath;
        
        public GameSaveService(string baseSavePath)
        {
            _baseSavePath = baseSavePath;
            
            string gameSavesPath = Path.Combine(_baseSavePath, "GameSaves");
            if (!Directory.Exists(gameSavesPath))
            {
                Directory.CreateDirectory(gameSavesPath);
            }
        }
        
        private string GetPath(string key, string sceneName = null)
        {
            string savesDirectory;
            
            if (!string.IsNullOrEmpty(sceneName))
            {
                savesDirectory = Path.Combine(_baseSavePath, "GameSaves", sceneName);
                
                if (!Directory.Exists(savesDirectory))
                {
                    Directory.CreateDirectory(savesDirectory);
                }
            }
            else
            {
                savesDirectory = Path.Combine(_baseSavePath, "GameSaves");
            }
            
            return Path.Combine(savesDirectory, key + ".json");
        }
        
        public void Save<T>(string key, T data, string sceneName = null)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);
                string path = GetPath(key, sceneName);
                File.WriteAllText(path, json);
                Debug.Log($"Game saved: {key} at {path}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save game: {e.Message}");
            }
        }
        
        public T Load<T>(string key, T defaultValue, string sceneName = null)
        {
            try
            {
                string path = GetPath(key, sceneName);
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    return JsonUtility.FromJson<T>(json);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load game: {e.Message}");
            }
            
            return defaultValue;
        }
        
        public bool HasKey(string key, string sceneName = null)
        {
            string path = GetPath(key, sceneName);
            return File.Exists(path);
        }
        
        public void DeleteSave(string key, string sceneName = null)
        {
            try
            {
                string path = GetPath(key, sceneName);
                if (File.Exists(path))
                {
                    File.Delete(path);
                    Debug.Log($"Deleted save: {key}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete save: {e.Message}");
            }
        }
        
        public void ClearAllSaves()
        {
            try
            {
                string gameSavesPath = Path.Combine(_baseSavePath, "GameSaves");
                if (Directory.Exists(gameSavesPath))
                {
                    Directory.Delete(gameSavesPath, true);
                    Directory.CreateDirectory(gameSavesPath);
                }
                Debug.Log("All saves cleared");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to clear saves: {e.Message}");
            }
        }
        
        public void ClearSceneSaves(string sceneName)
        {
            try
            {
                string scenePath = Path.Combine(_baseSavePath, "GameSaves", sceneName);
                if (Directory.Exists(scenePath))
                {
                    Directory.Delete(scenePath, true);
                    Debug.Log($"Cleared saves for scene: {sceneName}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to clear scene saves: {e.Message}");
            }
        }
    }
}