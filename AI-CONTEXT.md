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

Infrastructure contains implementations for external technologies and integrations.

Folders are creating by features

## Current Inspection Flow

Vision Sensor
    ↓
Image + Result
    ↓
Inspection Correlation
    ↓
Inspection
    ↓
Inspection Service
    ↓
Client

## Current Vision Sensor

The application currently uses FakeVisionSensorService.

The Fake simulates the behavior of the real vision sensor because the physical Keyence hardware is currently unavailable.

The Fake:

- simulates connection;
- simulates reconnection;
- sends an image;
- sends the inspection result;
- sends cycle time;
- alternates between Approved and Rejected;
- generates inspections every 3 seconds.

The future real implementation will use the Keyence IV4 SDK.

## Inspection Correlation

Image and inspection result are received separately.

An Inspection is created only when the required data for the same inspection is available.

The correlation layer is responsible for composing the complete Inspection.

## Inspection

An Inspection contains:

- Date and time;
- Judgment;
- Cycle time;
- Image.

The Client consumes completed inspections and does not access the sensor or SDK directly.

## Production

Production contains:

- Date;
- Produced;
- Approved;
- Rejected.

Production state is loaded when the application starts and maintained in memory during execution.

The database is updated when inspections are completed.

The UI does not query SQLite for real-time inspection data.

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

## Current MVP Scope

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
- Do not introduce unnecessary abstractions.
- Do not create unnecessary projects.
- Reuse existing services and contracts.
- Keep SDK-specific code inside Infrastructure.
- Keep persistence implementation inside Infrastructure.
- Client must not access SQLite or EF Core directly.
- Do not modify unrelated parts of the system.