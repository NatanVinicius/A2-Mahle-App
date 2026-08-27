# Architecture Instructions

## Dependency Direction

Allowed:

Client -> Application
Client -> Domain

Application -> Domain

Infrastructure -> Application
Infrastructure -> Domain

## Forbidden Dependencies

Client -> Infrastructure

Domain -> Application

Domain -> Infrastructure

Domain -> Client

Application -> Infrastructure

## External Integrations

External systems must be isolated in Infrastructure.

Examples:

- Keyence IV4 SDK;
- SQLite;
- Entity Framework Core;
- file system.

Application defines the contract when the application needs to communicate with an external system.

Infrastructure implements the contract.

## UI

The UI consumes Application services and application/domain models.

The UI must not contain integration logic.

The UI must not know how an external system works.

## State

Application services are responsible for application runtime state.

UI components should present state rather than become the source of truth.