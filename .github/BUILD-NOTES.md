# Build Configuration Notes

## Mobile Projects and Solution Filter

The FileTransferino solution includes Mobile projects (Android, iOS, Browser) that require specific SDKs. To simplify desktop development, a **solution filter** (`FileTransferino.Desktop.slnf`) has been created that excludes these projects.

### Why This Was Done

- **Android SDK Requirement**: The Android project requires API Level 36 to be installed
- **iOS SDK Requirement**: iOS projects require macOS with Xcode
- **Focus on Desktop**: Primary development is on the Avalonia desktop application
- **Faster Builds**: Excluding mobile projects significantly speeds up build times
- **Optional Development**: Mobile projects can be built explicitly when needed

### Building the Solution

#### Option 1: Build Desktop Projects Only (Recommended) ⭐
Use the solution filter to build only desktop-compatible projects:
```powershell
dotnet build FileTransferino.Desktop.slnf
```

This builds:
- FileTransferino.App (Desktop Avalonia app)
- FileTransferino.Core
- FileTransferino.Data
- FileTransferino.Infrastructure
- FileTransferino.Security
- FileTransferino.UI
- FileTransferino.Mobile (shared code)
- FileTransferino.Mobile.Desktop

#### Option 2: Build Just the Main Desktop App
Fastest option when you only need the desktop app:
```powershell
dotnet build FileTransferino.App\FileTransferino.App.csproj
```

#### Option 3: Run Without Building
```powershell
dotnet run --project FileTransferino.App\FileTransferino.App.csproj
```

#### Option 4: Build Everything (Will Fail Without SDKs)
This will attempt to build ALL projects including Android/iOS/Browser:
```powershell
dotnet build FileTransferino.slnx
```
⚠️ **Warning**: This will fail with Android SDK errors unless you have all mobile SDKs installed.

### Building Mobile Projects

If you need to build the mobile projects, you can:

#### Build Android Project
```powershell
dotnet build FileTransferino.Mobile\FileTransferino.Mobile.Android\FileTransferino.Mobile.Android.csproj
```

First, install the required Android SDK:
```powershell
dotnet build -t:InstallAndroidDependencies -f net10.0-android "-p:AndroidSdkDirectory=C:\Android" FileTransferino.Mobile\FileTransferino.Mobile.Android\FileTransferino.Mobile.Android.csproj
```

#### Build iOS Project
```powershell
dotnet build FileTransferino.Mobile\FileTransferino.Mobile.iOS\FileTransferino.Mobile.iOS.csproj
```

#### Build Browser Project
```powershell
dotnet build FileTransferino.Mobile\FileTransferino.Mobile.Browser\FileTransferino.Mobile.Browser.csproj
```

### Re-enabling Mobile Projects in Solution Builds

If you want to re-enable mobile projects to build automatically with the solution:

1. Open `FileTransferino.slnx` in a text editor
2. Find the project GUID entries for the mobile projects
3. Add back the `.Build.0` lines that were removed

For example, for Android:
```
{EF76F95B-A20F-4212-9880-78F2E7F40D5B}.Debug|Any CPU.Build.0 = Debug|Any CPU
```

### Current Project Build Status

| Project | Builds by Default | Notes |
|---------|-------------------|-------|
| FileTransferino.App | ✅ Yes | Main desktop application |
| FileTransferino.Core | ✅ Yes | Core business logic |
| FileTransferino.Data | ✅ Yes | Data access layer |
| FileTransferino.Security | ✅ Yes | Security/credentials |
| FileTransferino.Infrastructure | ✅ Yes | Infrastructure services |
| FileTransferino.UI | ✅ Yes | UI components library |
| FileTransferino.Mobile | ✅ Yes | Mobile shared code |
| FileTransferino.Mobile.Desktop | ✅ Yes | Mobile desktop target |
| FileTransferino.Mobile.Android | ❌ No | Requires Android SDK API 36 |
| FileTransferino.Mobile.iOS | ❌ No | Requires macOS/Xcode |
| FileTransferino.Mobile.Browser | ❌ No | WebAssembly target |

---

**Last Updated**: January 11, 2026
