namespace Moon_WiiVC_Injector.ViewModels;

// Editor state only; not yet serialized or passed to the build engine.
internal sealed class NintendontConfigViewModel : ViewModelBase
{
    public bool MemcardEmulation { get; set => SetProperty(ref field, value); } = true;
    public int MemcardBlocksIndex { get; set => SetProperty(ref field, value); }
    public bool MultiMemcard { get; set => SetProperty(ref field, value); }
    public int VideoModeIndex
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                OnPropertyChanged(nameof(CanSelectVideoType));
                OnPropertyChanged(nameof(CanPatchPal50));
            }
        }
    }
    public bool CanSelectVideoType => VideoModeIndex is 1 or 2;
    public int VideoTypeIndex { get; set => SetProperty(ref field, value); }
    public bool AutoVideoWidth { get; set => SetProperty(ref field, value); } = true;
    public decimal VideoWidth { get; set => SetProperty(ref field, value); } = 640;
    public decimal VideoOffset { get; set => SetProperty(ref field, value); }
    public bool ForceWidescreen { get; set => SetProperty(ref field, value); }
    public bool WiiUWidescreen { get; set => SetProperty(ref field, value); }
    public bool ForceProgressive
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
                OnPropertyChanged(nameof(CanPatchPal50));
        }
    }
    public bool CanPatchPal50 => CanSelectVideoType || ForceProgressive;
    public bool PatchPal50 { get; set => SetProperty(ref field, value); }
    public int LanguageIndex { get; set => SetProperty(ref field, value); }
    public int GamepadSlotIndex { get; set => SetProperty(ref field, value); }
    public bool CcRumble { get; set => SetProperty(ref field, value); }
    public bool UnlockReadSpeed { get; set => SetProperty(ref field, value); }
    public bool SkipIpl { get; set => SetProperty(ref field, value); }
    public bool Cheats { get; set => SetProperty(ref field, value); }
    public bool BbaEmulation { get; set => SetProperty(ref field, value); }
    public bool TriArcadeMode { get; set => SetProperty(ref field, value); }
    public bool OsReport { get; set => SetProperty(ref field, value); }
    public bool Log { get; set => SetProperty(ref field, value); }
}
