using System.Buffers.Binary;
using Avalonia.Platform.Storage;
using Moon_WiiVC_Injector;
using Moon_WiiVC_Injector.Core;
using Moon_WiiVC_Injector.Core.Nintendont;
using Moon_WiiVC_Injector.ViewModels;
using Moon_WiiVC_Injector.Services;

int checks = 0;
void Check(bool result, string name)
{
    if (!result) throw new Exception($"FAIL: {name}");
    checks++;
}
void Reject(Action action, string name)
{
    try { action(); }
    catch (ArgumentException) { checks++; return; }
    throw new Exception($"FAIL: {name} should reject input");
}
uint Read(byte[] bytes, int offset) => BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(offset, 4));

Check(MainViewModel.GetNintendontConfigFileName("GMSE") == "GMSE.bin", "export filename uses the disc ID prefix");
foreach (string id in new[] { "", "GMS", "GMSE01", "../x", "GMS/", "gmse", "GM\0E" })
    Check(MainViewModel.GetNintendontConfigFileName(id) is null, "invalid export ID rejected");

var ui = new NintendontConfigViewModel();
byte[] defaults = NintendontConfigSerializer.SerializeForInjection(ui.ToConfig());
Check(defaults.Length == 548 && Read(defaults, 0) == 0x01070CF6 && Read(defaults, 4) == 10, "v10 header and size");
Check(Read(defaults, 8) == 0x88 && Read(defaults, 12) == 0 && Read(defaults, 16) == uint.MaxValue, "default flags, video, Auto language");
Check(defaults[20] == 'd' && defaults[21] == 'i' && defaults[22..532].All(b => b == 0), "paths and alignment padding");
Check(defaults[532..548].All(b => b == 0), "default Wii U fields");

// Every editable flag independently, to catch swapped or omitted options.
foreach (var (property, bit) in new (string, int)[] {
    ("MemcardEmulation", 3), ("MultiMemcard", 13), ("Cheats", 0),
    ("ForceWidescreen", 5), ("ForceProgressive", 6), ("WiiUWidescreen", 15),
    ("CcRumble", 17), ("UnlockReadSpeed", 8), ("SkipIpl", 18), ("BbaEmulation", 19),
    ("TriArcadeMode", 16), ("OsReport", 9), ("Log", 12) })
{
    var single = new NintendontConfigViewModel { MemcardEmulation = false };
    if (property == "MultiMemcard") single.MemcardEmulation = true;
    typeof(NintendontConfigViewModel).GetProperty(property)!.SetValue(single, true);
    uint expected = (1u << bit) | (property == "MultiMemcard" ? 8u : 0u);
    Check((uint)single.ToConfig().Flags == expected, $"flag mapping {property}");
}

uint[] modeBits = [0, 0x10000, 0x50000, 0x20000];
uint[] typeBits = [4, 8, 1, 2]; // UI order NTSC, MPAL, PAL50, PAL60.
for (int mode = 0; mode < 4; mode++)
    for (int type = 0; type < 4; type++)
    {
        ui = new() { VideoModeIndex = mode, VideoTypeIndex = type };
        Check((uint)ui.ToConfig().VideoMode == (modeBits[mode] | (mode is 1 or 2 ? typeBits[type] : 0)), $"video mode/type {mode}/{type}");
    }
for (int language = 0; language < 7; language++)
{
    ui.LanguageIndex = language;
    Check((uint)ui.ToConfig().Language == (language == 0 ? uint.MaxValue : (uint)(language - 1)), $"language {language}");
}
for (int slot = 0; slot < 5; slot++)
{
    ui.GamepadSlotIndex = slot;
    Check(ui.ToConfig().WiiUGamepadSlot == slot, $"GamePad slot {slot}");
}
for (int blocks = 0; blocks < 6; blocks++)
{
    ui.MemcardBlocksIndex = blocks;
    Check(ui.ToConfig().MemcardBlocks == blocks, $"memory blocks {blocks}");
}

ui = new() { MultiMemcard = true, PatchPal50 = true };
Check(ui.ToConfig().Flags.HasFlag(NintendontFlags.MultiMemcard), "multi enabled with emulation");
ui.MemcardEmulation = false;
Check(!ui.ToConfig().Flags.HasFlag(NintendontFlags.MultiMemcard), "inactive memory dependency omitted");
Check(!ui.ToConfig().VideoMode.HasFlag(NintendontVideoMode.PatchPal50) && ui.PatchPal50, "inactive PAL50 omitted but selection preserved");
ui.ForceProgressive = true;
Check((uint)ui.ToConfig().VideoMode == 0x30, "progressive and PAL50 coordinated bits");
ui.VideoWidth = 701;
Check(ui.ToConfig().VideoScale == 0, "auto width ignores retained manual width");
ui.AutoVideoWidth = false;
Check(ui.ToConfig().VideoScale == 101, "manual width uses 600 offset");

ui = new() {
    MultiMemcard = true, Cheats = true, ForceWidescreen = true, WiiUWidescreen = true,
    ForceProgressive = true, CcRumble = true, UnlockReadSpeed = true, SkipIpl = true,
    BbaEmulation = true, TriArcadeMode = true, OsReport = true, Log = true,
    VideoModeIndex = 2, VideoTypeIndex = 0, PatchPal50 = true, LanguageIndex = 4,
    MemcardBlocksIndex = 2, AutoVideoWidth = false, VideoWidth = 720, VideoOffset = -20,
    GamepadSlotIndex = 4,
};
byte[] combined = NintendontConfigSerializer.SerializeForInjection(ui.ToConfig());
Check(combined.SequenceEqual(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "combined-v10.bin"))), "independent PowerPC C fixture matches all bytes");
Check(Read(combined, 12) == 0x50034 && combined[541] == 120 && combined[542] == 236 && Read(combined, 544) == 4, "combined offsets and signed bytes");
Check(NintendontConfigSerializer.Serialize(NintendontConfigSerializer.Deserialize(combined)).SequenceEqual(combined), "binary round trip");
var paths = new NintendontConfig { GamePath = new string('a', 254), CheatPath = "é", GameId = 0x474D5345 };
Check(NintendontConfigSerializer.Deserialize(NintendontConfigSerializer.Serialize(paths)) == paths, "path boundaries, UTF8, and GameID round trip");
Reject(() => NintendontConfigSerializer.Serialize(paths with { GamePath = new string('a', 255) }), "path overflow");
Reject(() => NintendontConfigSerializer.Serialize(paths with { CheatPath = "bad\0path" }), "embedded NUL");
Reject(() => NintendontConfigSerializer.Serialize(new() { VideoScale = -1 }), "negative scale");
Reject(() => NintendontConfigSerializer.Serialize(new() { VideoOffset = 21 }), "offset overflow");
Reject(() => NintendontConfigSerializer.Serialize(new() { Language = (NintendontLanguage)6 }), "invalid language");
Reject(() => NintendontConfigSerializer.Serialize(new() { MaxPads = 5 }), "invalid native pad count");
Reject(() => NintendontConfigSerializer.Serialize(new() { WiiUGamepadSlot = 5 }), "invalid GamePad slot");
Reject(() => NintendontConfigSerializer.Serialize(new() { MemcardBlocks = 6 }), "invalid memory size");
Reject(() => NintendontConfigSerializer.Serialize(new() { NetworkProfile = 4 }), "invalid network profile");
Reject(() => NintendontConfigSerializer.Serialize(new() { Flags = (NintendontFlags)(1u << 20) }), "unsupported v10 bit");
Reject(() => NintendontConfigSerializer.Serialize(new() { VideoMode = NintendontVideoMode.Pal50 | NintendontVideoMode.Ntsc }), "conflicting video types");
Reject(() => NintendontConfigSerializer.Serialize(new() { VideoMode = NintendontVideoMode.Force }), "forced video without format");
Reject(() => NintendontConfigSerializer.Serialize(new() { VideoMode = (NintendontVideoMode)0x40004 }), "deflicker missing force bit");
foreach (int size in new[] { 0, 544, 547, 549 })
    Reject(() => NintendontConfigSerializer.Deserialize(new byte[size]), $"invalid length {size}");
foreach (uint version in new uint[] { 8, 9, 11 })
{
    byte[] old = (byte[])combined.Clone();
    BinaryPrimitives.WriteUInt32BigEndian(old.AsSpan(4, 4), version);
    Reject(() => NintendontConfigSerializer.Deserialize(old), $"unsupported version {version}");
}
byte[] corrupt = (byte[])combined.Clone(); corrupt[0] = 0;
Reject(() => NintendontConfigSerializer.Deserialize(corrupt), "invalid magic");
corrupt = (byte[])combined.Clone(); Array.Fill(corrupt, (byte)'a', 20, 255);
Reject(() => NintendontConfigSerializer.Deserialize(corrupt), "unterminated path");
Reject(() => new NintendontConfigViewModel { VideoModeIndex = -1 }.ToConfig(), "no video selection");
Reject(() => new NintendontConfigViewModel { AutoVideoWidth = false, VideoWidth = 640.5m }.ToConfig(), "fractional width");
Reject(() => new NintendontConfigViewModel { VideoOffset = -0.5m }.ToConfig(), "fractional offset");

var notified = new HashSet<string?>();
var reactive = new NintendontConfigViewModel { PatchPal50 = true, VideoTypeIndex = 3 };
reactive.PropertyChanged += (_, e) => notified.Add(e.PropertyName);
reactive.VideoModeIndex = 2;
Check(reactive.CanSelectVideoType && reactive.CanPatchPal50 && notified.Contains("CanSelectVideoType") && notified.Contains("CanPatchPal50"), "mode dependencies notify UI");
reactive.VideoModeIndex = 0; notified.Clear(); reactive.ForceProgressive = true;
Check(reactive.CanPatchPal50 && notified.Contains("CanPatchPal50") && reactive.VideoTypeIndex == 3 && reactive.PatchPal50, "progressive dependency and retained selections");

var sanitized = NintendontConfigSerializer.Deserialize(NintendontConfigSerializer.SerializeForInjection(new() {
    Flags = NintendontFlags.Usb | NintendontFlags.CheatPath | NintendontFlags.Debugger |
        NintendontFlags.DebuggerWait | NintendontFlags.DriveLed | NintendontFlags.NativeSi,
    GamePath = "other.iso", CheatPath = "other.gct", MaxPads = 4, NetworkProfile = 3, GameId = 123,
}));
Check(sanitized.Flags == NintendontFlags.AutoBoot && sanitized.GamePath == "di" && sanitized.CheatPath == "" &&
    sanitized.MaxPads == 0 && sanitized.NetworkProfile == 0 && sanitized.GameId == 0, "injection boot fields normalized");

// Test the actual production insertion without games, keys, network or packaging.
string temp = Path.Combine(Path.GetTempPath(), "moon-nintendont-tests-" + Guid.NewGuid().ToString("N"));
try
{
    BaseIsoTemplate.CreateBaseDirectory(temp);
    string game = Path.Combine(temp, "files", "game.iso"), disc2 = Path.Combine(temp, "files", "disc2.iso");
    File.WriteAllBytes(game, [1, 2, 3]); File.WriteAllBytes(disc2, [4, 5, 6]);
    string destination = Path.Combine(temp, "files", "nincfg.bin");
    foreach (string mode in new[] { "wii", "dol", "gcn", "wiiware" })
    {
        BuildEngine.WriteEmbeddedNintendontConfig(temp, new() { SystemType = mode });
        Check(!File.Exists(destination), $"embed off: no file for {mode}");
    }
    var options = new BuildOptions { SystemType = "gcn", EmbedNintendontConfig = true, NintendontConfig = ui.ToConfig() };
    BuildEngine.WriteEmbeddedNintendontConfig(temp, options);
    Check(File.ReadAllBytes(destination).SequenceEqual(combined), "embed on: correct file at ISO filesystem root");
    Check(File.ReadAllBytes(game).SequenceEqual(new byte[] { 1, 2, 3 }) && File.ReadAllBytes(disc2).SequenceEqual(new byte[] { 4, 5, 6 }), "both game files intact");
    options.DisableNintendontAutoboot = true;
    Reject(() => BuildEngine.WriteEmbeddedNintendontConfig(temp, options), "legacy without autoboot cannot embed");
    options.DisableNintendontAutoboot = false; options.NintendontConfig = null;
    Reject(() => BuildEngine.WriteEmbeddedNintendontConfig(temp, options), "missing embedded settings");
    options.SystemType = "wii";
    Reject(() => BuildEngine.WriteEmbeddedNintendontConfig(temp, options), "embedding rejected outside GC");
}
finally { Directory.Delete(temp, recursive: true); }
string globalDirectory = Path.Combine(Path.GetTempPath(), "moon-global-config-tests-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(globalDirectory);
try
{
    var dialogs = new TestDialogs { Folder = globalDirectory };
    var sd = new SdCardViewModel(dialogs) { SelectedDrive = null };
    sd.NintendontConfig.ForceWidescreen = true;
    sd.NintendontConfig.LanguageIndex = 4;
    sd.NintendontConfig.GamepadSlotIndex = 4;
    await sd.GenerateConfigAsync();
    string globalFile = Path.Combine(globalDirectory, "nincfg.bin");
    byte[] globalBytes = File.ReadAllBytes(globalFile);
    var global = NintendontConfigSerializer.Deserialize(globalBytes);
    Check(global == sd.NintendontConfig.ToConfig(), "global menu uses shared model and serializer");
    Check(global.Flags.HasFlag(NintendontFlags.ForceWidescreen) && !global.Flags.HasFlag(NintendontFlags.ForceProgressive), "global widescreen bit corrected");
    Check(!global.Flags.HasFlag(NintendontFlags.AutoBoot) && global.GamePath == "", "global does not force injection boot settings");
    Check(global.Language == NintendontLanguage.Spanish && global.WiiUGamepadSlot == 4, "global language and None slot");
    Check(dialogs.LastTitle == "Information", "global save reports success");

    dialogs.Overwrite = MessageBoxResult.No;
    sd.NintendontConfig.ForceProgressive = true;
    await sd.GenerateConfigAsync();
    Check(File.ReadAllBytes(globalFile).SequenceEqual(globalBytes) && !File.Exists(globalFile + ".bak"), "declined overwrite leaves global file intact");
    dialogs.Overwrite = MessageBoxResult.Yes;
    await sd.GenerateConfigAsync();
    Check(File.ReadAllBytes(globalFile + ".bak").SequenceEqual(globalBytes), "overwrite backs up previous global bytes");
    byte[] replaced = File.ReadAllBytes(globalFile);
    Check(NintendontConfigSerializer.Deserialize(replaced).Flags.HasFlag(NintendontFlags.ForceProgressive), "confirmed overwrite writes new settings");

    sd.NintendontConfig.VideoOffset = 100;
    await sd.GenerateConfigAsync();
    Check(dialogs.LastTitle == "Error" && File.ReadAllBytes(globalFile).SequenceEqual(replaced), "invalid settings cannot truncate global file");
    Check(!Directory.EnumerateFiles(globalDirectory, "*.tmp").Any(), "global writes leave no temporary files");
    Check(ui.ForceWidescreen && !ReferenceEquals(ui, sd.NintendontConfig), "editors have independent state using the same model");
}
finally { Directory.Delete(globalDirectory, recursive: true); }
Console.WriteLine($"PASS: {checks} checks (binary ABI, shared editor, validation, embedded insertion and global SD saving).");

internal sealed class TestDialogs : IDialogService
{
    public string? Folder { get; set; }
    public MessageBoxResult Overwrite { get; set; } = MessageBoxResult.Yes;
    public string? LastTitle { get; private set; }
    public Task<MessageBoxResult> ShowMessageAsync(string message, string title, MessageBoxButtons buttons = MessageBoxButtons.Ok)
    {
        LastTitle = title;
        return Task.FromResult(title == "Replace global configuration" ? Overwrite :
            buttons == MessageBoxButtons.YesNo ? MessageBoxResult.Yes : MessageBoxResult.Ok);
    }
    public Task<string?> OpenFolderDialogAsync(string title) => Task.FromResult(Folder);
    public Task<string?> SaveFileDialogAsync(string title, string fileName, string? initialDirectory = null) => throw new NotSupportedException();
    public Task<string?> PromptAsync(string message, string title, string defaultValue = "") => throw new NotSupportedException();
    public Task<string?> OpenFileDialogAsync(string title, FilePickerFileType[]? filters = null) => throw new NotSupportedException();
    public Task ShowSettingsDialogAsync() => throw new NotSupportedException();
    public Task ShowSdCardMenuDialogAsync() => throw new NotSupportedException();
    public Task SetClipboardTextAsync(string text) => throw new NotSupportedException();
    public void OpenPathWithDefaultApp(string path) => throw new NotSupportedException();
}
