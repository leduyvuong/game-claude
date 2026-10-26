param([int]$Port=8787,[switch]$BuildOnly)
$ErrorActionPreference='Stop'
$taskRoot=Split-Path -Parent $PSScriptRoot
$taskOutput=Join-Path $taskRoot '.tools\server'
New-Item -ItemType Directory -Force -Path $taskOutput | Out-Null
$compiler=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$coreSources=Get-ChildItem -LiteralPath (Join-Path $taskRoot 'Assets\HarvestIsle\Scripts\Core') -Filter '*.cs' | ForEach-Object { $_.FullName }
& $compiler /nologo /utf8output /target:exe /r:System.Web.Extensions.dll "/out:$taskOutput\CoreWorker.exe" $coreSources (Join-Path $PSScriptRoot 'CoreWorker.cs')
if ($LASTEXITCODE -ne 0) { throw 'Server Core compilation failed.' }
if ($BuildOnly) { exit 0 }
$taskPortCheck=Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue
if ($taskPortCheck) { throw "Port $Port is already in use. Existing server was left untouched." }
$taskProcess=Start-Process -FilePath (Get-Command node).Source -ArgumentList @('Server/server.mjs',"--port=$Port") -WorkingDirectory $taskRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput "$taskOutput\server.log" -RedirectStandardError "$taskOutput\server-error.log"
Write-Output "Local server requested: http://127.0.0.1:$Port (PID $($taskProcess.Id))"
