# Deployment Manager - Infrastructure Document

## Overview

Deployment Manager is a self-hosted web application for orchestrating remote deployments of applications and services. It integrates with GitHub to fetch build artefacts and manages deployment to target machines via IIS and Windows Task Scheduler.

- **Author:** Hunter Industries / Toby Hunter
- **Version:** 1.0.0
- **Repository:** https://github.com/LegendarySpork9/DeploymentManager

## Technology Stack

| Component | Technology | Version |
|---|---|---|
| Framework | ASP.NET Core (Blazor Server) | .NET 10.0 |
| Language | C# | Latest |
| UI | Razor Components (Interactive Server) | - |
| CSS Framework | Bootstrap | 5.x |
| Logging | log4net | 3.3.1 |
| HTTP Client | RestSharp | 114.0.0 |
| JSON Serialisation | Newtonsoft.Json | 13.0.4 |
| IIS Management | Microsoft.Web.Administration | 11.1.0 |
| Task Scheduling | TaskScheduler | 2.12.2 |
| TOTP Authentication | Otp.NET | 1.4.1 |
| QR Code Generation | QRCoder | 1.8.0 |
| Testing | MSTest | 4.0.2 |
| Test SDK | Microsoft.NET.Test.Sdk | 18.0.1 |
| Mocking | Moq | 4.20.72 |
| Code Coverage | coverlet.collector | 6.0.2 |

## Solution Structure

```
DeploymentManager/
+-- DeploymentManager/              # Main web application
|   +-- Abstractions/               # Interface definitions
|   +-- Components/                 # Razor components
|   |   +-- Dialogs/                # Modal dialogs (Approval, File Upload)
|   |   +-- Layout/                 # Main layout and navigation
|   |   +-- Pages/                  # Application pages
|   |   +-- Shared/                 # Shared UI components
|   +-- Converters/                 # Value formatting and display converters
|   +-- Entities/                   # Enumerations
|   +-- Functions/                  # Utility functions
|   +-- Implementations/           # Interface implementations (wrappers)
|   +-- Models/                     # Data models
|   |   +-- Data/                   # Deployment and artefact models
|   |   +-- Forms/                  # Form input models
|   |   +-- Related/                # Supporting models
|   |   +-- Responses/              # API response models
|   |   +-- Shared/                 # Cross-cutting models
|   +-- Orchestrators/              # Deployment workflow orchestrators
|   |   +-- GitHub/                 # GitHub-specific deployment strategies
|   +-- Properties/                 # Launch settings
|   +-- Services/                   # Business logic services
|   +-- Values/                     # Constants and standard values
|   +-- wwwroot/                    # Static assets (CSS, JS)
+-- Tests/
|   +-- DeploymentManager.UnitTests/        # Unit tests — converters, functions, helpers only
|   |   +-- Converters/                     # Converter tests
|   |   +-- Functions/                      # Function tests
|   |   +-- Models/                         # Model transformation tests
|   +-- DeploymentManager.PersistenceTests/ # Persistence tests — file I/O, implementations
|   |   +-- Implementations/               # Wrapper tests (file system, GitHub client, clock)
|   |   +-- Services/                       # Service tests (approval, document, deployment history)
|   +-- DeploymentManager.IntegrationTests/ # Integration tests — orchestrators, service orchestration
|   |   +-- Orchestrators/                  # Deployment orchestrator tests
|   |   +-- Services/                       # GitHub, IIS, Task Scheduler service tests
+-- .github/workflows/              # CI/CD pipeline definitions
```

## Application Architecture

### Rendering Model

The application uses **Blazor Server** with Interactive Server Render Mode. The UI runs on the server and communicates with the browser over a SignalR (WebSocket) connection, enabling real-time updates during deployments.

### Dependency Injection

All services are registered as **singletons** in `Program.cs`. External dependencies are wrapped behind interfaces to support testability.

| Abstraction | Implementation | Purpose |
|---|---|---|
| `ILoggerService` | `LoggerServiceWrapper` | Application logging via log4net |
| `IFileSystem` | `FileSystemWrapper` | File system operations |
| `IGitHubClient` | `GitHubClientWrapper` | GitHub API communication |
| `IIISClient` | `IISClientWrapper` | IIS site and app pool management |
| `ITaskScheduler` | `TaskSchedulerWrapper` | Windows Task Scheduler management |
| `IClock` | `SystemClockProvider` | Time operations |

### Services

| Service | Responsibility |
|---|---|
| `ApprovalService` | TOTP credential management and deployment approval verification |
| `GitHubService` | Fetching artefacts from GitHub Actions and Releases |
| `DocumentService` | Artefact extraction and file management |
| `IISService` | Starting and stopping IIS sites and application pools |
| `TaskSchedulerService` | Starting and stopping Windows scheduled tasks |
| `DeploymentHistoryService` | Persisting and retrieving deployment history records |

### Orchestrators

| Orchestrator | Responsibility |
|---|---|
| `DeploymentOrchestrator` | Top-level deployment workflow coordination |
| `DeployActionsOrchestrator` | Deployment from GitHub Actions artefacts |
| `DeployReleasesOrchestrator` | Deployment from GitHub Release assets |
| `DeployUploadOrchestrator` | Deployment from manually uploaded files |

### Deployment Pipeline Stages

Deployments follow a staged pipeline defined by the `DeploymentStage` enum:

1. **FetchArtefacts** - Download artefacts from GitHub or accept uploaded files
2. **ExtractArtefacts** - Extract downloaded ZIP archives
3. **FetchArtefactFiles** - Identify files to deploy
4. **StopServices** - Stop IIS sites, app pools, or scheduled tasks on target machines
5. **MoveArtefacts** - Copy artefact files to the target deployment directory
6. **StartServices** - Restart IIS sites, app pools, or scheduled tasks
7. **CleanArtefacts** - Remove temporary artefact files

### Deployment Status Tracking

Each deployment progresses through statuses defined by the `Status` enum:

- `PendingApproval` - Awaiting TOTP approval
- `NotStarted` - Approved but not yet running
- `Running` - Deployment in progress
- `Completed` - Successfully deployed
- `CompletedWithWarnings` - Deployed with non-fatal issues
- `Skipped` - Stage was skipped
- `Failed` - Deployment failed

## Pages

| Page | Route | Purpose |
|---|---|---|
| Deployment Configuration | `/` | Configure and initiate deployments |
| Login | `/login` | Site authentication |
| Deployment History | `/deployment-history` | View past deployment records |
| Authenticator Setup | `/authenticator-setup` | Configure TOTP for deployment approvals |
| Error | `/Error` | Error display (production) |
| Not Found | `/not-found` | 404 page |

## Data Persistence

The application uses **file-based persistence** with no database dependency.

| Data | Storage Format | Location (Configurable) |
|---|---|---|
| Deployment History | JSON files (one per project) | `DeploymentHistoryLocation` |
| TOTP Credentials | JSON file (`credential.json`) | `ApprovalCredentialLocation` |
| Downloaded Artefacts | ZIP/extracted files | `ArtefactDownloadLocation` |
| Application Logs | Rolling text file | `Logs/DeploymentManager.log` |

## Authentication and Security

### Site Authentication

- Username and password login
- Credentials stored as a **Base64-encoded SHA512 hash** in `appsettings.json` (`SiteAuth`)
- Session managed via Blazor's `ProtectedSessionStorage` (encrypted browser session)

### Deployment Approval (TOTP)

- Time-based One-Time Password verification required before deployments execute
- 20-byte Base32-encoded secrets
- QR code generated for authenticator app enrolment
- Verification window: +/- 1 time step

### Remote Machine Authentication

- Windows impersonation via P/Invoke (`advapi32.dll`)
- Domain, username, and password credentials per environment
- Used for IIS and Task Scheduler operations on remote machines

### GitHub API Authentication

- Personal Access Token (Bearer token) configured in `GitHubOptions`
- Used for artefact and release downloads via the GitHub REST API

### Web Security

- HTTPS enforced with HSTS in production
- Antiforgery token validation on all forms
- Status code pages with re-execution for 404 handling

## External Integrations

### GitHub

- **API Base URL:** `https://api.github.com`
- **Client:** RestSharp HTTP library
- **Capabilities:**
  - Fetch GitHub Actions workflow artefacts
  - Download artefact ZIP archives
  - Fetch repository releases and assets
  - Download release assets
- **Authentication:** Bearer token (GitHub PAT)

### IIS (Internet Information Services)

- **Library:** Microsoft.Web.Administration
- **Capabilities:**
  - Connect to local and remote IIS instances
  - Stop and start IIS sites
  - Manage application pools
- **Authentication:** Windows impersonation for remote machines

### Windows Task Scheduler

- **Library:** TaskScheduler (managed wrapper)
- **Capabilities:**
  - Connect to remote task scheduler instances
  - Start and stop scheduled tasks
- **Authentication:** Windows impersonation for remote machines

## Configuration

### appsettings.json Structure

```json
{
  "AppSettings": {
    "SiteAuth": "<Base64-encoded SHA256 hash>",
    "DeploymentHistoryLocation": "<path to history directory>",
    "ApprovalCredentialLocation": "<path to TOTP credential directory>",
    "ArtefactDownloadLocation": "<path to artefact download directory>",
    "Environments": [
      {
        "Device": "<hostname or IP>",
        "Drive": "<drive letter>",
        "Name": "<Live|QA|Dev>",
        "ArtefactSource": "<Actions|Releases>",
        "Auth": {
          "Domain": "<optional domain>",
          "Username": "<username>",
          "Password": "<password>"
        }
      }
    ],
    "GitHubOptions": {
      "Auth": "<GitHub PAT>",
      "Owner": "<GitHub repository owner>"
    },
    "Projects": [
      {
        "Name": "<project display name>",
        "Type": "<Website|ConsoleApplication|API>",
        "Directory": "<deployment target directory>",
        "GitHub": {
          "Repository": "<repository name>",
          "Artefact": "<artefact name>"
        },
        "AdditionalDeploy": [],
        "Ignore": []
      }
    ]
  }
}
```

### Supported Project Types

| Type | Enum Value | Deployment Target |
|---|---|---|
| Website | `Website` | IIS Site + Application Pool |
| Console Application | `ConsoleApplication` | Windows Scheduled Task |
| API | `API` | IIS Site + Application Pool |

### Supported Environments

| Environment | Artefact Sources |
|---|---|
| Live | GitHub Actions, GitHub Releases |
| QA | GitHub Actions, GitHub Releases |
| Dev | GitHub Actions, GitHub Releases |

### Supported Deployment Types

| Type | Source |
|---|---|
| GitHub | Artefacts from GitHub Actions or Release assets |
| File Upload | Manually uploaded artefact files |

## Logging

- **Framework:** log4net 3.3.1
- **Output:** Rolling file appender
- **File:** `Logs/DeploymentManager.log`
- **Max File Size:** 10 MB
- **Backup Count:** 10 rolling files
- **Format:** `{ISO8601 Timestamp} {LEVEL} - {Message}`
- **Lock Model:** MinimalLock (concurrent access safe)
- **Contextual Data:** Log entries include an identifier that starts as the user's IP address and changes to `username (IP)` after login, tracked via the custom `ILoggerService` wrapper

## CI/CD

### GitHub Actions Workflows

All workflows run on `windows-latest` using .NET 10.0.x SDK.

| Workflow | Trigger | Steps |
|---|---|---|
| **CI on Commit** (`Commit.yml`) | Push to any branch | Checkout, Restore, Build (Release) |
| **CI on Pull Request** (`Pull Request.yml`) | PR to any branch | Checkout, Restore, Build (Release), Run Tests with Coverage (`dotnet test --collect:"XPlat Code Coverage"`), Generate Coverage Report, Post Coverage Status, Upload Coverage Artifact |
| **Check for Linked Issue** (`PR Linked Issue.yml`) | PR opened/edited/reopened/synchronised | Verifies PR has linked GitHub issues via description, comments, or Development section |

### Build Configuration

- **SDK:** .NET 10.0.x
- **Configuration:** Release
- **Test Runner:** `dotnet test` (MSTest)

### Code Coverage

- **Collector:** XPlat Code Coverage (via `coverlet.collector`)
- **Configuration:** `coverlet.runsettings` in solution root
- **Report Generator:** `dotnet-reportgenerator-globaltool`
- **Report Formats:** Cobertura, JsonSummary
- **Exclusions:** Program entry points, Models, Entities, generated code
- **CI Integration:** Coverage percentage posted to PR status and uploaded as artifact

## Hosting Requirements

### Runtime Prerequisites

- .NET 10.0 Runtime
- Windows Server (required for IIS and Task Scheduler integration)
- IIS with ASP.NET Core Hosting Bundle (if hosting in IIS)

### Network Requirements

- HTTPS (port 443) for client access
- Outbound HTTPS to `api.github.com` for artefact downloads
- Network access to target deployment machines for IIS and Task Scheduler management
- SMB/file share access to target deployment directories

### File System Requirements

- Read/write access to the deployment history directory
- Read/write access to the TOTP credential directory
- Read/write access to the artefact download directory
- Read/write access to the `Logs/` directory
- Write access to target deployment directories on remote machines

### Development Ports

| Profile | URL |
|---|---|
| HTTP | `http://localhost:5266` |
| HTTPS | `https://localhost:7121` |
