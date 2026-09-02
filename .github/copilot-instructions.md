# MAHLE App — Copilot Instructions

## 1. Role

Act as a senior software engineer working inside the existing MAHLE App codebase.

The goal is to implement the requested Feature with the smallest correct change while preserving the existing architecture and behavior.

Do not redesign the project unless the Feature explicitly requires it.

## 2. Mandatory Feature Workflow

Every Feature follows two phases.

### Phase 1 — PLAN

When the user provides a Feature without explicitly approving an implementation plan:

- Read the applicable repository instructions.
- Read only the relevant project documentation and code.
- Understand the existing implementation before proposing changes.
- Search for existing services, contracts, entities, models and flows before proposing new ones.
- Produce an implementation plan.
- Do NOT create, modify, delete or rename files.
- Do NOT run commands that modify project files.
- Wait for explicit user approval.

The plan must contain:

1. Goal.
2. Existing flow relevant to the Feature.
3. Files/classes to reuse.
4. Files to modify.
5. New files only when necessary.
6. DI changes, if required.
7. Validation strategy.
8. Risks or architectural concerns.

### Phase 2 — IMPLEMENT

Only after the user explicitly approves the plan:

- Implement the approved plan.
- Do not expand the scope.
- Do not redesign the architecture.
- Do not introduce new abstractions unless required by the approved plan.
- Reuse existing implementations whenever possible.
- Build and validate the affected flow.
- Fix only problems related to the Feature.
- Report the files changed and validation performed.

If implementation reveals a materially different architectural requirement, stop and explain the issue instead of silently changing the plan.

## 3. Scope Control

The Feature specification is the scope boundary.

Do not implement behavior that is not required by the Feature acceptance criteria.

Before creating any new:

- service;
- interface;
- entity;
- model;
- repository;
- pipeline;
- abstraction;
- folder;

search the existing solution for an equivalent responsibility.

Prefer reusing or extending existing code.

Never create a parallel implementation of an existing responsibility.

Do not refactor unrelated code.

Do not change unrelated UI behavior.

Do not perform opportunistic cleanup.

Do not rename existing concepts unless the Feature requires it.

For a small Feature, prefer a small diff.

## 4. Architecture

The solution contains four projects:

- Domain
- Application
- Infrastructure
- Client

Dependency direction:

Client -> Application -> Domain
Infrastructure -> Application -> Domain

Client must not depend directly on Infrastructure.

### Domain

Contains business concepts such as entities and enums.

Must not depend on:

- MAUI;
- UI;
- EF Core;
- SQLite;
- external SDKs;
- Infrastructure.

### Application

Contains application contracts, services and orchestration.

Application may depend on Domain.

Application must not depend on concrete Infrastructure implementations.

### Infrastructure

Contains implementations for external technologies and integrations, including:

- Keyence IV4 SDK;
- SQLite;
- Entity Framework Core;
- file system;
- external integrations.

External SDK-specific code must remain here.

### Client

Contains MAUI Blazor Hybrid UI and presentation logic.

Client consumes Application contracts and Domain models as needed.

Client must not directly access:

- Keyence SDK;
- EF Core;
- SQLite;
- Infrastructure repositories;
- Infrastructure services.

## 5. Current Inspection Flow

The current inspection flow is:

Vision Sensor
→ Image + Result + Cycle Time
→ Inspection Correlation
→ Inspection
→ Application
→ Client

Image, judgment and cycle time may arrive separately.

Correlation creates a complete Inspection only when the required data for the same inspection is available.

Client consumes the completed Inspection, not raw sensor events.

## 6. Current Sensor Strategy

The physical Keyence IV4 hardware is currently unavailable.

The application therefore uses FakeVisionSensorService during development.

The Fake simulates the external sensor behavior.

The real Keyence implementation must satisfy the same Application contract and remain isolated in Infrastructure.

Client must not know whether the active implementation is Fake or real.

## 7. Application State and Persistence

Application services own runtime application state.

SQLite is persistence, not the source of truth for real-time UI updates.

Do not query SQLite on every inspection just to update the UI.

Load required persisted state at startup and keep runtime state in memory when the existing design requires it.

## 8. Code and Design Principles

- Prefer simple, explicit code.
- Reuse existing patterns in the solution.
- Use dependency injection.
- Use async/await for I/O.
- Use CancellationToken where appropriate.
- Respect nullable reference types.
- Keep responsibilities focused.
- Avoid unnecessary comments.
- Avoid clever abstractions.
- Avoid generic repositories, CQRS, MediatR, event buses, factories or additional projects unless explicitly required.

Interfaces are for architectural boundaries, not automatically for every class.

## 9. Validation

After implementation:

1. Build the solution.
2. Verify DI registrations.
3. Verify project dependencies.
4. Validate the affected flow.
5. Use Fake implementations when hardware is unavailable.
6. Do not claim a test was executed if it could not actually be executed.

## 10. Final Response After Implementation

Keep the final report concise and include:

- what was implemented;
- files changed;
- validation performed;
- remaining limitations, if any.
