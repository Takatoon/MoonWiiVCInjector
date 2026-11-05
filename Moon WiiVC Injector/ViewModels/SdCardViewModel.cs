using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Moon_WiiVC_Injector.Properties;
using Moon_WiiVC_Injector.Core.Nintendont;
using Moon_WiiVC_Injector.Services;

namespace Moon_WiiVC_Injector.ViewModels;

internal sealed partial class SdCardViewModel : ViewModelBase
{
    // ==========================================
    // 1. Services & Dependencies
    // ==========================================
    private readonly IDialogService _dialogService;

    // ==========================================
    // 2. Observable Properties (UI State)
    // ==========================================
    public ObservableCollection<string> Drives
    {
        get;
        set => SetProperty(ref field, value);
    } = [];

    public string? SelectedDrive
    {
        get;
        set => SetProperty(ref field, value);
    }

    public int SelectedDriveIndex
    {
        get;
        set => SetProperty(ref field, value);
    } = -1;

    public NintendontConfigViewModel NintendontConfig { get; } = new();

    public string ActionStatus
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    // ==========================================
    // 4. Constructor
    // ==========================================
    public SdCardViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
        ReloadDrives();
    }

    // ==========================================
    // 5. Commands (UI Actions)
    // ==========================================
    [RelayCommand]
    public void ReloadDrives()
    {
        try
        {
            var driveList = new List<string>();

            if (OperatingSystem.IsMacOS())
            {
                if (Directory.Exists("/Volumes"))
                {
                    foreach (string dir in Directory.GetDirectories("/Volumes"))
                    {
                        string name = Path.GetFileName(dir);
                        if (!string.Equals(name, "Macintosh HD", StringComparison.OrdinalIgnoreCase))
                        {
                            driveList.Add(dir);
                        }
                    }
                }
            }
            else
            {
                var drives = DriveInfo.GetDrives()
                    .Where(d => d.IsReady && (d.DriveType == DriveType.Removable ||
                                (OperatingSystem.IsLinux() && (d.RootDirectory.FullName.StartsWith("/media") || d.RootDirectory.FullName.StartsWith("/run/media")))))
                    .Select(d => string.IsNullOrWhiteSpace(d.VolumeLabel) ? d.Name : $"{d.Name} ({d.VolumeLabel})")
                    .ToList();
                driveList.AddRange(drives);
            }

            Drives = new ObservableCollection<string>(driveList);
            if (Drives.Count > 0)
            {
                SelectedDriveIndex = 0;
                SelectedDrive = Drives[0];
            }
            else
            {
                SelectedDriveIndex = -1;
                SelectedDrive = null;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SdCardViewModel] Failed to reload drives: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task UpdateNintendontAsync()
    {
        string? driveLetter = GetSelectedDriveLetter();
        bool driveSpecified = !string.IsNullOrEmpty(driveLetter);

        string downloadPath = Path.Combine(Settings.Default.GetEffectiveTempPath(), "SOURCETEMP", "Download");
        string tempPath = Path.Combine(downloadPath, "apps", "nintendont");
        string sdPath = driveSpecified ? Path.Combine(driveLetter!, "apps", "nintendont") : string.Empty;

        if (!await Program.CheckForInternetConnectionAsync())
        {
            var res = await _dialogService.ShowMessageAsync(
                "Your internet connection could not be verified, do you wish to try and download Nintendont anyway?",
                "Internet Connection Verification Failed",
                MessageBoxButtons.YesNo);
            if (res == MessageBoxResult.No) return;
        }

        ActionStatus = "Downloading...";
        try
        {
            await Task.Run(async () =>
            {
                Directory.CreateDirectory(tempPath);
                var client = Program.Client;
                async Task DownloadFileAsync(string url, string path)
                {
                    using var stream = await client.GetStreamAsync(new Uri(url));
                    using var file = File.Create(path);
                    await stream.CopyToAsync(file);
                }

                await DownloadFileAsync("https://raw.githubusercontent.com/FIX94/Nintendont/master/loader/loader.dol", Path.Combine(tempPath, "boot.dol"));
                await DownloadFileAsync("https://raw.githubusercontent.com/FIX94/Nintendont/master/nintendont/meta.xml", Path.Combine(tempPath, "meta.xml"));
                await DownloadFileAsync("https://raw.githubusercontent.com/FIX94/Nintendont/master/nintendont/icon.png", Path.Combine(tempPath, "icon.png"));
            });
        }
        catch (Exception ex)
        {
            ActionStatus = string.Empty;
            await _dialogService.ShowMessageAsync($"Failed to download Nintendont: {ex.Message}", "Error", MessageBoxButtons.Ok);
            return;
        }
        ActionStatus = string.Empty;

        if (driveSpecified)
        {
            try
            {
                if (Directory.Exists(sdPath)) Directory.Delete(sdPath, true);
                Directory.CreateDirectory(sdPath);
                var dir = new DirectoryInfo(tempPath);
                foreach (var file in dir.GetFiles())
                {
                    var outPath = Path.Combine(sdPath, file.Name);
                    file.CopyTo(outPath, true);
                }

                await _dialogService.ShowMessageAsync("Download complete.", "Success", MessageBoxButtons.Ok);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowMessageAsync($"Failed to copy files to SD card: {ex.Message}", "Error", MessageBoxButtons.Ok);
            }
        }
        else
        {
            var dialogResult = await _dialogService.ShowMessageAsync(
                "SD Card not specified.\nDo you wish to save Nintendont somewhere else?",
                "Drive not specified",
                MessageBoxButtons.YesNo);

            if (dialogResult == MessageBoxResult.Yes)
            {
                var folder = await _dialogService.OpenFolderDialogAsync("Select folder to save Nintendont");
                if (!string.IsNullOrEmpty(folder))
                {
                    DateTime dateTime = DateTime.UtcNow.Date;
                    string zipPath = Path.Combine(folder, $"Nintendont-{dateTime:dd.MMM.yyyy}.zip");
                    if (File.Exists(zipPath)) File.Delete(zipPath);
                    await Task.Run(() => ZipFile.CreateFromDirectory(downloadPath, zipPath));
                    await _dialogService.ShowMessageAsync("Download complete.", "Success", MessageBoxButtons.Ok);
                }
            }
        }
    }

    [RelayCommand]
    public async Task GenerateConfigAsync()
    {
        string? driveLetter = GetSelectedDriveLetter();
        bool driveSpecified = !string.IsNullOrEmpty(driveLetter);

        string savePath = driveSpecified ? Path.Combine(driveLetter!, "nincfg.bin") : string.Empty;

        if (!driveSpecified)
        {
            var dialogResult = await _dialogService.ShowMessageAsync(
                "SD card not specified.\nDo you wish to save the file somewhere else?",
                "Drive not specified",
                MessageBoxButtons.YesNo);

            if (dialogResult == MessageBoxResult.Yes)
            {
                var folder = await _dialogService.OpenFolderDialogAsync("Select folder to save nincfg.bin");
                if (!string.IsNullOrEmpty(folder))
                {
                    savePath = Path.Combine(folder, "nincfg.bin");
                }
                else return;
            }
            else return;
        }

        try
        {
            // The same editor/model/writer is used for embedded and SD configs.
            // A global config does not force autoboot or a particular game path.
            byte[] bytes = NintendontConfigSerializer.Serialize(NintendontConfig.ToConfig());
            if (File.Exists(savePath))
            {
                var overwrite = await _dialogService.ShowMessageAsync(
                    "Replace the existing nincfg.bin? A backup will be saved as nincfg.bin.bak.",
                    "Replace global configuration", MessageBoxButtons.YesNo);
                if (overwrite != MessageBoxResult.Yes) return;
            }

            string directory = Path.GetDirectoryName(Path.GetFullPath(savePath))!;
            string temporaryPath = Path.Combine(directory, ".nincfg-" + Guid.NewGuid().ToString("N") + ".tmp");
            try
            {
                await File.WriteAllBytesAsync(temporaryPath, bytes);
                if (File.Exists(savePath)) File.Copy(savePath, savePath + ".bak", true);
                File.Move(temporaryPath, savePath, overwrite: true);
            }
            finally
            {
                if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
            }

            await _dialogService.ShowMessageAsync("Config generation complete.", "Information", MessageBoxButtons.Ok);
        }
        catch (Exception ex)
        {
            await _dialogService.ShowMessageAsync($"Failed to save config: {ex.Message}", "Error", MessageBoxButtons.Ok);
        }
    }

    // ==========================================
    // 6. Private Helper Methods
    // ==========================================
    private string? GetSelectedDriveLetter()
    {
        if (string.IsNullOrWhiteSpace(SelectedDrive)) return null;

        if (OperatingSystem.IsWindows())
        {
            if (SelectedDrive.Length >= 3 && SelectedDrive[1] == ':' && (SelectedDrive[2] == '\\' || SelectedDrive[2] == '/'))
            {
                return SelectedDrive[..3];
            }
            return SelectedDrive;
        }

        int parenIdx = SelectedDrive.IndexOf(" (", StringComparison.Ordinal);
        string path = parenIdx > 0 ? SelectedDrive[..parenIdx].Trim() : SelectedDrive.Trim();
        return Directory.Exists(path) ? path : null;
    }
}
