using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using EntryPoint.Interface;

namespace EntryPoint.Services
{
    public class SceneLoader : ISceneLoader
    {
        private string _targetScene;
        
        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    
        public void LoadSceneAsync(string sceneName, Action onLoaded = null)
        {
            if (onLoaded != null)
            {
                SceneManager.LoadSceneAsync(sceneName).completed += (operation) =>
                {
                    onLoaded?.Invoke();
                };
            }
            else
            {
                SceneManager.LoadSceneAsync(sceneName);
            }
        }
    
        public void LoadSceneWithLoadingScreen(string sceneName)
        {
            _targetScene = sceneName;
            SceneManager.LoadScene("Loading");
        }
        
        public string GetTargetScene()
        {
            return _targetScene;
        }
    }
}