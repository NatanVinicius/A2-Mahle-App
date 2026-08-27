# MAHLE App - Copilot Instructions

## General Rules

Before changing code:

1. Inspect the existing implementation.
2. Understand the current architecture and flow.
3. Identify the minimum number of files that need to change.
4. Reuse existing contracts, services and models whenever possible.
5. Do not modify unrelated code.

## Architecture

The solution contains four projects:

- Domain
- Application
- Infrastructure
- Client

Respect the dependency direction:

Client -> Application -> Domain

Infrastructure -> Application -> Domain

Client must not depend directly on Infrastructure.

## Layer Responsibilities

### Domain

Contains:

- entities;
- enums;
- domain concepts;
- domain rules.

Domain must not depend on:

- EF Core;
- SQLite;
- MAUI;
- UI;
- external SDKs.

### Application

Contains:

- application contracts;
- application services;
- orchestration;
- use cases.

Application must not depend on concrete Infrastructure implementations.

### Infrastructure

Contains implementations for:

- external SDKs;
- SQLite;
- Entity Framework Core;
- file system;
- external integrations.

External SDK-specific code must remain inside Infrastructure.

### Client

Contains:

- UI;
- pages;
- components;
- ViewModels/presentation logic.

Client consumes Application contracts.

Client must not directly access:

- SQLite;
- Entity Framework Core;
- Keyence SDK;
- Infrastructure repositories;
- Infrastructure services.

## Development Principles

Prefer the simplest solution that satisfies the requirement.

Do not introduce:

- unnecessary abstractions;
- generic repositories;
- CQRS;
- MediatR;
- event buses;
- unnecessary factories;
- unnecessary design patterns;
- additional projects;

unless explicitly requested.

Do not refactor unrelated code while implementing a feature.

Do not rename existing concepts without a concrete reason.

## Feature Implementation Workflow

For a new feature:

1. Read the relevant issue.
2. Read the relevant existing code.
3. Identify the affected layers.
4. Propose the implementation before making large changes.
5. Implement the smallest required change.
6. Register required dependencies in DI.
7. Build the solution.
8. Fix compilation errors.
9. Verify the affected flow.
10. Summarize the changes.

## Existing Inspection Flow

The current inspection flow is:

Vision Sensor
→ Correlation
→ Inspection
→ Application
→ Client

The Client must consume a complete Inspection.

The Client must not correlate raw sensor data.

## Persistence Rules

The UI must not use the database as its real-time state source.

Application services maintain runtime state.

SQLite is used for persistence and historical queries.

Do not query SQLite on every inspection just to update the UI.

## Fake Implementations

Fake implementations are allowed when hardware or external systems are unavailable.

A Fake must implement the same Application contract used by the real implementation.

The Client must not know whether the current implementation is Fake or real.

## Code Changes

When implementing a requested change:

- keep existing naming conventions;
- preserve nullable reference types;
- use async/await for I/O;
- use CancellationToken where appropriate;
- use dependency injection;
- prefer explicit code over clever abstractions;
- keep methods focused;
- avoid unnecessary comments.

## Validation

After implementation:

- build the solution;
- inspect compilation errors;
- verify DI registrations;
- verify project dependencies;
- verify the affected flow.

Do not claim that code was tested if it could not actually be executed.