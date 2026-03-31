using UnityEngine;
using UnityEngine.UI;
using EntryPoint.Interface;
using TMPro;
using System.Collections;
using EntryPoint.Services;
using UnityEngine.SceneManagement;

namespace EntryPoint.Entrypoints
{
    public class LoadingEntrypoint : MonoBehaviour
    {
        [SerializeField] private Slider progressBar;
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private TMP_Text loadingMessage;

        private ISceneLoader _sceneLoader;
        private string _targetScene;

        private void Start()
        {
            var gameEntrypoint = GlobalEntrypoint.Instance;

            if (gameEntrypoint == null)
            {
                return;
            }

            _sceneLoader = gameEntrypoint.SceneLoader;
            
            if (_sceneLoader is SceneLoader loader)
            {
                _targetScene = loader.GetTargetScene();
            }

            if (string.IsNullOrEmpty(_targetScene))
            {
                return;
            }

            StartCoroutine(LoadSceneAsync());
        }

        private IEnumerator LoadSceneAsync()
        {
            if (loadingMessage != null)
                loadingMessage.text = $"Loading {_targetScene}...";

            var asyncOperation = SceneManager.LoadSceneAsync(_targetScene);
            asyncOperation.allowSceneActivation = false;

            while (!asyncOperation.isDone)
            {
                float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);
                
                if (progressBar != null)
                    progressBar.value = progress;

                if (progressText != null)
                    progressText.text = $"{(progress * 100):F0}%";
                
                if (asyncOperation.progress >= 0.9f)
                {
                    if (loadingMessage != null)
                        loadingMessage.text = "Press any key to continue...";
                    
                    yield return new WaitForSeconds(1f);
                    asyncOperation.allowSceneActivation = true;
                }

                yield return null;
            }
        }
    }
}