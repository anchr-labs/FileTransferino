# FileTransferino Solution

A .NET 10 solution for a File Transfer application with clean architecture.

## Project Structure

### FileTransferino.App (Avalonia UI Application)
- **Type**: Desktop Application (Avalonia UI)
- **Target Framework**: .NET 10
- **Purpose**: User interface for the file transfer application
- **Dependencies**: References all other projects

### FileTransferino.Core (Class Library)
- **Type**: Class Library
- **Target Framework**: .NET 10
- **Purpose**: Core business logic and domain models
- **Dependencies**: None

### FileTransferino.Data (Class Library)
- **Type**: Class Library
- **Target Framework**: .NET 10
- **Purpose**: Data access layer and repository patterns
- **Dependencies**: None

### FileTransferino.Security (Class Library)
- **Type**: Class Library
- **Target Framework**: .NET 10
- **Purpose**: Security, authentication, and authorization logic
- **Dependencies**: None

### FileTransferino.Infrastructure (Class Library)
- **Type**: Class Library
- **Target Framework**: .NET 10
- **Purpose**: External services, logging, and infrastructure concerns
- **Dependencies**: None

## Dependency Graph

```
FileTransferino.App
    ├── FileTransferino.Core
    ├── FileTransferino.Data ──→ FileTransferino.Infrastructure ──→ FileTransferino.Core
    ├── FileTransferino.Security
    └── FileTransferino.Infrastructure ──→ FileTransferino.Core
```

All class libraries follow clean architecture with no circular dependencies.

## Application Data

On startup, the app creates the following structure in the user's application data directory:

```
%APPDATA%\FileTransferino\          (Windows)
~/.config/FileTransferino/          (Linux/macOS)
    ├── data/
    │   └── FileTransferino.db     (SQLite database)
    ├── logs/
    ├── themes/
    └── settings.json         (Application settings)
```

## Building the Solution

```powershell
dotnet build FileTransferino.sln
```

## Running the Application

```powershell
dotnet run --project FileTransferino.App\FileTransferino.App.csproj
```

## Features

### Theming System (Slice 1)
- **5 Built-in Themes**: Light, Dark, Ocean, Nord, Monokai
- **Semantic Tokens**: Centralized color tokens (Background, Surface, TextPrimary, etc.)
- **Runtime Theme Switching**: Instant UI updates without restart
- **Persistent Theme Settings**: Theme preference saved to settings.json

### Command Palette (Slice 1)
- **Keyboard Shortcut**: Press `Ctrl+K` to open
- **Quick Theme Switching**: Search and apply themes instantly
- **Fuzzy Search**: Filter commands by name or category
- **Keyboard Navigation**: Use arrow keys and Enter to execute

## Notes

- All projects target .NET 10
- Nullable reference types are enabled across all projects
- Implicit usings are enabled in all projects
- The App project uses compiled bindings by default for Avalonia
- **Slice 0.2 Foundation implemented:**
  - Cross-platform `AppPaths` for application directories
  - JSON-based settings persistence via `SettingsStore`
  - SQLite database with DbUp migrations
  - Async initialization that doesn't block UI thread
- **Slice 1 Theming & Command Palette implemented:**
  - Semantic theme token system with 5 built-in themes
  - IThemeService for runtime theme management
  - Command palette overlay with Ctrl+K shortcut
  - Theme persistence via existing SettingsStore

## Documentation

- **[Development Instructions](.github/INSTRUCTIONS.md)** - Essential commands and workflow guidelines
- **[Solution Summary](solution-summary.md)** - Detailed build status and project information

> 📝 Remember to keep all documentation files updated when making structural changes!
