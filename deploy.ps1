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
$ZipOut     = "$Source\$ModName.zip"

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
    "AutoDoor",
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

# ---- 3. Create distributable zip ----------------------------
# Use "$Staged\*" (not $Staged) so the zip root contains ModManifest.json + Scripts/
# directly, without a ComboMod\ wrapper folder. mod.io places the zip contents
# into the mod folder automatically, so the extra wrapper causes double-nesting.
# Write to a temp file first, then replace — avoids lock failures when VS Code
# or Explorer has the previous zip open.
$ZipTmp = "$Source\_ComboMod_new.zip"
if (Test-Path $ZipTmp) { Remove-Item $ZipTmp -Force }
Compress-Archive -Path "$Staged\*" -DestinationPath $ZipTmp -Force
if (-not (Test-Path $ZipTmp)) { throw "Zip was not created: $ZipTmp" }
if (Test-Path $ZipOut) { Remove-Item $ZipOut -Force -ErrorAction SilentlyContinue }
Move-Item $ZipTmp $ZipOut -Force
$zipSize = (Get-Item $ZipOut).Length
Write-Host "[ComboMod] Zip created: $ZipOut ($zipSize bytes, $(Get-Date -Format 'HH:mm:ss'))"

# ---- 4. Publish to mod.io ----------------------------------
$modVersion = $newVersion
$changelog  = "Auto-deployed v$modVersion via deploy.ps1"

if (-not $ModioOAuthToken) {
    Write-Warning "[ComboMod] Skipping mod.io publish -- set ModioOAuthToken in secrets.ps1."
    Write-Warning "           Get a token at: https://mod.io/me/access  (OAuth 2 Access -> Generate Token)"
} else {
Write-Host "[ComboMod] Publishing v$modVersion to mod.io..."
try {
    Add-Type -AssemblyName System.Net.Http

    # File uploads require OAuth2 Bearer token (API keys are read-only).
    $uri    = "https://api.mod.io/v1/games/$ModioGameId/mods/$ModioModId/files"
    $client = New-Object System.Net.Http.HttpClient
    $client.DefaultRequestHeaders.Authorization =
        New-Object System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", $ModioOAuthToken)

    $multipart = New-Object System.Net.Http.MultipartFormDataContent

    # Attach zip
    $fileStream   = [System.IO.File]::OpenRead($ZipOut)
    $fileContent  = New-Object System.Net.Http.StreamContent($fileStream)
    $fileContent.Headers.ContentType =
        [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse("application/zip")
    $multipart.Add($fileContent, "filedata", [System.IO.Path]::GetFileName($ZipOut))

    # Other fields
    $multipart.Add([System.Net.Http.StringContent]::new($modVersion), "version")
    $multipart.Add([System.Net.Http.StringContent]::new($changelog),  "changelog")
    $multipart.Add([System.Net.Http.StringContent]::new("1"),          "active")

    $res     = $client.PostAsync($uri, $multipart).Result
    $body    = $res.Content.ReadAsStringAsync().Result
    $fileStream.Close()

    if ($res.IsSuccessStatusCode) {
        $json = $body | ConvertFrom-Json
        $uploadedVersion = [string]$json.version
        Write-Host "[ComboMod] mod.io upload OK - file id: $($json.id), version: $uploadedVersion"
        if ($uploadedVersion -ne $modVersion) {
            Write-Warning "[ComboMod] Requested upload version '$modVersion' but mod.io reported '$uploadedVersion'."
            Write-Warning "           This usually indicates remote version reuse/normalization."
            Write-Warning "           Build metadata is now embedded per deploy to keep artifacts unique."
        }
    } else {
        Write-Warning "[ComboMod] mod.io upload failed (HTTP $([int]$res.StatusCode)): $body"
    }
}
catch {
    Write-Warning "[ComboMod] mod.io upload FAILED: $($_.Exception.Message)"
}
} # end if $ModioOAuthToken

# ---- 5. Cleanup staged folder --------------------------------
Remove-Item "$Source\_staged" -Recurse -Force

Write-Host ""
Write-Host "Done!"
Write-Host "  Local install : $Dest"
Write-Host "  mod.io page   : https://mod.io/g/corekeeper/m/combomod"
