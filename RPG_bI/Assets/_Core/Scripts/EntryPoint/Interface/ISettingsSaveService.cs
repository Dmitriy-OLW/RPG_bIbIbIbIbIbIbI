// ISettingsSaveService.cs
namespace EntryPoint.Interface
{
    public interface ISettingsSaveService
    {
        bool PostProcessing { get; set; }
        bool SplitScreen { get; set; }
        float ShadowDistance { get; set; }
        float RenderScale { get; set; }
        
        void SaveSettings();
        void LoadSettings();
        void ResetToDefaults();
    }
}