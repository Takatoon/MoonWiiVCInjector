# 🚀 Moon WiiVC Injector - Modernized Edition

## Nintendont per-game configuration — development roadmap

This fork aims to add per-game Nintendont configuration support to Moon WiiVC Injector, with changes developed incrementally for potential contribution to the upstream project.

The integration builds on **[V10lator's Nintendont autoboot forwarder](https://github.com/V10lator/nintendont-autoboot-forwarder)**, based on the original work by **[FIX94](https://github.com/FIX94/nintendont-autoboot-forwarder)**. Credit for the forwarder and its configuration loading behavior belongs to its authors.

**Status:** Phase 1 is implemented. Its default forwarder booted on a real Wii U as part of a Phase 2 test injection. Phases 2, 3 and 4 are planned for this branch.

### Phase 1 — Integrate V10lator's forwarder

The default GameCube autobooter is replaced with V10lator's forwarder, without modifying its source code.

The existing **Disable Autoboot** checkbox selects the DOL included in new GameCube injections:

| Disable Autoboot | Included DOL |
|---|---|
| Unchecked | V10lator's autoboot forwarder |
| Checked | Existing non-autoboot Nintendont forwarder |

The previous default autobooter remains in the repository resources for reference and recovery.

V10lator's forwarder searches for a configuration in this order:

1. `SD:/nintendont/configs/XXXX.bin` — game-specific configuration.
2. `nincfg.bin` embedded in the injection.
3. `SD:/nincfg.bin` — global configuration.

`XXXX` comes from the first four characters of the GameCube disc ID, not the Wii U Title ID. Configurations are selected by priority; their fields are not merged.

Phase 1 uses externally prepared configuration files. It does not add an editor or embed configurations. Nintendont must still be installed at `SD:/apps/nintendont/boot.dol`.

Use a correctly generated **CFG v10 (548-byte)** file for the pinned forwarder. Older 544-byte files may fail before Nintendont can migrate them. Keep a backup and regenerate or explicitly convert them with a compatible tool; do not simply pad them or change the version number. Without a readable configuration, the forwarder can fail to start. The existing Moon global CFG writer has known incomplete/incorrect option mappings and is scheduled for correction in Phase 2.

See [forwarder provenance, source, license and rebuild instructions](integrations/nintendont/README.md) for the exact revisions, checksum and validation limits.

### Phase 2 — Embed a configuration in each injection

Add a **Nintendont** tab for GameCube injections, with an optional configuration editor based on the existing SD Card Menu.

- Embedding will be disabled by default.
- When enabled, Moon will generate and include `nincfg.bin` in the injection.
- A game-specific configuration on SD will take precedence.
- A shared, tested serializer will also correct the existing global configuration generator.

### Phase 3 — Manage configurations on SD

Extend the **Nintendont SD Card Menu** to manage both global and
per-game configurations directly on an SD card.

- Select between global configuration (`SD:/nincfg.bin`) and per-game
  configurations (`SD:/nintendont/configs/XXXX.bin`).
- Create, load, import, edit, export and delete configurations.
- Show game names and regions when a matching database entry is
  available, retaining the filename for unidentified games.
- Reuse the editor, validator and serializer from Phase 2.
- Handle incompatible configuration versions explicitly, confirm
  overwrites and deletions, and preserve backups.

The existing Nintendont download/update action will remain available.
File access will be separated from configuration management so the
same workflow can support FTP in Phase 4.

### Phase 4 — Manage the console SD card over FTP

Add FTP as an alternative connection method in the same menu.

- Connect using the console IP, with advanced options for the port
  and transfer mode.
- Automatically detect the SD card root exposed by the FTP server.
- Manage global and per-game configurations using the same editor
  and operations available for a locally connected SD card.
- Download and update Nintendont on the console SD card.
- Remember connection settings locally for future sessions.

The initial scope targets anonymous connections provided by FTPiiU
and the Aroma FTPiiU plugin. No username or password fields are planned.
Local SD access will remain available without an FTP connection.

### Compatibility and validation

The integration pins the forwarder revision and documents its build, configuration format and binary checksum. The initial reference is CFG v10; older files must not be assumed compatible.

**Wii U test:** A *Luigi’s Mansion* (PAL, `GLMP01`) WUP built during Phase 2 booted with the same V10lator forwarder used by this branch. The test used rebuilt Wii U `meta.xml` metadata and an updated Nintendont loader on SD. The standalone Phase 1 build, global fallback and the non-autoboot path still need hardware testing.

Rebuild and reinstall existing injections to use the new forwarder or regenerate their Wii U metadata.

Changes to Wii Retail, Wii Homebrew and vWii NAND injection behavior, and unrelated GamePad patches, are outside this work.

---

[![Build Status](https://img.shields.io/badge/Build-Passing-success?style=for-the-badge&logo=.net&color=31c754)](https://github.com/Rodrigo-Matsuura/TeconmoonWiiVCInjector)
[![Framework](https://img.shields.io/badge/.NET-10.0%20(LTS)-512bd4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![UI Framework](https://img.shields.io/badge/UI-Avalonia%2012-purple?style=for-the-badge&logo=avalonia)](https://avaloniaui.net/)
[![Vibe Coding](https://img.shields.io/badge/Developed%20with-Vibe%20Coding-pink?style=for-the-badge&logo=ai)](https://github.com/Rodrigo-Matsuura/TeconmoonWiiVCInjector)
[![License](https://img.shields.io/badge/License-GPLv3-blue?style=for-the-badge)](LICENSE)

A modernized, optimized, and cross-platform fork of the classic **[piratesephiroth/TeconmoonWiiVCInjector](https://github.com/piratesephiroth/TeconmoonWiiVCInjector)**, maintained under **[Rodrigo-Matsuura/TeconmoonWiiVCInjector](https://github.com/Rodrigo-Matsuura/TeconmoonWiiVCInjector)**.

This project completely rewrites and modernizes the original Wii and GameCube Virtual Console Injector for Wii U. The tool has been re-architected from the ground up to eliminate legacy technical debt, adopt modern asynchronous C# and .NET standards, implement a decoupled MVVM architecture, and deliver major performance and user experience improvements.

---

## 🎯 Project Focus & Vision

The primary mission of this modernized edition is to provide a **clean, fast, portable, and reliable** injection experience:

1. **Modern Foundation**: Fully powered by **.NET 10 (LTS)** and **Avalonia UI**, removing dependencies on legacy Windows Forms and obsolete .NET Framework runtimes.
2. **Robust Architecture**: Built with clean separation of concerns using the **MVVM (Model-View-ViewModel)** pattern and dependency-injected services.
3. **High Efficiency**: Minimized memory footprint and CPU overhead during heavy ISO extraction and NFS repackaging.
4. **Enhanced UX**: Modern desktop interactions including Drag & Drop, real-time logging, and safe task cancellation.
5. **Zero-Elevation Portability**: Completely self-contained and portable with no administrator privileges or Windows Registry dependencies.

---

## ⚡ Key Improvements Over the Original

### 🖥️ 1. Modern Cross-Platform UI & MVVM Architecture
* **Avalonia UI Modernization**: Replaced legacy Windows Forms with Avalonia UI for clean rendering, consistent themes, and cross-platform UI capabilities.
* **CommunityToolkit.Mvvm**: Clean separation between presentation and business logic using reactive ViewModels, observable properties, and asynchronous commands.
* **Decoupled Dialog Services**: Abstracted file pickers, folder dialogs, and notifications behind a mockable and testable `IDialogService`.

### 🚀 2. Performance & Low-Level Memory Optimization
* **`ArrayPool<byte>` Buffer Management**: Reusable buffer pooling in `Nfs2Iso2Nfs` drastically reduces Garbage Collector (GC) allocations and memory spikes when handling multi-gigabyte disk images.
* **Instant In-Memory GameTDB Caching**: Game database queries (`wiitdb.txt`) are parsed once into static in-memory lookup dictionaries (`O(1)`), eliminating slow, line-by-line disk reads.
* **Singleton `HttpClient` Stack**: Replaced deprecated `WebClient` and `HttpWebRequest` with an optimized, pooled `HttpClient` for fast remote banner/sound downloads and lightweight `HEAD` connectivity checks.

### 🎮 3. Expanded Game Format & Toolchain Support
* **Out-of-the-Box Compression Support**: Native support for compressed disk images, including **NKIT** (`.nkit.iso`) and **NASOS** (`.iso.dec` / `.dec`), without requiring manual decompression.
* **Native Pure C# WUP Packaging Engine**: Integrated an in-process packaging engine based on **[CNUS_Packer](https://github.com/NicoAICP/CNUS_Packer)**, completely eliminating external Java (`java.exe`) and `NUSPacker.jar` execution during WUP package generation with native exception handling, real-time Avalonia UI progress reporting, and multi-run state safety.
* **Smart Tool & JAR Resolution**: Enhanced process launcher automatically discovers local dependencies and native binaries before falling back to system environments or Wine wrappers.

### ✨ 4. Enhanced User Experience & Controls
* **Drag-and-Drop Workflow**: Drag game ROMs, icons, banners, and boot sounds directly into the interface.
* **Build Cancellation Support**: Safely cancel long conversion processes at any moment with automatic cleanup of temporary files.
* **Real-Time Logs Viewer**: Dedicated in-app logging tab powered by `AppLogger` to inspect conversion progress and diagnose issues easily.

### 🔒 5. Portability, Cleanliness & Safety
* **Registry-Free Settings**: All encryption keys (Wii Common Key, Title Key, Ancast Key) and user preferences are stored in local configuration files rather than the Windows Registry.
* **No Administrator Privileges Required**: Runs fully with standard user permissions.
* **Clean Codebase**: 0 warnings, 0 errors, modernized with file-scoped namespaces and nullable reference types enabled.

---

## 🛠️ How to Build and Run

### Prerequisites
* **[.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)** or higher (if building from source).
* **Java Runtime Environment (JRE or JDK 17 or higher)**: While the Wii U WUP packaging engine (`NUSPacker`) has been completely modernized into native in-process C#, Java is still currently required because other tools in the toolchain have not yet been migrated (specifically `JNUSTool`, which downloads clean base files from Nintendo NUS). Ensure `java` is accessible from your system PATH.

### Run Locally
```bash
dotnet run --project "Moon WiiVC Injector"
```

### Build Solution
```bash
# Build in Release mode
dotnet build "Moon WiiVC Injector.sln" -c Release
```

### Publish Self-Contained Executables

#### Windows (x64)
```bash
dotnet publish "Moon WiiVC Injector/Moon WiiVC Injector.csproj" \
  -c Release \
  -r win-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -o publish/win-x64
```

#### Linux (x64)
```bash
dotnet publish "Moon WiiVC Injector/Moon WiiVC Injector.csproj" \
  -c Release \
  -r linux-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -o publish/linux-x64
```

---

## 🤝 Contributing

This project is built and maintained with **Vibe Coding**. Contributions, optimizations, and ideas are warmly welcomed! If you spot any unexpected behavior, want to suggest additional optimizations, or port and modernize other aspects of the codebase, feel free to open an **Issue** or submit a **Pull Request (PR)**!

---

## 🏆 Credits & Acknowledgements

Special thanks and full attribution to the authors and projects of the Wii, Wii U, and GameCube homebrew communities that made this injector possible:

* **[piratesephiroth](https://github.com/piratesephiroth/TeconmoonWiiVCInjector/commits?author=piratesephiroth)**: Maintainer of the enhanced upstream fork, contributing numerous bug fixes, stability improvements, and features.
* **[TeconMoon](https://github.com/TeconMoon/teconmoon-wiivc-injector)**: Creator of the original *TeconMoon's WiiVC Injector*.
* **[QuzarDC](https://github.com/piratesephiroth/TeconmoonWiiVCInjector/issues?q=is%3Apr+is%3Aopen+author%3AQuzarDC)**: Contributor of upstream improvements
* **[V10lator](https://github.com/V10lator)**: Author of the [Nintendont autoboot forwarder](https://github.com/V10lator/nintendont-autoboot-forwarder) targeted by this fork's per-game configuration roadmap, and contributor of upstream improvements.
* **[FIX94](https://github.com/FIX94)**:
  * **[NFS2ISO2NFS](https://github.com/FIX94/NFS2ISO2NFS)**: Wii U NFS partition extraction and ISO manipulation.
  * **[GetExtTypePatcher](https://github.com/FIX94/GetExtTypePatcher)**: PowerPC Classic Controller patcher for `main.dol` (native C# port in `DolPatcher.cs`).
  * **[c2w_patcher](https://github.com/FIX94/c2w_patcher)**: Cafe2Wii Ancast image patcher for vWii (native C# port in `C2wPatcher.cs`).
  * **[Nintendont](https://github.com/FIX94/Nintendont)**: GameCube autoboot and forwarder DOL loaders.
* **[Tim Ogus (timogus)](https://bitbucket.org/timogus/wav2btsnd)**: Creator of `wav2btsnd` (native C# lossless audio port in `BtsndConverter.cs`).
* **[Nanook](https://github.com/Nanook/NKit)**: Creator of `NKit` (Nintendo GameCube and Wii disc image compression/recovery tool).
* **[Wiimms (wiimm)](https://wit.wiimm.de/)**: Creator of `WIT` (Wiimms ISO Tools) for Wii/GC disc and partition handling.
* **[Maschell](https://github.com/Maschell)**:
  * **[JNUSTool](https://github.com/Maschell/JNUSTool)**: Nintendo NUS download client.
  * **[NUSPacker](https://github.com/Maschell/NUSPacker)**: Wii U WUP package encryption and generation.
* **[NicoAICP](https://github.com/NicoAICP)** & **[Morilli](https://github.com/Morilli)**: Creators of **[CNUS_Packer](https://github.com/NicoAICP/CNUS_Packer)**, the native C# port of NUSPacker used as the foundation for the built-in WUP packaging engine in `MoonWiiVCInjector` (eliminating the dependency on external Java runtime for package generation).
* **[Waninkoko](https://github.com/waninkoko)**: Creator of `wii-vmc` (Wii Video Mode Changer).
* **[GameTDB / WiiTDB](https://www.gametdb.com/)**: Comprehensive video game metadata database for Wii and GameCube titles.

---

## 📄 License

This project is licensed under the **GNU General Public License v3.0 (GPLv3)**. See the [LICENSE](LICENSE) file for details.
