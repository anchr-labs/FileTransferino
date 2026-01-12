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

### FileTransferino.UI (Class Library)
- **Type**: Class Library
- **Target Framework**: .NET 10
- **Purpose**: Shared UI components and controls
- **Dependencies**: None

### FileTransferino.Mobile (Mobile Projects)
- **Type**: Mobile Application Suite (Uno Platform)
- **Target Frameworks**: Multiple (Android, iOS, Browser, Desktop)
- **Purpose**: Cross-platform mobile application
- **Projects**: 
  - FileTransferino.Mobile (shared code)
  - FileTransferino.Mobile.Android
  - FileTransferino.Mobile.iOS
  - FileTransferino.Mobile.Browser
  - FileTransferino.Mobile.Desktop
- **Status**: ⚠️ In development (Android requires JDK 21)

## Dependency Graph

```
FileTransferino.App
    ├── FileTransferino.Core
    ├── FileTransferino.Data ──→ FileTransferino.Core, FileTransferino.Infrastructure
    ├── FileTransferino.Security ──→ FileTransferino.Infrastructure
    ├── FileTransferino.Infrastructure
    └── FileTransferino.UI

FileTransferino.Mobile
    └── (separate mobile-specific dependencies)
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
    │   └── errors.log             (Application error log)
    ├── themes/
    ├── secrets/                    (Encrypted credentials - Windows DPAPI)
    │   └── *.dat                  (Encrypted password files)
    └── settings.json              (Application settings)
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
- **Submenu Navigation**: Themes grouped under "Themes..." entry
- **Theme Preview**: Arrow keys preview themes in real-time
- **Escape Behavior**: Backs out of submenu, or closes palette at top level
- **Fuzzy Search**: Filter commands by name or category
- **Keyboard Navigation**: Use arrow keys and Enter to execute
- **Single-Click Apply**: Click a theme to apply it immediately
- **Adaptive Watermarks**: Placeholder text adapts to each theme for visibility

### Site Manager (Slice 2)
- **Theme Inheritance**: Automatically matches the user's selected theme
- **Secure Credential Storage**: Passwords encrypted with Windows DPAPI
- **FTP/FTPS/SFTP Support**: Configure connection profiles for all protocols
- **Profile Management**: Create, edit, and delete site profiles
- **Database Persistence**: Site profiles stored in SQLite (002_sites.sql migration)
- **Encrypted Secrets**: Credentials stored in `{Root}/secrets/` folder (never in database)
- **Main Application View**: The Site Manager is now embedded directly in the main window (no separate window required)
- **CRUD Operations**: 
  - Add new sites (New button)
  - Edit existing sites (select from list)
  - Delete sites (with confirmation dialog)
  - Auto-port selection (FTP/FTPS=21, SFTP=22)

### Welcome Overlay (First Run)
- **Behavior**: A compact, interruptible banner appears on first run to show quick tips and shortcuts
- **Duration**: 4 seconds (auto-dismiss) or dismissible via click or any key
- **Style**: Semi-transparent backdrop with a centered card showing shortcuts and app name

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
  - Real-time theme preview while navigating
  - Single-click theme application
- **Slice 2 Site Manager implemented:**
  - SiteProfile model with required properties
  - 002_sites.sql migration (Sites table with timestamps)
  - ISiteRepository + SiteRepository (Dapper-based CRUD)
  - ICredentialStore + WindowsDpapiCredentialStore (Windows DPAPI encryption)
  - SiteManagerView + SiteManagerViewModel (MVVM pattern)
  - Secure password storage (encrypted files in secrets/ folder)
  - Site Manager is the main embedded view
  - Full CRUD operations with confirmation dialogs

## Documentation

- **[Development Instructions](.github/INSTRUCTIONS.md)** - Essential commands and workflow guidelines
- **[Solution Summary](solution-summary.md)** - Detailed build status and project information

> 📝 Remember to keep all documentation files updated when making structural changes!
