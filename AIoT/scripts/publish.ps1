param(
	[string]$Configuration = "Release",
	[string[]]$Rids = @("win-x64", "linux-x64", "osx-x64"),
	[switch]$SelfContained = $true,
	[switch]$SingleFile = $true,
	[switch]$Trimmed = $false,
	[switch]$NoRestore
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectPath = Join-Path $scriptRoot "..\AIoT.csproj"
$distRoot = Join-Path $scriptRoot "..\dist"

if (-not (Test-Path $projectPath)) {
	throw "Project not found: $projectPath"
}

New-Item -ItemType Directory -Force -Path $distRoot | Out-Null

Write-Host "== AIoT publish ==" -ForegroundColor Cyan
Write-Host "Project: $projectPath"
Write-Host "Configuration: $Configuration"
Write-Host "RIDs: $($Rids -join ', ')"

foreach ($rid in $Rids) {
	$outDir = Join-Path $distRoot $rid
	New-Item -ItemType Directory -Force -Path $outDir | Out-Null

	$args = @(
		"publish",
		$projectPath,
		"-c", $Configuration,
		"-r", $rid,
		"-o", $outDir,
		"-p:PublishSingleFile=$($SingleFile.IsPresent.ToString().ToLower())",
		"-p:SelfContained=$($SelfContained.IsPresent.ToString().ToLower())",
		"-p:PublishTrimmed=$($Trimmed.IsPresent.ToString().ToLower())",
		"-p:AutoBuildProtocolPlugins=true"
	)

	if ($NoRestore) {
		$args += "--no-restore"
	}

	Write-Host "`nPublishing $rid -> $outDir" -ForegroundColor Yellow
	& dotnet @args

	if ($LASTEXITCODE -ne 0) {
		throw "dotnet publish failed for RID: $rid"
	}
}

Write-Host "`nPublish completed. Outputs in: $distRoot" -ForegroundColor Green
Write-Host "Windows executable example: $distRoot\win-x64\AIoT.exe" -ForegroundColor Green
