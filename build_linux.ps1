#!/usr/bin/env pwsh

$Output = Join-Path $PSScriptRoot Build

dotnet publish src/TSMapEditor/TSMapEditor.csproj --configuration=LinuxRelease --runtime linux-x64 --output=$Output --self-contained false