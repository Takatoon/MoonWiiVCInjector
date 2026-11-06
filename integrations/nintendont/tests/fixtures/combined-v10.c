#include "CommonConfig.h"

/* Compiled as PowerPC data against the pinned upstream header, not generated
   by the C# writer. Rebuild instructions are in the integration README. */
const NIN_CFG fixture __attribute__((section(".cfg_fixture"), used)) = {
    .Magicbytes = 0x01070CF6,
    .Version = NIN_CFG_VERSION,
    .Config = NIN_CFG_MEMCARDEMU | NIN_CFG_MC_MULTI | NIN_CFG_CHEATS |
        NIN_CFG_FORCE_WIDE | NIN_CFG_WIIU_WIDE | NIN_CFG_FORCE_PROG |
        NIN_CFG_AUTO_BOOT | NIN_CFG_REMLIMIT | NIN_CFG_CC_RUMBLE |
        NIN_CFG_SKIP_IPL | NIN_CFG_BBA_EMU | NIN_CFG_ARCADE_MODE |
        NIN_CFG_OSREPORT | NIN_CFG_LOG,
    .VideoMode = NIN_VID_FORCE | NIN_VID_FORCE_DF | NIN_VID_FORCE_NTSC |
        NIN_VID_PROG | NIN_VID_PATCH_PAL50,
    .Language = NIN_LAN_SPANISH,
    .GamePath = "di",
    .CheatPath = "",
    .MaxPads = 0,
    .GameID = 0,
    .MemCardBlocks = 2,
    .VideoScale = 120,
    .VideoOffset = -20,
    .NetworkProfile = 0,
    .WiiUGamepadSlot = NIN_CFG_MAXPAD,
};
_Static_assert(sizeof(fixture) == 548, "CFG v10 ABI mismatch");
