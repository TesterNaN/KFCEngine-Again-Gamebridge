# 一键准备 AGAIN 运行环境
#
# 从原版三张 CD 里取出游戏数据，放到 bin\Release 旁边，这样编译出来的
# Again.exe 就能直接跑。
#
# 数据不进仓库（1.9 GB，GitHub 放不下），所以换机器要重跑一遍。
#
# 用法：
#   .\tools\prepare-data.ps1 -Source "D:\...\秋之回忆 从今以后"
#
# -Source 指向解压后包含 CD1 / CD2 / CD3 的那个目录。
#
# 需要 7-Zip 和 innoextract：
#   7-Zip        https://www.7-zip.org/
#   innoextract  https://constexpr.org/innoextract/
#                （本脚本会尝试自动下载到 tools\inno）

param(
    [Parameter(Mandatory=$true)][string]$Source,
    [string]$SevenZip    = "",
    [string]$InnoExtract = "",
    [string]$OutDir      = ""
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

if (-not $OutDir) { $OutDir = Join-Path $root 'bin\Release' }
if (-not (Test-Path $OutDir)) { New-Item -ItemType Directory -Force -Path $OutDir | Out-Null }

# --- 7z ---
if (-not $SevenZip) {
    foreach ($p in @(
        "$env:ProgramFiles\7-Zip\7z.exe",
        "${env:ProgramFiles(x86)}\7-Zip\7z.exe",
        (Join-Path $root 'tools\7z.exe')
    )) { if (Test-Path $p) { $SevenZip = $p; break } }
}
if (-not $SevenZip -or -not (Test-Path $SevenZip)) {
    throw "找不到 7z.exe，请用 -SevenZip 指定"
}

# --- innoextract ---
if (-not $InnoExtract) {
    $cand = Join-Path $root 'tools\inno\innoextract.exe'
    if (Test-Path $cand) {
        $InnoExtract = $cand
    } else {
        Write-Host "下载 innoextract ..."
        $dir = Join-Path $root 'tools\inno'
        New-Item -ItemType Directory -Force -Path $dir | Out-Null
        $zip = Join-Path $dir 'innoextract.zip'
        Invoke-WebRequest 'https://github.com/dscharrer/innoextract/releases/download/1.9/innoextract-1.9-windows.zip' -OutFile $zip -UseBasicParsing
        Expand-Archive $zip -DestinationPath $dir -Force
        $InnoExtract = Join-Path $dir 'innoextract.exe'
    }
}
if (-not (Test-Path $InnoExtract)) { throw "找不到 innoextract.exe" }

# --- 定位 CD1 / CD2 / CD3 ---
$cd1 = Join-Path $Source 'CD1'
$cd2 = Join-Path $Source 'CD2'
$cd3 = Join-Path $Source 'CD3'
foreach ($d in @($cd1, $cd2, $cd3)) {
    if (-not (Test-Path $d)) { throw "缺少目录: $d" }
}
$setup = Join-Path $cd1 'setup.exe'
if (-not (Test-Path $setup)) { throw "找不到 $setup" }

# Inno Setup 的数据分卷散在三张盘上，必须先把三卷并到 setup.exe 同级目录，
# 否则 innoextract 只能解出第一卷的内容（chara.dat 会少一半）。
Write-Host "合并 split volumes ..."
foreach ($bin in @('setup-1.bin','setup-2.bin','setup-3.bin')) {
    $target = Join-Path $cd1 $bin
    if (Test-Path $target) { continue }
    foreach ($cd in @($cd2, $cd3)) {
        $src = Join-Path $cd $bin
        if (Test-Path $src) {
            Copy-Item $src $target -Force
            Write-Host ("  {0} <- {1}" -f $bin, (Split-Path $cd -Leaf))
            break
        }
    }
}

# --- 解包 ---
Write-Host "解包（约 1.9 GB）..."
$tmp = Join-Path $env:TEMP ("again_extract_" + [guid]::NewGuid().ToString('N'))
& $InnoExtract -e -d $tmp $setup 2>&1 | Select-Object -Last 3

$app = Join-Path $tmp 'app'
if (-not (Test-Path $app)) { throw "解包失败，没有找到 app 目录" }

# --- 只要资源，不要程序 ---
# exe / dll 由本仓库编译产出，不从光盘复制
$skip = @('Again.exe','AgainSetting.exe','AgainSetup.exe','AgainGDF.dll',
          'DirectShowLib-2005.dll','PlayOgg.dll','Invoke.dll',
          'GameuxInstallHelper.dll','Invoke.reg')

Write-Host "复制资源到 $OutDir ..."
Get-ChildItem $app -File | Where-Object { $_.Name -notin $skip } | ForEach-Object {
    Copy-Item $_.FullName $OutDir -Force
}
foreach ($d in 'bgm','movie','license','zh-CHS') {
    $src = Join-Path $app $d
    if (Test-Path $src) { Copy-Item $src $OutDir -Recurse -Force }
}

# 依赖 DLL 从仓库的 deps 补齐
Copy-Item (Join-Path $root 'deps\*.dll') $OutDir -Force

Remove-Item $tmp -Recurse -Force -ErrorAction SilentlyContinue

$total = (Get-ChildItem $OutDir -File | Measure-Object Length -Sum).Sum
Write-Host ("完成，{0:N1} MB" -f ($total / 1MB))
Write-Host "现在可以运行 bin\Release\Again.exe"
