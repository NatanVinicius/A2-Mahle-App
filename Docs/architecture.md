# Architecture

## Overview

MAHLE App is organized into four projects:

- Domain
- Application
- Infrastructure
- Client

The architecture follows a simplified Clean Architecture approach.

The project intentionally avoids unnecessary complexity because it is a small application maintained by a single developer.

## Projects

### Domain

Responsible for core business concepts.

Contains entities and enums.

Has no dependency on external technologies.

### Application

Responsible for application behavior and contracts.

Contains application services and interfaces consumed by the Client and implemented by Infrastructure.

### Infrastructure

Responsible for external technologies and implementations.

Examples:

- Keyence IV4 SDK;
- SQLite;
- Entity Framework Core;
- file system.

### Client

Responsible for the MAUI Blazor Hybrid UI.

The Client consumes Application contracts.

## Inspection Flow

The external vision sensor provides inspection information.

The information may arrive separately.

The correlation layer combines the required data into a complete Inspection.

Flow:

Vision Sensor
→ Image / Result / Cycle Time
→ Inspection Correlation
→ Inspection
→ Application
→ Client

## Persistence Flow

Completed inspections are persisted through Application contracts.

Flow:

Inspection
→ Application Service
→ Repository Contract
→ Infrastructure Repository
→ Entity Framework Core
→ SQLite

The Client does not access SQLite directly.

## Production State

Production information is loaded when the application starts.

After initialization, the current production state is maintained in memory.

Completed inspections update the runtime state.

Persistence is performed separately.

The UI consumes the runtime application state rather than querying SQLite for every inspection.

## External Sensor

The current implementation uses a Fake sensor because the physical Keyence hardware is unavailable.

The future implementation will replace the Fake with the Keyence IV4 SDK implementation.

Both implementations must satisfy the same Application contract.

The Client must not change when switching between them.