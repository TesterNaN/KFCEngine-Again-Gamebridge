# KFCEngine v2 · 娱乐通版 —《秋之回忆：从今以后 AGAIN》

**218 个源文件 / 43,000 行 C# 2.0 代码，编译零错误。**

这是 KFCEngine 第二代的**娱乐通（GameBridge）零售版**还原。同一个游戏由两家分别发行，
两套程序并不相同：

| | 仓库 | 发行 | 保护 | 压盘 / 补丁 |
|---|---|---|---|---|
| 光谱 | [KFCEngine-Again-TTIME](https://github.com/TesterNaN/KFCEngine-Again-TTIME) | T-Time | 加壳 | Patch01 2007-10-09，Patch02 2008-01-31 |
| **娱乐通** | **本仓库** | GameBridge | 加壳 | 零售盘 2007-10-22 |

## 两边的差异

**程序**：娱乐通这版更完整

- 滚动条可以**拖动**（光谱版只能靠外部改 `Start`）
- **片尾职员表**（`StaffScreen`，播 `resource.dat` 里那段 ASF 影片）
- 消息记录可以**拖动定位**（`MoveTo` 二分查找）
- `wait voice="true"` 会**等语音播完**再继续
- 窗口/全屏切换后会**重排对话框**
- 影片文件名在**装载阶段**就解析好，不在执行命令时才查

**剧本**：光谱那边更新

| | `system.dat` | 日期 |
|---|---|---|
| 光谱 Patch01 | 11,378,080 字节 | 2007-10-01 |
| 娱乐通零售 | 10,972,704 字节 | 2007-10-22 |

光谱的剧本数据大 405 KB。所以这不是一条线上的新旧关系，而是两家各自维护：
娱乐通的程序是较新的构建，光谱的剧本有后来补的内容。

两边引擎同源，外围组件（`Invoke.dll`、`PlayOgg.dll`、`AgainSetting.exe`）逐字节相同。

> 第一代的完整还原 → **[KFCEngine-Yukihota](https://github.com/TesterNaN/KFCEngine-Yukihota)**
> 第三代 → **[KFCEngine-CloseTo](https://github.com/TesterNaN/KFCEngine-CloseTo)**

---

## 游戏数据

本仓库只有**引擎本体**，数据要自己准备（约 1.9 GB，GitHub 放不下）。

《AGAIN》PC 中文版的数据放在一种叫 **`KFCD`** 的容器里：

```
chara.dat      848 MB   立绘
bg.dat         303 MB   背景
voice.dat      158 MB   语音
resource.dat    22 MB
se.dat          16 MB   音效
bg.zh-CHS.dat   15 MB
system.dat      10 MB   剧本与系统
font.zh-CHS.dat 3.9 MB  中文字体
system.zh-CHS.dat 1.1 MB
script.dat / bgm.dat / movie.dat / font.dat / bgm.zh-CHS.dat
bgm\            72 首 ogg
movie\          5 个视频
zh-CHS\         Again.resources.dll + AgainSetting.resources.dll
```

两种方式，任选其一。

### 方式一：下载游戏本体，从三张 CD 里取数据

游戏本体是这个压缩包（2.0 GB）：

| 来源 | 文件 | SHA256 |
|---|---|---|
| 零售版光盘镜像 | `秋之回忆 从今以后.rar` | `F544FBE8…F38D0CD1D91C` |

解压后是这样：

```
秋之回忆 从今以后\
├─ CD1\  setup.exe + setup-1.bin (667 MB)
├─ CD2\  setup-2.bin (668 MB)
└─ CD3\  setup-3.bin (548 MB) + Again_Data\ + directX\
```

拿 CD1 那个目录去跑：

```
.\tools\prepare-data.ps1 -Source "D:\...\秋之回忆 从今以后"
```

脚本会自动把 CD2、CD3 的分卷复制到 CD1（Inno Setup 的三卷必须同目录，否则只能解出第一卷，`chara.dat` 会少一半），然后调 `innoextract` 解包，挑出资源，跳过程序文件。

> 不需要运行安装程序。光盘目录直接当数据容器解包即可。

### 方式二：直接下载数据包

数据包已经打好了（1.92 GB，**只含资源**，不含任何 exe / dll）：

| 网盘 | 链接 | 提取码 |
|---|---|---|
| OneDrive | [Again-data.7z](https://1drv.ms/u/c/d48d7cc20d2177d6/IQCs85Yt4fnjTr5rdJemX_-oAUJaIhn8A1vKj2YYgVViIQo?e=3nugbP) | — |
| 百度网盘 | [Again-data.7z](https://pan.baidu.com/s/15oC-M8o1r0BOOBP4x7dmAQ) | `h48z` |

下载 `Again-data.7z` 后：

```
.\tools\restore-data.ps1 -Archive "D:\Downloads\Again-data.7z"
```

脚本会先核对 SHA256 再解包，文件对不上就停下：

```
SHA256  2FA08AA3E02F0A701103B577160F418F80AFB9FD37BC27B141EB1DEBC89462EC
大小    2,014,944,933 字节
```

### 跑起来

数据就位后直接启动 `bin\Release\Again.exe`。

首次运行会先弹设置窗口（`AgainSetting.exe`），选好设备/分辨率之后进标题画面。
想用窗口模式就在设置里把「全屏幕」勾掉——窗口固定 800×600（原版 `WindowSize`
是 readonly 常量，画面缩放和坐标映射都依赖它）。

**注意别拿错版本**：网上流传的《AGAIN》PC 中文版绝大多数是澄空学园的 **ADAM 版**
（C++ 引擎、目录里有 `adamsetting.ini` / `fmod.dll` / `msvcr71.dll`），跟这套完全无关。
本仓库对应的是 KIDS Fans Channel 用 .NET 做的那一版，目录特征是 `Again.exe` + `KFCD` 开头的数据文件。

---

## 编译

需要三样：

1. **.NET Framework 2.0** —— 不是 3.5，不是 4.0。原程序集就是 2.0 的，`csc.exe` 在 `C:\Windows\Microsoft.NET\Framework\v2.0.50727\`
2. **MSBuild 4.0** —— 用来驱动构建
3. **DirectX for Managed Code** —— 可选，`deps\` 里已经带了

```bat
msbuild Again.csproj /t:Rebuild
```

产物：`bin\Release\Again.exe`（2.4 MB）。

`deps\` 里的依赖：

| 文件 | 版本 | 作用 |
|---|---|---|
| `Microsoft.DirectX.dll` | 1.0.2902 | Managed DirectX 基础 |
| `Microsoft.DirectX.Direct3D.dll` | 1.0.2902 | 3D 设备 |
| `Microsoft.DirectX.Direct3DX.dll` | **1.0.2911** | 与原始程序集引用的版本一致 |
| `Microsoft.DirectX.DirectInput.dll` | 1.0.2902 | 输入 |
| `Microsoft.DirectX.DirectSound.dll` | 1.0.2902 | 音频 |
| `DirectShowLib-2005.dll` | 2.0.0.0 | 视频播放 |

这些是 .NET 2.0 时代的 Managed DirectX，原本装在
`C:\Windows\Microsoft.NET\DirectX for Managed Code\` 下。

**`Direct3DX` 必须用 1.0.2911**：v1《雪萤》用的是 1.0.2906，两者不通用——
V2 原程序集的引用清单里写的就是 `1.0.2911.0`，装错版本运行时会报 `FileLoadException`。

---

## 代码结构

```
src/
├─ KidFansChannel.Again/          游戏层（Commands / Effects / Screens）
├─ KidFansChannel.GalCore/        视觉小说核心，最大的模块（Commands / Defines /
│                                 ExtBases / Records / Screens）
├─ KidFansChannel.GalExt/         扩展（DisplayStyles / Effects / TransformStyles）
├─ KidFansChannel.GameCore/       主循环、设备管理
├─ KidFansChannel.Game2DCore/     2D 图形（图片、字体、精灵）
├─ KidFansChannel.DataCore/       归档读取（KFCD 容器的解析就在这里）
├─ KidFansChannel.SoundCore/      音频（Ogg Vorbis + DirectSound）
├─ KidFansChannel.VideoCore/      视频（DirectShow）
├─ KidFansChannel.InputCore/      键鼠手柄
deps/                             Managed DirectX + DirectShowLib
Invoke/                           Invoke.dll 的反编译源码（36 行，跳板）
tools/                            数据准备脚本
```

模块依赖大致是：

```
Again ──► GalCore ──► GameCore ──► Game2DCore ──► Direct3D
             │            ├────► SoundCore ─────► DirectSound
             │            ├────► VideoCore ─────► DirectShow
             │            ├────► InputCore ─────► DirectInput
             └──────────► DataCore（KFCD 归档）
```

---

## 两代对比

| | v1《雪萤》(2006-03) | v2《AGAIN》(2007 秋) |
|---|---|---|
| 文件 / 行数 | 32 / 10,983 | **218 / 43,069** |
| 命名空间 | 一个 `Ӓ`，所有类挤在一起 | **8 个分层模块** |
| 运行时 | .NET 1.1 | **.NET 2.0** |
| 图形 | Managed DirectX 9（Direct3DX 1.0.2906） | Direct3DX **1.0.2911** |
| 视频 | winmm MCI（`mciSendString`） | **DirectShow** |
| 声音 | MCI / DirectSound 混用 | 独立 SoundCore |
| 可运行 | ✓ 到正片 | **✓ 到序幕** |

### 两个发行商，两套保护

《AGAIN》PC 中文版由光谱（T-Time）和娱乐通（GameBridge）分别发行过，两边用的是
**同一条 KFC 引擎**，反盗版方案不同：

| | 形态 | 保护 | exe |
|---|---|---|---|
| 光谱（T-Time） | Patch01 增量补丁 | StarForce FrontLine 5 | 3,147,048 字节 |
| 娱乐通（GameBridge） | 零售版三张 CD | SecuROM | 7,561,216 字节 |

两边 exe 的版本资源一字不差，`Invoke.dll`、`PlayOgg.dll`、`AgainSetting.exe`
三个组件字节级相同——所以内层是同一份 `Again.Managed.exe`，只是外壳不同。

本仓库的代码取自**娱乐通零售版**，光谱补丁只带增量，不含完整资源。

---

## 程序集怎么取出来的

`Again.exe` 是原生启动器套 SecuROM。.NET 程序集作为 PE 资源配置整块嵌入，加密存放：

| 资源类型 | ID | 大小 | 内容 |
|---|---|---|---|
| `IDR_ASSEMBLY` | 110 | 2,696,712 字节 | 加密的 `Again.Managed.exe` |
| `IDR_ASSEMBLY` | 111 | 16,384 字节 | `Invoke.dll`，明文 |

文件先要取对：它不在安装包里，存放在零售盘第三张的 `Again_Data\Again.exe`。
安装包只装数据。

资源表是明文，取资源不经过 SecuROM 的解密逻辑，所以不需要处理壳的授权检查。

### 解密参数

`Invoke.dll` 与光谱版字节级相同（SHA256 `937B0ADA…`），参数也一样：

```csharp
string text = "KID Fans Channel";     // 3DES 密钥，补零到 24 字节
byte[] iv = pwd 的前 8 字节;           // pwd = "Again"
TripleDESCryptoServiceProvider.CreateDecryptor(key, iv);
```

### 步骤

1. 取零售盘第三张的 `Again_Data\Again.exe`
2. 解析 PE 资源表，定位 `IDR_ASSEMBLY` id=110
3. 取出该资源，位于文件偏移 452,704，长度 2,696,712 字节
4. 3DES-CBC 解密：key 取 `"KID Fans Channel"` 补零到 24 字节，
   IV 取 `"Again"` 补零到 8 字节
5. 明文长度是密文长度减 8
6. 结果以 `MZ` 开头，即 `Again.Managed.exe`

---

## 版权

- 《Memories Off ～それから～ Again》原作 © KID Corp.
- 中文 PC 版移植与本地化 © KIDS Fans Channel（2007）
- .NET 运行时与 Managed DirectX © Microsoft

本仓库为静态存档与可读性还原。代码版权归原作者，如权利人认为不妥，请通过 Issue 联系。

---

## 相关

- [KFCEngine-Yukihota](https://github.com/TesterNaN/KFCEngine-Yukihota) — 第一代《雪萤》，完整可运行的还原
- [KFCEngine-CloseTo](https://github.com/TesterNaN/KFCEngine-CloseTo) — 第三代《思念的碎片》，同样可运行；它的变量命名大量沿用了这里的成果
- [NOTICE.md](NOTICE.md) — 版权细节
