using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.Universal;
using EntryPoint.Interface;
using EntryPoint.Services;
using System.IO;
using SaveSystem.Repository;
using SaveSystem.Interactor;

namespace EntryPoint.Entrypoints
{
    public class GlobalEntrypoint : MonoBehaviour
    {
        private static GlobalEntrypoint  _instance;

        public static GlobalEntrypoint  Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("GlobalEntrypoint ");
                    _instance = go.AddComponent<GlobalEntrypoint >();
                    DontDestroyOnLoad(go);
                }

                return _instance;
            }
        }

        [Header("Audio Settings")] 
        [SerializeField] private AudioMixer _audioMixer;

        [SerializeField] private AudioClip[] _musicPlaylist;
        [SerializeField] private int _startMusicIndex = 0;

        [Header("Graphics Settings")] 
        [SerializeField] private UniversalRenderPipelineAsset _urpAsset;

        [Header("Save Path Settings")] 
        [SerializeField] private SavePathType _savePathType = SavePathType.PersistentData;
        [SerializeField] private string _customSavePath = "";
        
        public IAudioSaveService AudioSaveService { get; private set; }
        public IGameSaveService GameSaveService { get; private set; }
        public ISettingsSaveService SettingsSaveService { get; private set; }
        
        public ISaveInteractor SaveInteractor { get; private set; }
        public ISceneLoader SceneLoader { get; private set; }

        private string _baseSavePath;

        public enum SavePathType
        {
            PersistentData, 
            ProjectData, 
            Custom 
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeSavePath();
            InitializeServices();
            LoadAllSettings();
            StartMusic();
            LoadMainMenu();
        }

        private void InitializeSavePath()
        {
            switch (_savePathType)
            {
                case SavePathType.PersistentData:
                    _baseSavePath = Application.persistentDataPath;
                    break;

                case SavePathType.ProjectData:
                    _baseSavePath = Application.dataPath + "/GameSaves";
                    break;

                case SavePathType.Custom:
                    if (!string.IsNullOrEmpty(_customSavePath))
                        _baseSavePath = _customSavePath;
                    else
                        _baseSavePath = Application.persistentDataPath;
                    break;
            }
        }

        private void InitializeServices()
        {
            GameObject audioObject = new GameObject("MusicAudioSource");
            audioObject.transform.SetParent(transform);
            AudioSource musicSource = audioObject.AddComponent<AudioSource>();
            
            AudioSaveService = new AudioSaveService(_audioMixer, musicSource, _baseSavePath);
            
            SettingsSaveService = new SettingsSaveService(_urpAsset, _baseSavePath);
            
            GameSaveService = new GameSaveService(_baseSavePath);

            var saveRepo = new GameSaveRepository(_baseSavePath);
            SaveInteractor = new SaveInteractor(saveRepo);
        
            SceneLoader = new SceneLoader();
        }

        private void LoadAllSettings()
        {
            AudioSaveService.LoadSettings();
            
            SettingsSaveService.LoadSettings();
        }

        private void StartMusic()
        {
            if (_musicPlaylist != null && _musicPlaylist.Length > 0)
            {
                AudioSaveService.PlayMusic(_musicPlaylist, _startMusicIndex);
            }
        }

        private void LoadMainMenu()
        {
            if (SceneLoader != null)
            {
                SceneLoader.LoadScene("MainMenu");
            }
        }
        
        public string GetSceneSavePath(string sceneName)
        {
            return Path.Combine(_baseSavePath, "GameSaves", sceneName);
        }
    }
}