using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SceneManagement
{
    public class SceneController : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private Camera[] _allCameras;
        
        [Header("Screen Mode Objects")]
        [SerializeField] private GameObject _singlePlayerObject;
        [SerializeField] private GameObject _splitScreenObject;
        
        private bool _postProcessing;
        private bool _splitScreen;
        
        public void Initialize(bool postProcessing, bool splitScreen)
        {
            _postProcessing = postProcessing;
            _splitScreen = splitScreen;
            
            ApplyPostProcessing();
            ApplyScreenMode();
        }
        
        private void ApplyPostProcessing()
        {
            if (_allCameras == null || _allCameras.Length == 0)
                return;
                
            foreach (var camera in _allCameras)
            {
                if (camera != null)
                {
                    var urpCameraData = camera.GetComponent<UniversalAdditionalCameraData>();
                    if (urpCameraData != null)
                    {
                        urpCameraData.renderPostProcessing = _postProcessing;
                    }
                    
                    var volume = camera.GetComponent<Volume>();
                    if (volume != null)
                    {
                        volume.enabled = _postProcessing;
                    }
                }
            }
        }
        
        private void ApplyScreenMode()
        {
            if (_splitScreenObject != null)
                _splitScreenObject.SetActive(_splitScreen);
                
            if (_singlePlayerObject != null)
                _singlePlayerObject.SetActive(!_splitScreen);
        }
        
        public void UpdatePostProcessing(bool enabled)
        {
            _postProcessing = enabled;
            ApplyPostProcessing();
        }
        
        public void UpdateScreenMode(bool splitScreen)
        {
            _splitScreen = splitScreen;
            ApplyScreenMode();
        }
    }
}