#include <stddef.h>
#include "CommonConfig.h"

/* Compile with the pinned PowerPC compiler and the actual dependency header. */
_Static_assert(NIN_CFG_VERSION == 10, "Unexpected configuration version");
_Static_assert(sizeof(NIN_CFG) == 548, "Unexpected configuration size");
_Static_assert(offsetof(NIN_CFG, Config) == 8, "Config offset");
_Static_assert(offsetof(NIN_CFG, GamePath) == 20, "GamePath offset");
_Static_assert(offsetof(NIN_CFG, CheatPath) == 275, "CheatPath offset");
_Static_assert(offsetof(NIN_CFG, MaxPads) == 532, "MaxPads offset");
_Static_assert(offsetof(NIN_CFG, GameID) == 536, "GameID offset");
_Static_assert(offsetof(NIN_CFG, MemCardBlocks) == 540, "MemCardBlocks offset");
_Static_assert(offsetof(NIN_CFG, VideoScale) == 541, "VideoScale offset");
_Static_assert(offsetof(NIN_CFG, VideoOffset) == 542, "VideoOffset offset");
_Static_assert(offsetof(NIN_CFG, NetworkProfile) == 543, "NetworkProfile offset");
_Static_assert(offsetof(NIN_CFG, WiiUGamepadSlot) == 544, "WiiUGamepadSlot offset");
