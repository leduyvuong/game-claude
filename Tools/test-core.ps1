param([string]$CompilerPath = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe')
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path -Parent $PSScriptRoot
$taskOutput = Join-Path $taskRoot '.tools\tests'
New-Item -ItemType Directory -Force -Path $taskOutput | Out-Null
if (-not (Test-Path -LiteralPath $CompilerPath)) {
    throw 'C# compiler not found. Supply -CompilerPath pointing to a compatible C# compiler.'
}
$taskExe = Join-Path $taskOutput 'FarmModelTests.exe'
$coreSources = Get-ChildItem -LiteralPath (Join-Path $taskRoot 'Assets\HarvestIsle\Scripts\Core') -Filter '*.cs' | ForEach-Object { $_.FullName }
$testSources = Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'Tests') -Filter '*.cs' | ForEach-Object { $_.FullName }
& $CompilerPath /nologo /utf8output /target:exe "/out:$taskExe" $coreSources $testSources
if ($LASTEXITCODE -ne 0) { throw 'Core compilation failed.' }
& $taskExe
if ($LASTEXITCODE -ne 0) { throw 'Core tests failed.' }
