# MAHLE App - AI Context

## Project

MAHLE App is a desktop application for monitoring industrial inspection lines.

The application receives inspection data from a vision sensor, correlates the received data into a complete inspection, updates the UI in real time and persists inspection and production information.

## Technology

- C#
- .NET
- MAUI Blazor Hybrid
- Entity Framework Core
- SQLite

## Solution Structure

The solution is divided into four projects:

- Domain
- Application
- Infrastructure
- Client

## Architecture

Dependency direction:

Client -> Application -> Domain
Infrastructure -> Application -> Domain

Client must not access Infrastructure directly.

Folders are organized by feature.

## Inspection Flow

Vision Sensor
    ↓
Image + Result + Cycle Time
    ↓
Inspection Correlation
    ↓
Inspection
    ↓
Application
    ↓
Client

Image, judgment and cycle time may be received separately.

Correlation creates the complete Inspection only when the required data for the same inspection is available.

## Current Vision Sensor

The physical Keyence IV4 hardware is currently unavailable.

The application currently uses FakeVisionSensorService.

The Fake simulates:

- connection;
- disconnection;
- reconnection;
- image reception;
- result reception;
- cycle time reception;
- Approved and Rejected inspections;
- inspections every 3 seconds.

The future real implementation will use the Keyence IV4 SDK.

The real SDK implementation belongs in Infrastructure and must satisfy the same Application contract used by the Fake.

## Inspection

An Inspection contains:

- Date and time;
- Judgment;
- Cycle time;
- Image.

The Client consumes completed inspections through Application and does not access the sensor or SDK directly.

## Production

Production contains:

- Date;
- Produced;
- Approved;
- Rejected.

Production state is loaded at application startup and maintained in memory during execution.

The database is not queried on every inspection for real-time UI updates.

## Persistence

SQLite is used for local persistence.

Entity Framework Core is used as the ORM.

The database is stored under:

Data/db/mahle.db

## History

The History screen provides:

- date filtering;
- judgment filtering;
- production view;
- inspection view.

Default filters:

- current day;
- production;
- all judgments.

Inspection results are displayed with vertical scrolling without breaking the table layout.

## MVP Scope

### Included

1. Connection and reception of inspection data.
2. Real-time UI updates.
3. Saving rejected inspection images.
4. Local persistence.
5. PDF report export.

### Not included

- Advanced statistical trend charts.

## Important Constraints

- Keep the architecture simple.
- Avoid overengineering.
- Reuse existing services and contracts.
- Keep SDK-specific code inside Infrastructure.
- Keep persistence implementation inside Infrastructure.
- Client must not access SQLite or EF Core directly.
- Do not modify unrelated parts of the system.

## AI Development Workflow

Every Feature is handled in two phases:

1. PLAN — analyze the Feature and existing code without changing files.
2. IMPLEMENT — only after explicit user approval of the plan.

The plan is the checkpoint that prevents unnecessary architecture changes and scope expansion.
