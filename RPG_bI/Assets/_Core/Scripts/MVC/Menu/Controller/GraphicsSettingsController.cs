using System;
using EntryPoint.Interface;
using MVC.Menu.View;

namespace MVC.Menu.Controller
{
    public class GraphicsSettingsController : IDisposable
    {
        private GraphicsSettingsView _view;
        private ISettingsSaveService _settingsService;

        public GraphicsSettingsController(GraphicsSettingsView view, ISettingsSaveService settingsService)
        {
            _view = view;
            _settingsService = settingsService;

            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            if (_view != null)
            {
                _view.OnPostProcessingChanged += HandlePostProcessingChanged;
                _view.OnShadowDistanceChanged += HandleShadowDistanceChanged;
                _view.OnRenderScaleChanged += HandleRenderScaleChanged;
                _view.OnSplitScreenChanged += HandleSplitScreenChanged;
                _view.OnSaveClicked += HandleSave;
                _view.OnCloseClicked += HandleClose;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (_view != null)
            {
                _view.OnPostProcessingChanged -= HandlePostProcessingChanged;
                _view.OnShadowDistanceChanged -= HandleShadowDistanceChanged;
                _view.OnRenderScaleChanged -= HandleRenderScaleChanged;
                _view.OnSplitScreenChanged -= HandleSplitScreenChanged;
                _view.OnSaveClicked -= HandleSave;
                _view.OnCloseClicked -= HandleClose;
            }
        }

        private void HandlePostProcessingChanged(bool value)
        {
            _settingsService.PostProcessing = value;
        }

        private void HandleShadowDistanceChanged(float value)
        {
            _settingsService.ShadowDistance = value;
        }

        private void HandleRenderScaleChanged(float value)
        {
            _settingsService.RenderScale = value;
        }

        private void HandleSplitScreenChanged(bool value)
        {
            _settingsService.SplitScreen = value;
        }

        private void HandleSave()
        {
            _settingsService.SaveSettings();
            HandleClose();
        }

        private void HandleClose()
        {
        }

        public void LoadSettingsToView()
        {
            if (_view != null)
            {
                _view.SetPostProcessing(_settingsService.PostProcessing);
                _view.SetShadowDistance(_settingsService.ShadowDistance);
                _view.SetRenderScale(_settingsService.RenderScale);
                _view.SetSplitScreen(_settingsService.SplitScreen);
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