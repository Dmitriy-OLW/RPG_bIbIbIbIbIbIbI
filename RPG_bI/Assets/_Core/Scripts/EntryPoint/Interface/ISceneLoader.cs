using System;
using UnityEngine.SceneManagement;

namespace EntryPoint.Interface
{
    public interface ISceneLoader
    {
        void LoadScene(string sceneName);
        void LoadSceneAsync(string sceneName, Action onLoaded = null);
        void LoadSceneWithLoadingScreen(string sceneName);
    }
}