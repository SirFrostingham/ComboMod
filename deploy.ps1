# ============================================================
# ComboMod — Deploy Script
# Copies mod files to the Core Keeper mods directory,
# clears the compile cache, and publishes to mod.io.
# ============================================================
# SECURITY: Keep this file private — it contains your mod.io API key.

$ModName    = "ComboMod"
$ModsDir    = "$env:USERPROFILE\AppData\LocalLow\Pugstorm\Core Keeper\Steam\10717115\mods"
$Dest       = "$ModsDir\$ModName"
$Source     = $PSScriptRoot   # the folder containing this script
$ComboZipOut = "$Source\$ModName.zip"

# mod.io config — credentials live in secrets.ps1 (git-ignored)
# Copy secrets.example.ps1 -> secrets.ps1 and fill in your values.
$secretsFile = Join-Path $PSScriptRoot "secrets.ps1"
if (-not (Test-Path $secretsFile)) {
    Write-Error "Missing secrets.ps1 -- copy secrets.example.ps1 to secrets.ps1 and fill in credentials."
    exit 1
}
. $secretsFile
$ModioGameId  = 5289
$ModioModId   = 5824265

# Individual mod.io IDs (placeholder values until each standalone mod exists)
$ModioModIdAllSkillPerks      = "5842927"
$ModioModIdAutoGatesAndDoors   = "5842881"
$ModioModIdBetterTextInput    = "5842929"
$ModioModIdExperienceTweaks   = "5842930"
$ModioModIdInfiniteOreBoulder = "5842933"
$ModioModIdInstantPortalCharge= "5842935"
$ModioModIdKeepInventory      = "5842937"
$ModioModIdMoreMapReveal      = "5842940"
$ModioModIdQuickUnlock        = "5842942"
$ModioModIdSolariteShovel     = "5842943"

$IndividualMods = @(
    [PSCustomObject]@{ Name = "All Skill Perks"; RelativePath = "ModsToFix\\All Skill Perks";                                 ZipName = "AllSkillPerks.zip";      ModId = $ModioModIdAllSkillPerks },
    [PSCustomObject]@{ Name = "AutoGatesAndDoors";       RelativePath = "ModsToFix\\AutoGatesAndDoors";                                       ZipName = "AutoGatesAndDoors.zip";          ModId = $ModioModIdAutoGatesAndDoors },
    [PSCustomObject]@{ Name = "Better Text Input";RelativePath = "ModsToFix\\Better Text Input";                              ZipName = "BetterTextInput.zip";    ModId = $ModioModIdBetterTextInput },
    [PSCustomObject]@{ Name = "Experience Tweaks";RelativePath = "ModsToFix\\Experience Tweaks";                              ZipName = "ExperienceTweaks.zip";   ModId = $ModioModIdExperienceTweaks },
    [PSCustomObject]@{ Name = "InfiniteOreBoulder";RelativePath = "ModsToFix\\InfiniteOreBoulder";                            ZipName = "InfiniteOreBoulder.zip"; ModId = $ModioModIdInfiniteOreBoulder },
    [PSCustomObject]@{ Name = "InstantPortalCharge";RelativePath = "ModsToFix\\InstantPortalCharge";                          ZipName = "InstantPortalCharge.zip";ModId = $ModioModIdInstantPortalCharge },
    [PSCustomObject]@{ Name = "Keep inventory on death (for dedicated Servers)"; RelativePath = "ModsToFix\\Keep inventory on death (for dedicated Servers)"; ZipName = "KeepInventoryOnDeath.zip"; ModId = $ModioModIdKeepInventory },
    [PSCustomObject]@{ Name = "More Map Reveal"; RelativePath = "ModsToFix\\More Map Reveal";                                 ZipName = "MoreMapReveal.zip";      ModId = $ModioModIdMoreMapReveal },
    [PSCustomObject]@{ Name = "Quick Unlock";    RelativePath = "ModsToFix\\Quick Unlock";                                    ZipName = "QuickUnlock.zip";        ModId = $ModioModIdQuickUnlock },
    [PSCustomObject]@{ Name = "Solarite Shovel"; RelativePath = "ModsToFix\\Solarite Shovel";                                 ZipName = "SolariteShovel.zip";     ModId = $ModioModIdSolariteShovel }
)

if ($IndividualMods.Count -ne 10) {
    throw "Expected 10 individual mod package definitions, found $($IndividualMods.Count)."
}

function Invoke-ModioUpload {
    param(
        [Parameter(Mandatory = $true)][string]$Label,
        [Parameter(Mandatory = $true)][string]$ModId,
        [Parameter(Mandatory = $true)][string]$ZipPath,
        [Parameter(Mandatory = $true)][string]$Version,
        [Parameter(Mandatory = $true)][string]$Changelog,
        [Parameter(Mandatory = $true)][string]$GameId,
        [Parameter(Mandatory = $true)][string]$OAuthToken
    )

    if (-not (Test-Path $ZipPath)) {
        Write-Warning "[$Label] Cannot upload missing zip: $ZipPath"
        return
    }

    Write-Host "[$Label] Publishing v$Version to mod.io (mod id: $ModId)..."

    $client = $null
    $multipart = $null
    $fileStream = $null

    try {
        Add-Type -AssemblyName System.Net.Http

        # File uploads require OAuth2 Bearer token (API keys are read-only).
        $uri = "https://api.mod.io/v1/games/$GameId/mods/$ModId/files"

        $client = New-Object System.Net.Http.HttpClient
        $client.DefaultRequestHeaders.Authorization =
            New-Object System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", $OAuthToken)

        $multipart = New-Object System.Net.Http.MultipartFormDataContent

        # Attach zip
        $fileStream = [System.IO.File]::OpenRead($ZipPath)
        $fileContent = New-Object System.Net.Http.StreamContent($fileStream)
        $fileContent.Headers.ContentType =
            [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse("application/zip")
        $multipart.Add($fileContent, "filedata", [System.IO.Path]::GetFileName($ZipPath))

        # Other fields
        $multipart.Add([System.Net.Http.StringContent]::new($Version),  "version")
        $multipart.Add([System.Net.Http.StringContent]::new($Changelog), "changelog")
        $multipart.Add([System.Net.Http.StringContent]::new("1"),       "active")

        $res  = $client.PostAsync($uri, $multipart).Result
        $body = $res.Content.ReadAsStringAsync().Result

        if ($res.IsSuccessStatusCode) {
            $json = $body | ConvertFrom-Json
            $uploadedVersion = [string]$json.version
            if ([string]::IsNullOrWhiteSpace($uploadedVersion)) {
                Write-Host "[$Label] mod.io upload OK - file id: $($json.id)"
            }
            else {
                Write-Host "[$Label] mod.io upload OK - file id: $($json.id), version: $uploadedVersion"
            }
        }
        else {
            Write-Warning "[$Label] mod.io upload failed (HTTP $([int]$res.StatusCode)): $body"
        }
    }
    catch {
        Write-Warning "[$Label] mod.io upload FAILED: $($_.Exception.Message)"
    }
    finally {
        if ($fileStream) { $fileStream.Dispose() }
        if ($multipart) { $multipart.Dispose() }
        if ($client) { $client.Dispose() }
    }
}

function Get-StampedManifestJson {
    param(
        [Parameter(Mandatory = $true)][string]$ManifestPath,
        [Parameter(Mandatory = $true)][string]$Version
    )

    $manifestObject = Get-Content $ManifestPath -Raw | ConvertFrom-Json
    $manifestObject | Add-Member -NotePropertyName version -NotePropertyValue $Version -Force
    return ($manifestObject | ConvertTo-Json -Depth 100)
}

function Update-EmbeddedScriptVersion {
    param(
        [Parameter(Mandatory = $true)][string]$FilePath,
        [Parameter(Mandatory = $true)][string]$Version,
        [string]$ForcedName
    )

    if (-not (Test-Path $FilePath)) {
        return
    }

    $content = Get-Content $FilePath -Raw
    $updated = $content

    if ($ForcedName) {
        $updated = [regex]::Replace(
            $updated,
            'public\s+const\s+string\s+MOD_NAME\s*=\s*"[^"]*"\s*;',
            ('public const string MOD_NAME = "{0}";' -f $ForcedName),
            1
        )
    }

    $updated = [regex]::Replace(
        $updated,
        'public\s+const\s+string\s+MOD_VERSION\s*=\s*"[^"]*"\s*;',
        ('public const string MOD_VERSION = "{0}";' -f $Version),
        1
    )

    if ($updated -ne $content) {
        Set-Content -Path $FilePath -Value $updated -Encoding UTF8
    }
}

# ---- 1. Read + bump deploy version ----------------------------
# ComboMod is a pack of many files (no single ComboMod.cs), so keep deploy
# version in a dedicated file used for mod.io uploads.
$VersionFile = Join-Path $Source "deploy.version.txt"
if (-not (Test-Path $VersionFile)) {
    $oldVersion = "0.0.0"
} else {
    $oldVersion = (Get-Content $VersionFile -Raw).Trim()
    if (-not $oldVersion) { $oldVersion = "0.0.0" }
}

if ($oldVersion -notmatch '^\d+\.\d+\.\d+$') {
    throw "deploy.version.txt must be in x.y.z format. Found: $oldVersion"
}

$parts = $oldVersion -split '\.'
$parts[-1] = [int]$parts[-1] + 1
$newVersion = ($parts -join '.')
Set-Content -Path $VersionFile -Value $newVersion
Write-Host "[ComboMod] Version bumped: $oldVersion -> $newVersion"

# ---- Build the staged layout from root ModManifest.json -------
$manifestPath = Join-Path $Source "ModManifest.json"
if (-not (Test-Path $manifestPath)) {
    throw "Missing ModManifest.json at $manifestPath"
}

$manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
if (-not $manifest.files -or $manifest.files.Count -eq 0) {
    throw "ModManifest.json has no files[] entries to stage."
}

$StagedRoot = Join-Path $Source "_staged"
$Staged     = Join-Path $StagedRoot $ModName
if (Test-Path $StagedRoot) { Remove-Item $StagedRoot -Recurse -Force }
New-Item -ItemType Directory -Path $Staged | Out-Null

# Always include the root ModManifest
Copy-Item $manifestPath (Join-Path $Staged "ModManifest.json") -Force

# Copy each file listed in files[] to the same relative path under the staged dir
foreach ($entry in $manifest.files) {
    $relativePath = ($entry.path -replace '/', '\')
    $srcPath      = Join-Path $Source $relativePath

    if (-not (Test-Path $srcPath)) {
        throw "Manifest entry not found on disk: $relativePath"
    }

    $destPath = Join-Path $Staged $relativePath
    $destDir  = Split-Path $destPath -Parent
    if (-not (Test-Path $destDir)) {
        New-Item -ItemType Directory -Path $destDir -Force | Out-Null
    }

    Copy-Item $srcPath $destPath -Force
}

# Embed deploy/build metadata in staged output so each published zip is uniquely
# versioned (prevents remote hosts from reusing a previous identical artifact).
$gitCommit = "unknown"
try {
    $gitCommitOutput = (& git -C $Source rev-parse --short HEAD 2>$null)
    if ($LASTEXITCODE -eq 0 -and $gitCommitOutput) {
        $gitCommit = ($gitCommitOutput | Select-Object -First 1).Trim()
    }
}
catch {
    # Non-fatal: keep git commit as "unknown" when git is unavailable.
}

$buildInfoPath = Join-Path $Staged "_combomod.build.txt"
$buildInfo = @(
    "version=$newVersion"
    "built_utc=$([DateTime]::UtcNow.ToString('o'))"
    "git_commit=$gitCommit"
) -join [Environment]::NewLine
Set-Content -Path $buildInfoPath -Value $buildInfo -Encoding UTF8

# Also copy deploy.version.txt into the staged package for traceability.
Copy-Item $VersionFile (Join-Path $Staged "deploy.version.txt") -Force

Write-Host "[ComboMod] Staged to: $Staged"

# ---- 2. Install to local mods directory ---------------------
if (Test-Path $Dest) {
    Write-Host "[ComboMod] Removing existing install at: $Dest"
    Remove-Item $Dest -Recurse -Force
}
# Create dest dir and copy contents directly (avoids double-nesting if Dest survives Remove-Item)
New-Item -ItemType Directory -Path $Dest -Force | Out-Null
Copy-Item "$Staged\*" $Dest -Recurse -Force
Write-Host "[ComboMod] Installed to: $Dest"

# Move duplicate local installs that share the same manifest/mod name out of
# the active mods directory.
# Example problematic folder: "ComboMod (10 mods WORKING!)".
# Having multiple folders with name "ComboMod" in ModManifest can cause stale
# scripts to be compiled/loaded unexpectedly.
$destLower = $Dest.ToLowerInvariant()
$duplicateInstalls = Get-ChildItem -Path $ModsDir -Directory |
    Where-Object {
        $_.Name -like "$ModName*" -and $_.FullName.ToLowerInvariant() -ne $destLower
    }

$DisabledModsDir = "$ModsDir`_disabled"
if (-not (Test-Path $DisabledModsDir)) {
    New-Item -ItemType Directory -Path $DisabledModsDir -Force | Out-Null
}

foreach ($dup in $duplicateInstalls) {
    $disabledName = $dup.Name
    $disabledPath = Join-Path $DisabledModsDir $disabledName
    if (Test-Path $disabledPath) {
        $disabledName = "$($dup.Name)_$(Get-Date -Format 'yyyyMMddHHmmss')"
        $disabledPath = Join-Path $DisabledModsDir $disabledName
    }

    Move-Item -LiteralPath $dup.FullName -Destination $disabledPath -Force
    Write-Host "[ComboMod] Moved duplicate install out of active mods: $($dup.Name) -> $disabledPath"
}

# ---- 2b. Delete compiled mod caches -------------------------
# The game caches compiled scripts under LocalAppData\Temp\...\ModLoader.
# If stale cache folders remain from previous standalone installs (or older
# ComboMod layouts), Core Keeper can keep compiling/loading old script content.
# Delete ComboMod cache + known bundled-module cache folders to force a clean
# Roslyn compile on next game start.
$CacheRoot = "$env:LOCALAPPDATA\Temp\Pugstorm\Core Keeper\ModLoader"
$CacheTargets = @(
    $ModName,
    "all-skill-perks",
    "AutoGatesAndDoors",
    "BetterTextInput",
    "ExperienceTweaks",
    "InfiniteOreBoulder",
    "InstantPortalCharge",
    "Keep Inventory On Death",
    "MoreMapReveal",
    "quick-unlock",
    "Solarite Shovel"
) | Select-Object -Unique

$clearedAnyCache = $false
foreach ($target in $CacheTargets) {
    $cachePath = Join-Path $CacheRoot $target
    if (Test-Path $cachePath) {
        Remove-Item $cachePath -Recurse -Force
        Write-Host "[ComboMod] Cleared mod compile cache: $cachePath"
        $clearedAnyCache = $true
    }
}

if (-not $clearedAnyCache) {
    Write-Host "[ComboMod] No matching compile cache folders found (clean slate)."
}

# ---- 3. Create distributable zips ---------------------------
# ComboMod zip: use "$Staged\*" (not $Staged) so the zip root contains
# ModManifest.json + Scripts/ directly, without a ComboMod\ wrapper folder.
$ZipTmp = "$Source\_ComboMod_new.zip"
if (Test-Path $ZipTmp) { Remove-Item $ZipTmp -Force }
Compress-Archive -Path "$Staged\*" -DestinationPath $ZipTmp -Force
if (-not (Test-Path $ZipTmp)) { throw "Zip was not created: $ZipTmp" }
if (Test-Path $ComboZipOut) { Remove-Item $ComboZipOut -Force -ErrorAction SilentlyContinue }
Move-Item $ZipTmp $ComboZipOut -Force
$zipSize = (Get-Item $ComboZipOut).Length
Write-Host "[ComboMod] Zip created: $ComboZipOut ($zipSize bytes, $(Get-Date -Format 'HH:mm:ss'))"

# Individual mod zips:
# Create each zip FROM inside its own mod directory so the zip root starts
# with that mod's files/folders, then move the finished zip to project root.
$CreatedIndividualZips = @()
foreach ($mod in $IndividualMods) {
    $modDir = Join-Path $Source $mod.RelativePath
    if (-not (Test-Path $modDir)) {
        throw "[$($mod.Name)] Missing mod directory: $modDir"
    }

    $modZipOut = Join-Path $Source $mod.ZipName
    $modZipTmp = Join-Path $modDir ("_$([System.IO.Path]::GetFileNameWithoutExtension($mod.ZipName))_new.zip")
    $modManifestPath = Join-Path $modDir "ModManifest.json"

    if (-not (Test-Path $modManifestPath)) {
        throw "[$($mod.Name)] Missing ModManifest.json: $modManifestPath"
    }

    $updatedManifestJson = Get-StampedManifestJson -ManifestPath $modManifestPath -Version $newVersion
    Set-Content -Path $modManifestPath -Value $updatedManifestJson -Encoding UTF8

    if ($mod.Name -eq "AutoGatesAndDoors") {
        Update-EmbeddedScriptVersion -FilePath (Join-Path $modDir "Scripts\Mod.cs") -Version $newVersion -ForcedName "AutoGatesAndDoors"
    }

    if (Test-Path $modZipTmp) { Remove-Item $modZipTmp -Force -ErrorAction SilentlyContinue }
    if (Test-Path $modZipOut) { Remove-Item $modZipOut -Force -ErrorAction SilentlyContinue }

    Push-Location $modDir
    try {
        Compress-Archive -Path "*" -DestinationPath $modZipTmp -Force
    }
    finally {
        Pop-Location
    }

    if (-not (Test-Path $modZipTmp)) {
        throw "[$($mod.Name)] Zip was not created: $modZipTmp"
    }

    [System.IO.File]::Copy($modZipTmp, $modZipOut, $true)
    Remove-Item $modZipTmp -Force -ErrorAction SilentlyContinue
    $indZipSize = (Get-Item $modZipOut).Length
    Write-Host "[$($mod.Name)] Zip created from mod dir: $modZipOut ($indZipSize bytes, $(Get-Date -Format 'HH:mm:ss'))"

    $CreatedIndividualZips += [PSCustomObject]@{
        Name    = $mod.Name
        ModId   = [string]$mod.ModId
        ZipPath = $modZipOut
    }
}

if ($CreatedIndividualZips.Count -ne 10) {
    throw "Expected 10 individual mod zips, created $($CreatedIndividualZips.Count)."
}

# ---- 4. Publish to mod.io ----------------------------------
$modVersion = $newVersion
$changelog  = "Auto-deployed v$modVersion via deploy.ps1"

if (-not $ModioOAuthToken) {
    Write-Warning "[mod.io] Skipping publish -- set ModioOAuthToken in secrets.ps1."
    Write-Warning "         Get a token at: https://mod.io/me/access  (OAuth 2 Access -> Generate Token)"
}
else {
    # Upload ComboMod
    $comboUploadParams = @{
        Label      = "ComboMod"
        ModId      = [string]$ModioModId
        ZipPath    = $ComboZipOut
        Version    = $modVersion
        Changelog  = $changelog
        GameId     = [string]$ModioGameId
        OAuthToken = $ModioOAuthToken
    }
    Invoke-ModioUpload @comboUploadParams

    # Upload all 10 individual mods (currently expected to fail until real IDs are set)
    foreach ($modUpload in $CreatedIndividualZips) {
        $modUploadParams = @{
            Label      = $modUpload.Name
            ModId      = $modUpload.ModId
            ZipPath    = $modUpload.ZipPath
            Version    = $modVersion
            Changelog  = "Auto-deployed v$modVersion via deploy.ps1 ($($modUpload.Name))"
            GameId     = [string]$ModioGameId
            OAuthToken = $ModioOAuthToken
        }
        Invoke-ModioUpload @modUploadParams
    }
}

# ---- 5. Cleanup staged folder --------------------------------
if (Test-Path $StagedRoot) {
    Remove-Item $StagedRoot -Recurse -Force
}

Write-Host ""
Write-Host "Done!"
Write-Host "  Local install        : $Dest"
Write-Host "  Combo zip            : $ComboZipOut"
Write-Host "  Individual zip count : $($CreatedIndividualZips.Count)"
Write-Host "  mod.io page          : https://mod.io/g/corekeeper/m/combomod"
