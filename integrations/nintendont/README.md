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

This phase does not embed a configuration. Supply a compatible configuration
externally and install Nintendont at `SD:/apps/nintendont/boot.dol`.
The search does not merge configurations. A successful read does not establish
semantic validity: the upstream forwarder does not validate the CFG version or
magic before passing it on. A short per-game file may fall back to another
source; a short global file fails. If no usable configuration exists, startup
fails. In particular, do not assume 544-byte v8/v9 files will work with this
548-byte reader. Do not simply pad old files or change their version number.

The installed Nintendont loader, not this forwarder, performs any supported CFG
migration in memory. Updating Moon does not update existing installed injects;
rebuild and reinstall them to change their DOL.

## Validation status

The following PC checks passed:

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

On real Wii U hardware, this same forwarder booted a Phase 2 *Luigi’s Mansion*
(PAL, `GLMP01`) WUP after regenerating the Wii U menu metadata and updating
the Nintendont loader on SD. The standalone Phase 1 build, global fallback,
non-autoboot, two-disc games and return to the Wii U menu still need testing.
