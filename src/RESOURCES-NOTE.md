# 嵌入资源

程序集里嵌了两个资源：

| 名称 | 大小 | 内容 |
| --- | --- | --- |
| `KidFansChannel.Again.Resource.resources` | 1,824,535 字节 | 游戏内资源（图片、字符串等） |
| `KidFansChannel.GameCore.Window.resources` | 180 字节 | 窗体布局 |

两者都在 `resources/` 下，由 `Again.csproj` 用 `/resource:` 参数打进产物，
与原程序集一致。
