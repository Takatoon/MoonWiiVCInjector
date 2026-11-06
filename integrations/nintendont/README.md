# V10lator Nintendont autoboot forwarder

This is a separate PowerPC executable embedded as a resource by Moon and copied
to `sys/main.dol` in the generated Wii virtual ISO. It is not linked into the
.NET application. Moon's GPLv3 license does not replace this program's notices.

## Provenance

- Upstream: https://github.com/V10lator/nintendont-autoboot-forwarder
- Forwarder commit: `92d1ed80cc10f3323229456a38ccff475387baf2`
- Nintendont dependency: `8b140805db8882adcc9af5d293288b3aa77da457`
- Resource: `Moon WiiVC Injector/Resources/Dol/nintendont_v10lator.dol`
- Size: **262656 bytes**
- SHA-256: `02103ae10d8ddd1e5f0f3e7f194f650b1333ea6464faf1be1a8319c854a34db2`
- Configuration ABI: **v10, 548 bytes**, big-endian PowerPC.
- Build: normal `nintendont_loader.dol`, without `DEBUG_BUILD`.
- Source code changes: **none**.

Credit belongs to V10lator, FIX94 and the other authors identified in the source
files, including Dimok for the application booter. The upstream repository
provides GPL version 2; individual source files also retain their MIT and GPLv2
notices. See `upstream/GPL-2.0.txt` and the notices in `upstream/source.tar.gz`. These files are
copied beside Moon's build and publish output and should accompany distribution
of the executable.

`upstream/source.tar.gz` contains the forwarder sources and build scripts from the pinned
commit, including the symbolic links, plus the Nintendont files used by this
build: `CommonConfig.h`, `Metadata.h`, `NintendontVersion.h`, `wdvd.c`, and
`wdvd.h`. Generated binaries and intermediate files are excluded. The full
dependency is available at:
https://github.com/FIX94/Nintendont/tree/8b140805db8882adcc9af5d293288b3aa77da457

## Rebuild

The upstream Dockerfile references `ghcr.io/wiiu-env/devkitppc:20230621`, which
was unavailable during this build. The official contemporary image below was
used instead, with **devkitPPC release 43 / GCC 13.1.0**:

```text
devkitpro/devkitppc@sha256:f363ff415be696605f3031f938154594d67a3ee16bca4ad0b4daa137bab6da23
```

Extract the source archive on a filesystem that preserves symbolic links. For
example, from this directory using Docker on Linux or WSL:

```sh
docker run --rm --network none \
  -v "$PWD:/input:ro" \
  devkitpro/devkitppc@sha256:f363ff415be696605f3031f938154594d67a3ee16bca4ad0b4daa137bab6da23 \
  bash -lc 'set -eu; mkdir -p /tmp/forwarder; tar -xzf /input/upstream/source.tar.gz -C /tmp/forwarder; make -C /tmp/forwarder -j2; sha256sum /tmp/forwarder/nintendont_loader.dol'
```

The original build emits a linker warning that `app_booter.elf` has an RWX LOAD
segment. The build completes; this warning is recorded rather than suppressed.
Current devkitPPC images use different libogc APIs and are not substitutes for
the pinned environment without further work.

## Behavior and compatibility

The default GC build selects this DOL. Selecting **Disable Autoboot** still
selects the original `nintendont_forwarder.dol`. The previous
`nintendont_default_autobooter.dol` remains embedded for recovery/reference.

Configuration search order:

1. `SD:/nintendont/configs/XXXX.bin`, using the first four bytes of the GC disc ID.
2. `nincfg.bin` at the root of the Wii virtual ISO filesystem.
3. `SD:/nincfg.bin`.

Install Nintendont at `SD:/apps/nintendont/boot.dol`. With **Embed custom config**
enabled in the Nintendont tab, Moon now writes CFG v10 to `files/nincfg.bin`
before rebuilding the GC wrapper ISO. Otherwise supply a compatible SD
configuration externally. No SD file is written or migrated by this feature.
The search does not merge configurations. A successful read does not establish
semantic validity: the upstream forwarder does not validate the CFG version or
magic before passing it on. A short per-game file may fall back to another
source; a short global file fails. If no usable configuration exists, startup
fails. In particular, do not assume 544-byte v8/v9 files will work with this
548-byte reader. Do not simply pad old files or change their version number.

The installed Nintendont loader, not this forwarder, performs any supported CFG
migration in memory. Updating Moon does not update existing installed injects;
rebuild and reinstall them to change their DOL.

## CFG writer and embedding

The editor creates an immutable configuration snapshot before a build starts.
The writer uses explicit big-endian offsets and two zero alignment bytes at
530–531. It writes `GamePath="di"`, an empty cheat path, autoboot enabled, USB
disabled, `MaxPads=0`, `GameID=0`, and `NetworkProfile=0` for an injection.
The six memory sizes map to codes 0–5; GamePad players map to 0–3 and None to 4.
Automatic language is `0xFFFFFFFF`. Manual width is stored as width minus 600,
with zero for Auto. Video offset is signed. Deflicker includes both FORCE and
FORCE_DF, and progressive coordinates Config bit 6 with VideoMode bit 4.

Inactive Multi Memcard, video-type and PAL50 selections are retained in the UI
but omitted from the generated settings. Patch PAL50 is available with Force
Progressive, or with Force/Force Deflicker targeting NTSC or MPAL. This follows
the [PAL50 patch branches in Nintendont revision c683db3](https://github.com/FIX94/Nintendont/blob/c683db301c941a64e6fe5cef6ac079bd1a726074/kernel/Patch.c#L2334-L2413):
without Force Progressive, forcing PAL50 or PAL60 skips the PAL50 conversion.
Both the per-injection tab and the
800-pixel-wide SD Card Menu host the same `NintendontConfigView` and use separate
instances of its shared ViewModel. The global menu serializes the same model
without forcing autoboot or `GamePath="di"`. It confirms replacements, backs up
the original bytes to `nincfg.bin.bak`, and writes via a temporary file in the
destination directory before replacing the target. The v10 parser explicitly rejects
other versions and short files; it does not silently convert imported data.

Run the dependency-free checks from the repository root:

```sh
dotnet run --project integrations/nintendont/tests/SerializationTests/SerializationTests.csproj -c Release
```

The checks exercise all editor flag mappings, video and language selectors,
controller slots, ranges, malformed files, production insertion into a
temporary ISO directory, and global saving (including cancellation, backups
and invalid settings). They require no games, keys, console or network after
restoring the normal project dependencies. `fixtures/combined-v10.bin` is a
548-byte independent reference generated from `fixtures/combined-v10.c` using
the pinned PowerPC compiler and upstream header. To regenerate it, extract
`upstream/source.tar.gz` to `/tmp/forwarder` inside the build container and run:

```sh
powerpc-eabi-gcc -std=c11 -c combined-v10.c -I/tmp/forwarder/Nintendont/common/include -o /tmp/fixture.o
powerpc-eabi-objcopy -O binary --only-section=.cfg_fixture /tmp/fixture.o combined-v10.bin
```

## Validation status

The following PC checks passed:

- Phase 2 serialization suite: 190 checks covering the binary format, shared
  editor, validation, embedded insertion and global SD saving.
- Rebuilding `upstream/source.tar.gz` produces the same DOL byte for byte and checksum.
- `dotnet build "Moon WiiVC Injector.sln" -c Release --no-restore`: zero warnings
  and zero errors.
- `pwsh -NoProfile -File integrations/nintendont/tests/Test-NintendontForwarder.ps1` (from the repo
  root, PowerShell 7.6/.NET 10): validates the DOL's section ranges and entry
  point, verifies the checksum, invokes Moon's production resource extraction
  for both checkbox values, and checks retained legacy/NAND resources.
- `integrations/nintendont/tests/verify-nintendont-config.c` compiled with the pinned PowerPC compiler
  against the dependency header: v10, 548 bytes and field offsets match.
- Build output includes this notice, the license and corresponding source.

On real Wii U hardware, a standalone Phase 1 injection reached gameplay
after the older Nintendont configuration was replaced with a compatible CFG
v10 file. In Phase 2, a WUP with an embedded CFG v10 booted with this
forwarder after regenerating the Wii U menu metadata and updating the
Nintendont loader on SD. A per-game configuration on SD took precedence over
the embedded video settings.

Returning to the Wii U Menu was confirmed during testing. The remaining
hardware checks are disc swapping, launching an injection with
**Disable Autoboot** selected to verify that it opens the Nintendont menu,
and booting with only `SD:/nincfg.bin` available. For that last check, use
an injection without an embedded config and remove the per-game SD override.
