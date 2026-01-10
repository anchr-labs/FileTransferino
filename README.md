﻿# FtpClient Solution

A .NET 10 solution for an FTP Server application with clean architecture.

## Project Structure

### FtpServer.App (Avalonia UI Application)
- **Type**: Desktop Application (Avalonia UI)
- **Target Framework**: .NET 10
- **Purpose**: User interface for the FTP server
- **Dependencies**: References all other projects

### FtpServer.Core (Class Library)
- **Type**: Class Library
- **Target Framework**: .NET 10
- **Purpose**: Core business logic and domain models
- **Dependencies**: None

### FtpServer.Data (Class Library)
- **Type**: Class Library
- **Target Framework**: .NET 10
- **Purpose**: Data access layer and repository patterns
- **Dependencies**: None

### FtpServer.Security (Class Library)
- **Type**: Class Library
- **Target Framework**: .NET 10
- **Purpose**: Security, authentication, and authorization logic
- **Dependencies**: None

### FtpServer.Infrastructure (Class Library)
- **Type**: Class Library
- **Target Framework**: .NET 10
- **Purpose**: External services, logging, and infrastructure concerns
- **Dependencies**: None

## Dependency Graph

```
FtpServer.App
    ├── FtpServer.Core
    ├── FtpServer.Data
    ├── FtpServer.Security
    └── FtpServer.Infrastructure
```

All class libraries are independent with no circular dependencies.

## Building the Solution

```powershell
dotnet build FtpClient.sln
```

## Running the Application

```powershell
dotnet run --project FtpServer.App\FtpServer.App.csproj
```

## Notes

- All projects target .NET 10
- Nullable reference types are enabled across all projects
- Implicit usings are enabled in class libraries
- The App project uses compiled bindings by default for Avalonia
- Placeholder classes exist in each class library to establish namespace structure
- No business logic is implemented yet - this is a skeleton solution

## Documentation

- **[Development Instructions](.github/INSTRUCTIONS.md)** - Essential commands and workflow guidelines
- **[Solution Summary](solution-summary.md)** - Detailed build status and project information

> 📝 Remember to keep all documentation files updated when making structural changes!
