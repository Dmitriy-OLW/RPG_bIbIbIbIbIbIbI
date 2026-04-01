using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using EntryPoint.Interface;

namespace EntryPoint.Services
{
    public class SettingsSaveService : ISettingsSaveService
    {
        private UniversalRenderPipelineAsset _urpAsset;
        private string _savePath;
        
        private bool _postProcessing = true;
        private bool _splitScreen = false;
        private float _shadowDistance = 50f;
        private float _renderScale = 1f;
        
        public bool PostProcessing
        {
            get => _postProcessing;
            set
            {
                _postProcessing = value;
            }
        }
        
        public bool SplitScreen
        {
            get => _splitScreen;
            set
            {
                _splitScreen = value;
            }
        }
        
        public float ShadowDistance
        {
            get => _shadowDistance;
            set
            {
                _shadowDistance = Mathf.Clamp(value, 0f, 200f);
                ApplyShadowDistance();
            }
        }
        
        public float RenderScale
        {
            get => _renderScale;
            set
            {
                _renderScale = Mathf.Clamp(value, 0.5f, 2f);
                ApplyRenderScale();
            }
        }
        
        public SettingsSaveService(UniversalRenderPipelineAsset urpAsset, string savePath)
        {
            _urpAsset = urpAsset;
            _savePath = savePath;
            LoadSettings();
        }
        
        public void SaveSettings()
        {
            SettingsData data = new SettingsData
            {
                postProcessing = _postProcessing,
                splitScreen = _splitScreen,
                shadowDistance = _shadowDistance,
                renderScale = _renderScale
            };
            
            string json = JsonUtility.ToJson(data, true);
            string fullPath = Path.Combine(_savePath, "graphics_settings.json");
            
            string directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            File.WriteAllText(fullPath, json);
        }
        
        public void LoadSettings()
        {
            string fullPath = Path.Combine(_savePath, "graphics_settings.json");
            
            if (File.Exists(fullPath))
            {
                string json = File.ReadAllText(fullPath);
                SettingsData data = JsonUtility.FromJson<SettingsData>(json);
                
                _postProcessing = data.postProcessing;
                _splitScreen = data.splitScreen;
                _shadowDistance = data.shadowDistance;
                _renderScale = data.renderScale;
            }
            
            ApplySettings();
        }
        
        public void ResetToDefaults()
        {
            _postProcessing = true;
            _splitScreen = false;
            _shadowDistance = 50f;
            _renderScale = 1f;
            
            ApplySettings();
            SaveSettings();
        }
        
        private void ApplySettings()
        {
            ApplyShadowDistance();
            ApplyRenderScale();
        }
        
        private void ApplyShadowDistance()
        {
            if (_urpAsset != null)
            {
                _urpAsset.shadowDistance = _shadowDistance;
            }
        }
        
        private void ApplyRenderScale()
        {
            if (_urpAsset != null)
            {
                _urpAsset.renderScale = _renderScale;
            }
        }
        
        public bool GetPostProcessing() => _postProcessing;
        public bool GetSplitScreen() => _splitScreen;
        public float GetShadowDistance() => _shadowDistance;
        public float GetRenderScale() => _renderScale;
        
        [Serializable]
        private class SettingsData
        {
            public bool postProcessing;
            public bool splitScreen;
            public float shadowDistance;
            public float renderScale;
        }
    }
}