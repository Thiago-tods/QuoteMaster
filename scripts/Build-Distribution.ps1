$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$sdkExe = (Get-Command dotnet.exe -ErrorAction SilentlyContinue).Source
if (-not $sdkExe) {
    $localSdk = Join-Path $repoRoot '..\..\work\dotnet-sdk\dotnet.exe'
    if (Test-Path $localSdk) { $sdkExe = (Resolve-Path $localSdk).Path }
}
if (-not $sdkExe) { throw 'Instale o .NET 10 SDK ou ajuste o caminho local do SDK antes de publicar.' }

$distRoot = Join-Path $repoRoot 'dist'
$publishParent = Join-Path $distRoot 'publish'
$publishDir = Join-Path $publishParent 'win-x64'
$installerDir = Join-Path $distRoot 'installer'
New-Item -ItemType Directory -Force -Path $publishParent, $installerDir | Out-Null
$resolvedParent = [IO.Path]::GetFullPath($publishParent) + [IO.Path]::DirectorySeparatorChar
$resolvedTarget = [IO.Path]::GetFullPath($publishDir)
if (-not $resolvedTarget.StartsWith($resolvedParent, [StringComparison]::OrdinalIgnoreCase)) { throw "Destino de publicação fora do diretório esperado: $resolvedTarget" }
if (Test-Path $publishDir) { Remove-Item -LiteralPath $publishDir -Recurse -Force }
New-Item -ItemType Directory -Force -Path $publishDir | Out-Null

Push-Location $repoRoot
try {
    & $sdkExe publish src/QuoteMaster.Web/QuoteMaster.Web.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -p:PublishTrimmed=false -o $publishDir
    if ($LASTEXITCODE -ne 0) { throw "Publicação do servidor falhou (código $LASTEXITCODE)." }
    & $sdkExe publish src/QuoteMaster.Launcher/QuoteMaster.Launcher.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -p:PublishTrimmed=false -o $publishDir
    if ($LASTEXITCODE -ne 0) { throw "Publicação do launcher falhou (código $LASTEXITCODE)." }

    $compiler = 'C:\Program Files\Inno Setup 7\ISCC.exe'
    if (-not (Test-Path $compiler)) { $compiler = (Get-Command ISCC.exe -ErrorAction SilentlyContinue).Source }
    if (-not $compiler) { throw 'Inno Setup 7 não encontrado. Instale-o para gerar o instalador.' }
    & $compiler /Qp (Join-Path $repoRoot 'installer\quotemaster.iss')
    if ($LASTEXITCODE -ne 0) { throw "Compilação do instalador falhou (código $LASTEXITCODE)." }

    $archive = Join-Path $distRoot 'QuoteMaster-1.0.0-win-x64-portable.zip'
    if (Test-Path $archive) { Remove-Item -LiteralPath $archive -Force }
    Compress-Archive -Path (Join-Path $publishDir '*') -DestinationPath $archive -CompressionLevel Optimal
    $installer = Join-Path $installerDir 'QuoteMaster-Setup-1.0.0-win-x64.exe'
    $hashLines = @(
        "$( (Get-FileHash -Algorithm SHA256 $installer).Hash.ToLowerInvariant() )  $(Split-Path $installer -Leaf)",
        "$( (Get-FileHash -Algorithm SHA256 $archive).Hash.ToLowerInvariant() )  $(Split-Path $archive -Leaf)"
    )
    [IO.File]::WriteAllLines((Join-Path $distRoot 'SHA256SUMS.txt'), $hashLines)
    Write-Host "Instalador: $installer"
    Write-Host "Pacote portátil: $archive"
} finally { Pop-Location }
