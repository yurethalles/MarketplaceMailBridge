param(
    [string]$ValheimPath = 'C:\Program Files (x86)\Steam\steamapps\common\Valheim',
    [string]$ProfilePath = 'C:\Users\Usuario\AppData\Roaming\com.kesomannen.gale\valheim\profiles\Glitnir Northbound - Player'
)

$ErrorActionPreference = 'Stop'
$out = Join-Path $PSScriptRoot 'dist'
New-Item -ItemType Directory -Force -Path $out | Out-Null

$references = @(
    (Join-Path $ProfilePath 'BepInEx\core\BepInEx.dll'),
    (Join-Path $ProfilePath 'BepInEx\plugins\KG-Marketplace_And_Server_NPCs_Revamped\kg.Marketplace.dll'),
    (Join-Path $ValheimPath 'valheim_Data\Managed\assembly_valheim.dll'),
    (Join-Path $ValheimPath 'valheim_Data\Managed\UnityEngine.dll'),
    (Join-Path $ValheimPath 'valheim_Data\Managed\UnityEngine.CoreModule.dll'),
    (Join-Path $ValheimPath 'valheim_Data\Managed\netstandard.dll')
)

foreach ($reference in $references) {
    if (-not (Test-Path -LiteralPath $reference)) { throw "Dependência não encontrada: $reference" }
}

$csc = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $csc)) { throw 'Compilador .NET Framework não encontrado.' }

$arguments = @(
    '/target:library',
    '/optimize+',
    ('/out:{0}' -f (Join-Path $out 'MarketplaceMailBridge.dll'))
)
$arguments += $references | ForEach-Object { '/reference:{0}' -f $_ }
$arguments += @(
    (Join-Path $PSScriptRoot 'src\MarketplaceMailBridge.cs'),
    (Join-Path $PSScriptRoot 'src\AssemblyInfo.cs')
)

& $csc @arguments
if ($LASTEXITCODE -ne 0) { throw "Compilação falhou com código $LASTEXITCODE." }

Write-Host "Criado: $(Join-Path $out 'MarketplaceMailBridge.dll')"
