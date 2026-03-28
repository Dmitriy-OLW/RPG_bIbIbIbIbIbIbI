using UnityEngine;

namespace EntryPoint.Interface
{
    public interface IAudioSaveService
    {
        float GeneralVolume { get; set; }
        float MusicVolume { get; set; }
        float SfxVolume { get; set; }
    
        void SaveSettings();
        void LoadSettings();
        void PlayMusic(AudioClip[] clips, int firstMusic);
        void StopMusic();
    }
}