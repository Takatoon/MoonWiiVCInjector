using System.Buffers.Binary;
using System.Text;

namespace Moon_WiiVC_Injector.Core.Nintendont;

internal static class NintendontConfigSerializer
{
    public const int Size = 548;
    public const uint Magic = 0x01070CF6;
    public const uint Version = 10;
    private static readonly UTF8Encoding Utf8 = new(false, true);

    public static byte[] SerializeForInjection(NintendontConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        // The embedded-file branch of the forwarder does not set these values.
        return Serialize(config with
        {
            Flags = (config.Flags | NintendontFlags.AutoBoot) &
                ~(NintendontFlags.Usb | NintendontFlags.CheatPath | NintendontFlags.Debugger |
                  NintendontFlags.DebuggerWait | NintendontFlags.DriveLed | NintendontFlags.NativeSi),
            GamePath = "di", CheatPath = string.Empty, MaxPads = 0, GameId = 0, NetworkProfile = 0,
        });
    }

    public static byte[] Serialize(NintendontConfig config)
    {
        NintendontConfigValidator.Validate(config);
        byte[] bytes = new byte[Size];
        Write(0, Magic); Write(4, Version); Write(8, (uint)config.Flags);
        Write(12, (uint)config.VideoMode); Write(16, (uint)config.Language);
        Utf8.GetBytes(config.GamePath, bytes.AsSpan(20, 255));
        Utf8.GetBytes(config.CheatPath, bytes.AsSpan(275, 255));
        // Bytes 530–531 are PowerPC alignment padding, left zeroed.
        Write(532, config.MaxPads); Write(536, config.GameId);
        bytes[540] = config.MemcardBlocks;
        bytes[541] = unchecked((byte)config.VideoScale);
        bytes[542] = unchecked((byte)config.VideoOffset);
        bytes[543] = config.NetworkProfile;
        Write(544, config.WiiUGamepadSlot);
        return bytes;

        void Write(int offset, uint value) => BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(offset, 4), value);
    }

    public static NintendontConfig Deserialize(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length != Size)
            throw new ArgumentException($"Expected a {Size}-byte CFG v10 file. Older or truncated files require explicit conversion.");
        if (BinaryPrimitives.ReadUInt32BigEndian(bytes) != Magic)
            throw new ArgumentException("Invalid Nintendont configuration magic.");
        if (BinaryPrimitives.ReadUInt32BigEndian(bytes[4..]) != Version)
            throw new ArgumentException("Only CFG v10 is supported; no implicit version conversion is performed.");
        var config = new NintendontConfig
        {
            Flags = (NintendontFlags)BinaryPrimitives.ReadUInt32BigEndian(bytes[8..]),
            VideoMode = (NintendontVideoMode)BinaryPrimitives.ReadUInt32BigEndian(bytes[12..]),
            Language = (NintendontLanguage)BinaryPrimitives.ReadUInt32BigEndian(bytes[16..]),
            GamePath = ReadPath(bytes.Slice(20, 255)), CheatPath = ReadPath(bytes.Slice(275, 255)),
            MaxPads = BinaryPrimitives.ReadUInt32BigEndian(bytes[532..]),
            GameId = BinaryPrimitives.ReadUInt32BigEndian(bytes[536..]),
            MemcardBlocks = bytes[540], VideoScale = unchecked((sbyte)bytes[541]),
            VideoOffset = unchecked((sbyte)bytes[542]), NetworkProfile = bytes[543],
            WiiUGamepadSlot = BinaryPrimitives.ReadUInt32BigEndian(bytes[544..]),
        };
        NintendontConfigValidator.Validate(config);
        return config;
    }

    private static string ReadPath(ReadOnlySpan<byte> bytes)
    {
        int end = bytes.IndexOf((byte)0);
        if (end < 0) throw new ArgumentException("Configuration path is not NUL terminated.");
        try { return Utf8.GetString(bytes[..end]); }
        catch (DecoderFallbackException ex) { throw new ArgumentException("Invalid UTF-8 configuration path.", ex); }
    }
}
