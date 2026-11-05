using System.Text;

namespace Moon_WiiVC_Injector.Core.Nintendont;

internal static class NintendontConfigValidator
{
    public static void Validate(NintendontConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        if (((uint)config.Flags & ~0x000FFFFFu) != 0)
            throw new ArgumentException("Unsupported v10 configuration flags.");
        uint video = (uint)config.VideoMode;
        if ((video & ~0x0007003Fu) != 0 || (video >> 16) is not (0 or 1 or 2 or 5))
            throw new ArgumentException("Invalid video mode flags.");
        uint type = video & 0xF;
        if (type != 0 && (type & (type - 1)) != 0)
            throw new ArgumentException("Select only one video type.");
        if ((video & (uint)NintendontVideoMode.Force) != 0 && type == 0)
            throw new ArgumentException("A forced video mode requires a video type.");
        if (!Enum.IsDefined(config.Language))
            throw new ArgumentException("Invalid Nintendont language.");
        if (config.MaxPads > 4 || config.WiiUGamepadSlot > 4 || config.MemcardBlocks > 5 || config.NetworkProfile > 3)
            throw new ArgumentException("Invalid controller, memory card or network setting.");
        if (config.VideoScale != 0 && config.VideoScale is < 40 or > 120)
            throw new ArgumentException("Video scale must be Auto (0), or 40–120 (640–720 pixels).");
        if (config.VideoOffset is < -20 or > 20)
            throw new ArgumentException("Video offset must be between -20 and 20.");
        ValidatePath(config.GamePath);
        ValidatePath(config.CheatPath);
    }

    private static void ValidatePath(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        if (path.Contains('\0') || Encoding.UTF8.GetByteCount(path) > 254)
            throw new ArgumentException("Configuration paths must fit in 254 UTF-8 bytes plus a NUL terminator.");
    }
}
