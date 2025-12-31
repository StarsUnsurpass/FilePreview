# FilePreview

**Project URL**: [https://github.com/StarsUnsurpass/FilePreview](https://github.com/StarsUnsurpass/FilePreview)  
**Author**: [StarsUnsurpass](https://github.com/StarsUnsurpass)
**Author Homepage**: [https://github.com/StarsUnsurpass](https://github.com/StarsUnsurpass)

FilePreview 是一款旨在 Windows 平台上复刻并超越 macOS "Quick Look" 体验的轻量级预览工具。
它运行在系统托盘区，允许用户通过简单的快捷键立即预览文件内容，而无需打开关联的重型应用程序。

## 核心特性

- **极速响应**: 针对 Windows 系统底层优化，预览窗口秒开。
- **现代 UI**: 采用 Windows 11 Fluent Design 设计语言，支持 Mica (云母) 材质及圆角阴影。
- **智能交互**:
  - 选中文件按下 `Space` (空格) 立即弹出预览。
  - 再次按下 `Space` 或 `Esc` 快速关闭。
  - 点击窗口外区域自动隐藏（失焦关闭）。
  - 支持窗口自由拖拽、最小化到任务栏。
- **多功能标题栏**: 集成文件管理、视图切换及工具菜单。
- **完善的日志系统**: 基于 Serilog 的日志记录，便于追踪错误和分析运行状态。

## 更新日志

### 2025-12-31
- **新增日志记录功能**:
  - 集成了 [Serilog](https://serilog.net/) 框架。
  - 支持每日滚动的日志文件存储（位于 `logs/` 目录）。
  - 实现了对应用生命周期（启动、退出）、键盘钩子事件、文件路径获取过程及预览器加载过程的全面监控。
  - 增加了全局未处理异常捕获，提升了系统的可维护性与稳定性。

## 支持预览的格式

FilePreview 采用插件化架构，目前已支持以下格式：

- **图像**: JPG, PNG, BMP, GIF, WEBP, ICO 等。
- **文本与代码**: TXT, JSON, XML, CS, PY, CPP, HTML, CSS 等（支持语法高亮）。
- **专业文档**:
  - **Markdown**: 渲染为格式化的 HTML 页面。
  - **PDF**: 高性能内置渲染。
- **压缩包**: 支持查看 ZIP 内部文件结构。
- **媒体文件**: MP4, MKV, AVI (视频) 及 MP3, WAV (音频) 的播放。
- **文件夹**: 显示文件夹属性、包含项及最后修改时间。
- **万能预览**: 通过 Shell 扩展支持 Word, Excel, PPT 等已安装软件的预览句柄。

## 技术栈

- **语言**: C# (.NET 10)
- **UI 框架**: WPF + [Wpf.Ui](https://github.com/lepoco/wpfui) (Fluent Design)
- **底层交互**: Win32 API (键盘钩子) + COM Interop (资源管理器集成)
- **核心组件**:
  - `AvalonEdit`: 代码语法高亮。
  - `WebView2`: PDF 及 Markdown 渲染。
  - `Markdig`: Markdown 解析。
  - `Microsoft.Windows.CsWin32`: 现代化的 P/Invoke 生成。
  - `Serilog`: 结构化日志记录。

## 使用说明

1. 启动 `FilePreview.exe`。
2. 程序将运行在系统托盘区。
3. 在“资源管理器”或“桌面”选中任意文件。
4. 按下 **空格键** 弹出预览。
5. 通过顶部的 `File` 菜单可以复制路径或使用默认应用打开文件。

## 开发路线图

- [x] 全局键盘钩子 (Space 监听)
- [x] Windows 11 风格预览窗体
- [x] 智能窗口尺寸自适应
- [x] 多级功能菜单系统
- [x] 单实例运行检测
- [x] 完善的日志记录与异常处理
- [ ] 插件系统扩展接口
- [ ] 预览高清图片时的性能预加载
- [ ] 多显示器自适应定位

---

*FilePreview - 让 Windows 也能拥有优雅的预览体验。*
