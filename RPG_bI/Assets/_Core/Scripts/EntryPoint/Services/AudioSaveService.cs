using System;
using UnityEngine;
using UnityEngine.Audio;
using System.IO;
using EntryPoint.Interface;

namespace EntryPoint.Services
{
    public class AudioSaveService : IAudioSaveService
    {
        private AudioMixer _audioMixer;
        private AudioSource _musicSource;
        private float _generalVolume = 0.8f;
        private float _musicVolume = 0.8f;
        private float _sfxVolume = 0.9f;

        private const string GENERAL_VOLUME_PARAM = "GeneralVolume";
        private const string MUSIC_VOLUME_PARAM = "MusicVolume";
        private const string SFX_VOLUME_PARAM = "SfxVolume";

        private string _savePath;

        public float GeneralVolume
        {
            get => _generalVolume;
            set
            {
                _generalVolume = Mathf.Clamp01(value);
                ApplyGeneralVolume();
            }
        }

        public float MusicVolume
        {
            get => _musicVolume;
            set
            {
                _musicVolume = Mathf.Clamp01(value);
                ApplyMusicVolume();
            }
        }

        public float SfxVolume
        {
            get => _sfxVolume;
            set
            {
                _sfxVolume = Mathf.Clamp01(value);
                ApplySfxVolume();
            }
        }

        public AudioSaveService(AudioMixer audioMixer, AudioSource musicSource, string savePath)
        {
            _audioMixer = audioMixer;
            _musicSource = musicSource;
            _savePath = savePath;

            if (_musicSource != null)
            {
                _musicSource.loop = true;
                if (_audioMixer != null)
                {
                    _musicSource.outputAudioMixerGroup = _audioMixer.FindMatchingGroups("Music")[0];
                }
            }

            LoadSettings();
        }
        
        
        public void SaveSettings()
        {
            AudioSettingsData data = new AudioSettingsData
            {
                generalVolume = _generalVolume,
                musicVolume = _musicVolume,
                sfxVolume = _sfxVolume
            };

            string json = JsonUtility.ToJson(data, true);
            string fullPath = Path.Combine(_savePath, "audio_settings.json");

            string directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(fullPath, json);
        }

        public void LoadSettings()
        {
            string fullPath = Path.Combine(_savePath, "audio_settings.json");

            if (File.Exists(fullPath))
            {
                string json = File.ReadAllText(fullPath);
                AudioSettingsData data = JsonUtility.FromJson<AudioSettingsData>(json);

                _generalVolume = data.generalVolume;
                _musicVolume = data.musicVolume;
                _sfxVolume = data.sfxVolume;
            }

            ApplyGeneralVolume();
            ApplyMusicVolume();
            ApplySfxVolume();
        }

        public void PlayMusic(AudioClip[] clips, int firstMusic)
        {
            if (clips == null || clips.Length == 0 || _musicSource == null)
                return;

            firstMusic = Mathf.Clamp(firstMusic, 0, clips.Length - 1);
            _musicSource.clip = clips[firstMusic];
            _musicSource.Play();
        }

        public void StopMusic()
        {
            if (_musicSource != null)
            {
                _musicSource.Stop();
            }
        }

        private void ApplyGeneralVolume()
        {
            if (_audioMixer != null)
            {
                float volumeDB = _generalVolume > 0.01f ? Mathf.Log10(_generalVolume) * 20 : -80f;
                _audioMixer.SetFloat(GENERAL_VOLUME_PARAM, volumeDB);
            }
        }

        private void ApplyMusicVolume()
        {
            if (_audioMixer != null)
            {
                float volumeDB = _musicVolume > 0.01f ? Mathf.Log10(_musicVolume) * 20 : -80f;
                _audioMixer.SetFloat(MUSIC_VOLUME_PARAM, volumeDB);
            }
        }

        private void ApplySfxVolume()
        {
            if (_audioMixer != null)
            {
                float volumeDB = _sfxVolume > 0.01f ? Mathf.Log10(_sfxVolume) * 20 : -80f;
                _audioMixer.SetFloat(SFX_VOLUME_PARAM, volumeDB);
            }
        }

        [Serializable]
        private class AudioSettingsData
        {
            public float generalVolume;
            public float musicVolume;
            public float sfxVolume;
        }
    }
}