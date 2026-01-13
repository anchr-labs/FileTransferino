# GitHub Instructions for FileTransferino Project

## Essential Development Guidelines

### 📝 Documentation Updates
**IMPORTANT:** Always keep `README.md` and `solution-summary.md` up to date whenever:
- Adding new projects or features
- Changing project structure or dependencies
- Modifying build configurations
- Completing major iterations or milestones
- Making architectural changes

These files serve as the source of truth for the project structure and should reflect the current state.

---

### 🔨 Build & Run Commands

**Build desktop projects (recommended):**
```powershell
dotnet build FileTransferino.Desktop.slnf
```

**Build the entire solution (includes Mobile - requires SDKs):**
```powershell
dotnet build FileTransferino.slnx
```
⚠️ Note: Building the full solution requires Android SDK API 36, iOS SDK, and WebAssembly tools. Use `FileTransferino.Desktop.slnf` for desktop development.

**Clean build artifacts:**
```powershell
dotnet clean FileTransferino.Desktop.slnf
```

**Rebuild from scratch:**
```powershell
dotnet clean FileTransferino.Desktop.slnf; dotnet build FileTransferino.Desktop.slnf
```

**Run the Avalonia app:**
```powershell
dotnet run --project FileTransferino.App\FileTransferino.App.csproj
```

**Restore NuGet packages:**
```powershell
dotnet restore FileTransferino.slnx
```

---

### 🏗️ Project Structure Rules

- **FileTransferino.App** → References ALL other projects
- **Class Libraries** → Should remain independent (no cross-references)
- All projects target **.NET 10**
- Maintain **clean architecture** principles
- No circular dependencies allowed

---

### 📦 Adding New Projects

When adding a new project to the solution:

1. Create the project:
   ```powershell
   dotnet new classlib -n ProjectName -o ProjectName
   ```

2. Add to solution:
   ```powershell
   dotnet sln FileTransferino.slnx add ProjectName\ProjectName.csproj
   ```

3. Add references if needed:
   ```powershell
   dotnet add FileTransferino.App\FileTransferino.App.csproj reference ProjectName\ProjectName.csproj
   ```

4. **Update README.md and solution-summary.md** with the new project info

---

### 🧪 Testing

**Run all tests (when test projects exist):**
```powershell
dotnet test FileTransferino.slnx
```

**Run tests with verbose output:**
```powershell
dotnet test FileTransferino.slnx --logger "console;verbosity=detailed"
```

---

### 📋 Solution Management

**List all projects in solution:**
```powershell
dotnet sln FileTransferino.slnx list
```

**Check for errors:**
```powershell
dotnet build FileTransferino.slnx --no-incremental
```

---

### 🔍 Common Tasks

**Update all NuGet packages:**
```powershell
dotnet list package --outdated
dotnet add package PackageName --version x.x.x
```

**Check project references:**
```powershell
dotnet list reference
```

**Format code:**
```powershell
dotnet format FileTransferino.slnx
```

---

### 🚀 Git Workflow

**Before committing:**
1. Ensure solution builds: `dotnet build FileTransferino.slnx`
2. Update documentation if project structure changed
3. Review changes: `git diff`
4. Stage changes: `git add .`
5. Commit: `git commit -m "Descriptive message"`

**Branch naming conventions:**
- `feature/feature-name` - New features
- `bugfix/issue-description` - Bug fixes
- `refactor/description` - Code refactoring
- `docs/description` - Documentation updates

---

### ⚠️ Important Notes

- Always build after pulling changes
- Keep target frameworks synchronized across projects
- Update documentation as part of your workflow, not as an afterthought
- Use meaningful commit messages that explain "why" not just "what"
- Test the solution builds before pushing to remote

---

### 📚 Documentation Files

- **README.md** - High-level project overview and structure (root directory)
- **solution-summary.md** - Detailed solution status and build information (root directory)
- **.github/INSTRUCTIONS.md** - This file - development guidelines

**Keep all three synchronized with project changes!**
