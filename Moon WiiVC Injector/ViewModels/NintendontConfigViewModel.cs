using System.Threading.Tasks;
using Moon_WiiVC_Injector.Core.Nintendont;
using Moon_WiiVC_Injector.Services;

namespace Moon_WiiVC_Injector.ViewModels;

internal sealed class NintendontConfigViewModel(IDialogService dialogService, bool isInjectionEditor = false) : ViewModelBase
{
    private const decimal DefaultVideoWidth = 640;
    public bool IsAdvanced { get; private set => SetProperty(ref field, value); }
    public bool IsInjectionEditor => isInjectionEditor;
    public bool EmbedCustomConfig { get; set => SetProperty(ref field, value); }

    public bool DisableAutoboot
    {
        get;
        set
        {
            if (SetProperty(ref field, value) && value)
                EmbedCustomConfig = false;
        }
    }

    public bool SimpleWidescreen
    {
        get => ForceWidescreen || WiiUWidescreen;
        set
        {
            ForceWidescreen = value;
            WiiUWidescreen = value;
        }
    }

    private bool HasAdvancedChanges => DisableAutoboot || ForceWidescreen != WiiUWidescreen ||
        !MemcardEmulation || MemcardBlocksIndex != 0 || MultiMemcard || VideoModeIndex != 0 ||
        VideoTypeIndex != 0 || !AutoVideoWidth || VideoWidth != DefaultVideoWidth || VideoOffset != 0 ||
        PatchPal50 || LanguageIndex != 0 || GamepadSlotIndex != 0 || CcRumble || UnlockReadSpeed ||
        SkipIpl || Cheats || BbaEmulation || TriArcadeMode || OsReport || Log;

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
    public int VideoTypeIndex
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
                OnPropertyChanged(nameof(CanPatchPal50));
        }
    }
    public bool AutoVideoWidth { get; set => SetProperty(ref field, value); } = true;
    public decimal VideoWidth { get; set => SetProperty(ref field, value); } = DefaultVideoWidth;
    public decimal VideoOffset { get; set => SetProperty(ref field, value); }
    public bool ForceWidescreen
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
                OnPropertyChanged(nameof(SimpleWidescreen));
        }
    }
    public bool WiiUWidescreen
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
                OnPropertyChanged(nameof(SimpleWidescreen));
        }
    }
    public bool ForceProgressive
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
                OnPropertyChanged(nameof(CanPatchPal50));
        }
    }
    // Nintendont only patches PAL50 to progressive output or forced NTSC/MPAL.
    // https://github.com/FIX94/Nintendont/blob/c683db301c941a64e6fe5cef6ac079bd1a726074/kernel/Patch.c#L2334-L2413
    public bool CanPatchPal50 => ForceProgressive || (CanSelectVideoType && VideoTypeIndex is 0 or 1);
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

    public async Task SetAdvancedAsync(bool enabled)
    {
        if (enabled == IsAdvanced) return;
        if (!enabled)
        {
            if (HasAdvancedChanges && await dialogService.ShowMessageAsync(
                "Switch to Simple and reset advanced settings?",
                "Advanced configuration", MessageBoxButtons.YesNo) != MessageBoxResult.Yes)
                return;

            ResetAdvancedSettings();
        }

        IsAdvanced = enabled;
    }

    private void ResetAdvancedSettings()
    {
        bool widescreen = SimpleWidescreen;
        DisableAutoboot = false;
        MemcardEmulation = true;
        MemcardBlocksIndex = 0;
        MultiMemcard = false;
        VideoModeIndex = 0;
        VideoTypeIndex = 0;
        AutoVideoWidth = true;
        VideoWidth = DefaultVideoWidth;
        VideoOffset = 0;
        SimpleWidescreen = widescreen;
        PatchPal50 = false;
        LanguageIndex = 0;
        GamepadSlotIndex = 0;
        CcRumble = false;
        UnlockReadSpeed = false;
        SkipIpl = false;
        Cheats = false;
        BbaEmulation = false;
        TriArcadeMode = false;
        OsReport = false;
        Log = false;
    }

    public NintendontConfig ToConfig()
    {
        if (VideoModeIndex is < 0 or > 3 || VideoTypeIndex is < 0 or > 3 ||
            LanguageIndex is < 0 or > 6 || GamepadSlotIndex is < 0 or > 4 || MemcardBlocksIndex is < 0 or > 5)
            throw new ArgumentException("Invalid Nintendont selection. Please review the configuration.");
        if (VideoOffset != decimal.Truncate(VideoOffset) || VideoOffset is < -20 or > 20)
            throw new ArgumentException("Video Offset must be a whole number between -20 and 20.");
        if (!AutoVideoWidth && (VideoWidth != decimal.Truncate(VideoWidth) || VideoWidth is < 640 or > 720))
            throw new ArgumentException("Video Width must be a whole number between 640 and 720.");

        NintendontFlags flags = NintendontFlags.None;
        Add(MemcardEmulation, NintendontFlags.MemcardEmulation);
        Add(MemcardEmulation && MultiMemcard, NintendontFlags.MultiMemcard);
        Add(Cheats, NintendontFlags.Cheats);
        Add(ForceWidescreen, NintendontFlags.ForceWidescreen);
        Add(WiiUWidescreen, NintendontFlags.WiiUWidescreen);
        Add(ForceProgressive, NintendontFlags.ForceProgressive);
        Add(CcRumble, NintendontFlags.CcRumble);
        Add(UnlockReadSpeed, NintendontFlags.UnlockReadSpeed);
        Add(SkipIpl, NintendontFlags.SkipIpl);
        Add(BbaEmulation, NintendontFlags.BbaEmulation);
        Add(TriArcadeMode, NintendontFlags.TriArcadeMode);
        Add(OsReport, NintendontFlags.OsReport);
        Add(Log, NintendontFlags.Log);

        NintendontVideoMode video = VideoModeIndex switch
        {
            0 => NintendontVideoMode.Auto,
            1 => NintendontVideoMode.Force,
            2 => NintendontVideoMode.ForceDeflicker,
            _ => NintendontVideoMode.None,
        };
        if (CanSelectVideoType)
            video |= VideoTypeIndex switch
            {
                0 => NintendontVideoMode.Ntsc,
                1 => NintendontVideoMode.Mpal,
                2 => NintendontVideoMode.Pal50,
                _ => NintendontVideoMode.Pal60,
            };
        if (ForceProgressive) video |= NintendontVideoMode.Progressive;
        if (CanPatchPal50 && PatchPal50) video |= NintendontVideoMode.PatchPal50;

        var config = new NintendontConfig
        {
            Flags = flags, VideoMode = video,
            Language = LanguageIndex == 0 ? NintendontLanguage.Automatic : (NintendontLanguage)(LanguageIndex - 1),
            MemcardBlocks = MemcardEmulation ? (byte)MemcardBlocksIndex : (byte)0,
            VideoScale = AutoVideoWidth ? (sbyte)0 : (sbyte)(VideoWidth - 600),
            VideoOffset = (sbyte)VideoOffset, WiiUGamepadSlot = (uint)GamepadSlotIndex,
        };
        NintendontConfigValidator.Validate(config);
        return config;

        void Add(bool enabled, NintendontFlags flag) { if (enabled) flags |= flag; }
    }
}
