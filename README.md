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

### 2026-01-06 (v1.1.0)
- **新增预览器**:
  - **十六进制 (Hex) 预览**: 支持查看二进制文件 (`.dll`, `.exe`, `.bin`, `.dat` 等) 的十六进制内容。
- **扩展格式支持**:
  - **代码与配置**: 新增 Go, Rust, Ruby, PHP, Vue, Lua, Swift, Dart, R, Perl, VB.NET, F#, Assembly 及 CMake, Docker, EditorConfig 等配置文件的语法高亮。
  - **压缩包**: 扩展支持查看基于 Zip 的格式结构，包括 `.apk`, `.epub`, `.jar`, `.nupkg`, `.vsix` 等。
  - **图像**: 新增 TIFF (`.tiff`, `.tif`) 支持。
  - **多媒体**: 新增 WebM 视频及 AAC 音频支持。
- **界面更新**: 更新了“关于”对话框的信息。

### 2026-01-02
- **扩展格式支持**:
  - 新增 **CSV** 预览，支持表格化数据展示。
  - 增强 **代码预览**，新增对 TS, TSX, JSX, Java, Kotlin, SQL, Shell, Batch, PowerShell, TOML, YAML 等数十种开发格式的支持。
  - 将 PDF 预览升级为通用 **Web 预览**（基于 WebView2），新增对 SVG 矢量图及 HTML 网页的原生支持。
- **架构优化**:
  - 修复了在混合 WPF/WinForms 环境下的类型歧义编译问题。
  - 重构了预览器工厂逻辑，提升了渲染引擎的匹配精度。

## 支持预览的格式

FilePreview 采用插件化架构，目前已支持以下格式：

- **图像**: JPG, PNG, BMP, GIF, WEBP, ICO, TIFF。
- **文本与代码**: 几乎所有主流编程语言 (C#, JS, Py, Go, Rust, Java, PHP 等) 及各类配置文件（支持语法高亮）。
- **二进制文件**: DLL, EXE, BIN, DAT 等（十六进制视图）。
- **专业文档**:
  - **Markdown**: 渲染为格式化的 HTML 页面。
  - **PDF**: 高性能内置渲染。
  - **CSV**: 表格化预览。
- **矢量图形**: SVG 原生渲染。
- **压缩包**: ZIP, EPUB, APK, JAR, NUPKG 等（查看内部结构）。
- **媒体文件**: MP4, MKV, AVI, WEBM (视频) 及 MP3, WAV, FLAC, AAC (音频)。
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
