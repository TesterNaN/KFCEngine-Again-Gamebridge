# 嵌入资源说明

原程序集里嵌了两个资源：

| 名称 | 大小 | 内容 |
| --- | --- | --- |
| `KidFansChannel.Again.Resource.resources` | 1,824,535 字节 | 游戏内资源（图片、字符串等）—— **版权内容，未包含在本仓库** |
| `KidFansChannel.GameCore.Window.resources` | 180 字节 | 窗体布局 |

`Resource.resx` 在反编译时会被还原出来（约 2.38 MB），但它属于 KID / KIDS Fans Channel
的美术与文本资产，因此**没有提交到本仓库**。

不包含它只会让编译产物少 2.4 MB，代码本身照样能编译通过。

如果你手上有合法的游戏副本，可以从原程序集里自行提取：

```powershell
# 从 Again.Managed.exe 导出资源（需要 ILSpy / dnSpy / ResourceHacker）
ilspycmd -o dump Again.Managed.exe
```
