using System;
using EntryPoint.Interface;
using MVC.Menu.View;

namespace MVC.Menu.Controller
{
    public class AudioSettingsController : IDisposable
    {
        private AudioSettingsView _view;
        private IAudioSaveService _audioService;

        public AudioSettingsController(AudioSettingsView view, IAudioSaveService audioService)
        {
            _view = view;
            _audioService = audioService;

            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            if (_view != null)
            {
                _view.OnGeneralVolumeChanged += HandleGeneralVolumeChanged;
                _view.OnMusicVolumeChanged += HandleMusicVolumeChanged;
                _view.OnSfxVolumeChanged += HandleSfxVolumeChanged;
                _view.OnSaveClicked += HandleSave;
                _view.OnCloseClicked += HandleClose;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (_view != null)
            {
                _view.OnGeneralVolumeChanged -= HandleGeneralVolumeChanged;
                _view.OnMusicVolumeChanged -= HandleMusicVolumeChanged;
                _view.OnSfxVolumeChanged -= HandleSfxVolumeChanged;
                _view.OnSaveClicked -= HandleSave;
                _view.OnCloseClicked -= HandleClose;
            }
        }

        private void HandleGeneralVolumeChanged(float value)
        {
            _audioService.GeneralVolume = value;
        }

        private void HandleMusicVolumeChanged(float value)
        {
            _audioService.MusicVolume = value;
        }

        private void HandleSfxVolumeChanged(float value)
        {
            _audioService.SfxVolume = value;
        }

        private void HandleSave()
        {
            _audioService.SaveSettings();
            HandleClose();
        }

        private void HandleClose()
        {
            
        }

        public void LoadSettingsToView()
        {
            if (_view != null)
            {
                _view.SetGeneralVolume(_audioService.GeneralVolume);
                _view.SetMusicVolume(_audioService.MusicVolume);
                _view.SetSfxVolume(_audioService.SfxVolume);
            }
        }

        public void Show()
        {
            LoadSettingsToView();
        }

        public void Dispose()
        {
            UnsubscribeFromEvents();
        }
    }
}