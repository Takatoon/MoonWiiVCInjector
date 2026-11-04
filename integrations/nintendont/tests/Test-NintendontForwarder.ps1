param(
    [string]$AssemblyPath = (Join-Path $PSScriptRoot '../../../Moon WiiVC Injector/bin/Release/net10.0/Moon WiiVC Injector.dll')
)

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
$dolDirectory = Join-Path $root 'Moon WiiVC Injector/Resources/Dol'
$dolPath = Join-Path $dolDirectory 'nintendont_v10lator.dol'
$expectedHash = '02103ae10d8ddd1e5f0f3e7f194f650b1333ea6464faf1be1a8319c854a34db2'

function Assert-Condition([bool]$Condition, [string]$Message) {
    if (!$Condition) { throw $Message }
}

$bytes = [System.IO.File]::ReadAllBytes($dolPath)
Assert-Condition ($bytes.Length -eq 262656) 'Unexpected DOL size.'
Assert-Condition ((Get-FileHash $dolPath -Algorithm SHA256).Hash -eq $expectedHash) 'Unexpected DOL hash.'

function Read-U32BE([int]$Offset) {
    $word = [byte[]]$bytes[$Offset..($Offset + 3)]
    if ([BitConverter]::IsLittleEndian) { [Array]::Reverse($word) }
    return [uint64][BitConverter]::ToUInt32($word, 0)
}

# DOL has seven text and eleven data sections. Verify file bounds, memory
# ranges, non-overlap and that the entry point belongs to executable text.
$entryPoint = Read-U32BE 0xE0
$sections = @()
$entryFound = $false
for ($index = 0; $index -lt 18; $index++) {
    $offset = Read-U32BE ($index * 4)
    $address = Read-U32BE (0x48 + $index * 4)
    $size = Read-U32BE (0x90 + $index * 4)
    if ($size -eq 0) { continue }
    Assert-Condition ($offset -ge 0x100 -and $offset + $size -le $bytes.Length) "Invalid section $index file range."
    Assert-Condition ($address -ge 0x80000000L -and $address + $size -le 0x94000000L) "Invalid section $index memory range."
    foreach ($section in $sections) {
        Assert-Condition ($offset + $size -le $section.Offset -or $offset -ge $section.End) 'Overlapping DOL file sections.'
        Assert-Condition ($address + $size -le $section.Address -or $address -ge $section.MemoryEnd) 'Overlapping DOL memory sections.'
    }
    $sections += @{ Offset = $offset; End = $offset + $size; Address = $address; MemoryEnd = $address + $size }
    if ($index -lt 7 -and $entryPoint -ge $address -and $entryPoint -lt $address + $size) { $entryFound = $true }
}
Assert-Condition $entryFound 'DOL entry point is outside executable text.'

# Exercise the actual production selection and extraction method.
$assembly = [System.Reflection.Assembly]::LoadFrom((Resolve-Path $AssemblyPath).Path)
$templateType = $assembly.GetType('Moon_WiiVC_Injector.Core.BaseIsoTemplate', $true)
$copyMethod = $templateType.GetMethod('CopyNintendontDol')
$testDirectory = Join-Path $root ('Moon WiiVC Injector/bin/forwarder-test-' + [Guid]::NewGuid().ToString('N'))
[System.IO.Directory]::CreateDirectory($testDirectory) | Out-Null
foreach ($case in @(
    @{ Disabled = $false; File = 'nintendont_v10lator.dol' },
    @{ Disabled = $true; File = 'nintendont_forwarder.dol' }
)) {
    $destination = Join-Path $testDirectory $case.File
    $copyMethod.Invoke($null, [object[]]@([string]$destination, [bool]$case.Disabled)) | Out-Null
    $sourceHash = (Get-FileHash (Join-Path $dolDirectory $case.File)).Hash
    Assert-Condition ((Get-FileHash $destination).Hash -eq $sourceHash) "Wrong resource for Disable Autoboot=$($case.Disabled)."
}

foreach ($resource in @('nintendont_default_autobooter.dol', 'wiivc_chan_booter.dol')) {
    $stream = $assembly.GetManifestResourceStream('Moon_WiiVC_Injector.Resources.Dol.' + $resource)
    Assert-Condition ($null -ne $stream) "Missing retained resource: $resource."
    try {
        $expectedBytes = [System.IO.File]::ReadAllBytes((Join-Path $dolDirectory $resource))
        $actualHash = [Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($stream))
        $expectedResourceHash = [Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($expectedBytes))
        Assert-Condition ($actualHash -eq $expectedResourceHash) "Retained resource changed: $resource."
    } finally { $stream.Dispose() }
}

Write-Output 'PASS: DOL structure/checksum, production DOL selection, retained legacy/NAND resources.'
Write-Output "Extracted test files: $testDirectory"
