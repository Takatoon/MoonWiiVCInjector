namespace Moon_WiiVC_Injector.Core.Nintendont;

// ABI pinned to FIX94/Nintendont 8b140805db8882adcc9af5d293288b3aa77da457.
[Flags]
internal enum NintendontFlags : uint
{
    None = 0,
    Cheats = 1u << 0,
    Debugger = 1u << 1,
    DebuggerWait = 1u << 2,
    MemcardEmulation = 1u << 3,
    CheatPath = 1u << 4,
    ForceWidescreen = 1u << 5,
    ForceProgressive = 1u << 6,
    AutoBoot = 1u << 7,
    UnlockReadSpeed = 1u << 8,
    OsReport = 1u << 9,
    Usb = 1u << 10,
    DriveLed = 1u << 11,
    Log = 1u << 12,
    MultiMemcard = 1u << 13,
    NativeSi = 1u << 14,
    WiiUWidescreen = 1u << 15,
    TriArcadeMode = 1u << 16,
    CcRumble = 1u << 17,
    SkipIpl = 1u << 18,
    BbaEmulation = 1u << 19,
}

[Flags]
internal enum NintendontVideoMode : uint
{
    Auto = 0,
    Force = 1u << 16,
    None = 2u << 16,
    ForceDeflicker = (1u | 4u) << 16,
    Pal50 = 1u << 0,
    Pal60 = 1u << 1,
    Ntsc = 1u << 2,
    Mpal = 1u << 3,
    Progressive = 1u << 4,
    PatchPal50 = 1u << 5,
}

internal enum NintendontLanguage : uint
{
    English, German, French, Spanish, Italian, Dutch,
    Automatic = uint.MaxValue,
}

internal sealed record NintendontConfig
{
    public NintendontFlags Flags { get; init; } = NintendontFlags.MemcardEmulation;
    public NintendontVideoMode VideoMode { get; init; }
    public NintendontLanguage Language { get; init; } = NintendontLanguage.Automatic;
    public string GamePath { get; init; } = string.Empty;
    public string CheatPath { get; init; } = string.Empty;
    public uint MaxPads { get; init; }
    public uint GameId { get; init; }
    public byte MemcardBlocks { get; init; }
    public sbyte VideoScale { get; init; }
    public sbyte VideoOffset { get; init; }
    public byte NetworkProfile { get; init; }
    // Slots are zero based: 0–3 are players 1–4, 4 means None.
    public uint WiiUGamepadSlot { get; init; }
}
