# 从下载的数据包还原运行环境
#
# 适用于「直接下载数据包」的方式：拿到 Again-data.7z 之后跑这个，
# 它会先校验 SHA256，再解到 bin\Release。校验不过就不会解。
#
# 用法：
#   .\tools\restore-data.ps1 -Archive "D:\Downloads\Again-data.7z"
#
# 需要 7-Zip。

param(
    [Parameter(Mandatory=$true)][string]$Archive,
    [string]$SevenZip = "",
    [string]$OutDir   = "",
    [switch]$SkipHash
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

if (-not $OutDir) { $OutDir = Join-Path $root 'bin\Release' }
if (-not (Test-Path $OutDir)) { New-Item -ItemType Directory -Force -Path $OutDir | Out-Null }
if (-not (Test-Path $Archive)) { throw "找不到数据包: $Archive" }

# 与仓库里 SHA256SUMS.md 保持一致
$expected = '2FA08AA3E02F0A701103B577160F418F80AFB9FD37BC27B141EB1DEBC89462EC'

if (-not $SkipHash) {
    Write-Host "校验 SHA256 ..."
    $actual = (Get-FileHash $Archive -Algorithm SHA256).Hash
    if ($actual -ne $expected) {
        throw "校验失败`n  期望: $expected`n  实际: $actual`n文件可能损坏或被改动，已停止。"
    }
    Write-Host "校验通过"
}

if (-not $SevenZip) {
    foreach ($p in @(
        "$env:ProgramFiles\7-Zip\7z.exe",
        "${env:ProgramFiles(x86)}\7-Zip\7z.exe",
        (Join-Path $root 'tools\7z.exe')
    )) { if (Test-Path $p) { $SevenZip = $p; break } }
}
if (-not $SevenZip -or -not (Test-Path $SevenZip)) { throw "找不到 7z.exe" }

Write-Host "解包到 $OutDir ..."
& $SevenZip x $Archive -o"$OutDir" -y | Out-Null

# 依赖 DLL 从仓库的 deps 补齐（包里不含程序文件）
Copy-Item (Join-Path $root 'deps\*.dll') $OutDir -Force

$total = (Get-ChildItem $OutDir -File | Measure-Object Length -Sum).Sum
Write-Host ("完成（{0:N1} MB）" -f ($total / 1MB))
Write-Host "现在可以运行 bin\Release\Again.exe"
