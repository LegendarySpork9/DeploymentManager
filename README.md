# DeploymentManager [![Commit Status](https://github.com/LegendarySpork9/DeploymentManager/actions/workflows/Commit.yml/badge.svg?event=push)](https://github.com/LegendarySpork9/DeploymentManager/actions/workflows/Commit.yml) [![PR Status](https://github.com/LegendarySpork9/DeploymentManager/actions/workflows/Pull%20Request.yml/badge.svg)](https://github.com/LegendarySpork9/DeploymentManager/actions/workflows/Pull%20Request.yml)

Web application for orchestrating remote deployments. Fetches build artefacts from GitHub Actions or Releases and deploys to target machines via IIS and Windows Task Scheduler. Includes TOTP-based approval and deployment history tracking.

## Tech Stack

| Technology | Version |
|------------|---------|
| .NET | 10.0 |
| Language | C# |
| UI Framework | Blazor Server |
| IIS Management | Microsoft.Web.Administration |
| Authentication | TOTP (Otp.NET) |

## Status

Active development

## Documentation

Setup, configuration, and usage documentation is available on the [LegendarySpork9 Wiki](https://wiki.legendaryspork9.co.uk).

---

Copyright © 2026 Toby Hunter, All rights reserved.
