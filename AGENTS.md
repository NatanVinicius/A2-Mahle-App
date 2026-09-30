# AGENTS.md

MAHLE App — a Windows-only .NET MAUI Blazor Hybrid desktop app that monitors industrial inspection lines. It receives data from a vision sensor, correlates it into complete inspections, updates the UI in real time, and persists to SQLite.

## Read these first

- `.github/copilot-instructions.md` — binding workflow + architecture rules. Read before doing any work.
- `AI-CONTEXT.md` — current project context and MVP scope.
- `Docs/architecture.md`, `Docs/development-workflow.md` — design and process.

`README.md` is a stale .NET 9 MAUI template description — do NOT trust its framework version, architecture, or doc links. The app targets `net10.0`.

## Mandatory workflow

Every feature has two phases: **PLAN** (analyze, write a plan, change no files) then **IMPLEMENT** (only after explicit user approval). Do not expand scope, refactor unrelated code, or add abstractions not required by the plan. Search for existing services/contracts before introducing new ones. Commit with `feat:`/`fix:` prefixes describing one logical change.

## Projects and dependency rules

Four projects under `src/`, all `net10.0` (Client also targets `net10.0-windows10.0.19041.0`):

- `A2-Mahle-App.Domain` — entities + enums; no external dependencies.
- `A2-Mahle-App.Application` — services + contracts (interfaces) consumed by Client, implemented by Infrastructure.
- `A2-Mahle-App.Infrastructure` — EF Core/SQLite, vision sensor, file system.
- `A2-Mahle-App.Client` — MAUI Blazor UI + composition root (`MauiProgram.cs`).

Dependency direction: `Client → Application → Domain`, `Infrastructure → Application → Domain`. Client code must not use Infrastructure, SQLite, EF Core, or sensor types.

**Gotcha:** `A2-Mahle-App.Client.csproj` *does* reference Infrastructure, but only so `MauiProgram.cs` can call `AddInfrastructure(...)`. That DI call is the single allowed touchpoint — keep all other Client code on Application contracts.

Folders are organized by feature (`Features/<Feature>/...` in Domain/Application/Infrastructure, `Components/...` in Client).

## Naming gotcha

Assembly names use hyphens (`A2-Mahle-App.Client`); root namespaces drop them (`A2MahleApp.Client`). Use file-scoped namespaces under `A2MahleApp.*`. `.editorconfig` enforces 2-space indent, CRLF, braces always, and explicit accessibility modifiers.

## Build & run

- Requires .NET 10 SDK and Node.js. Windows-only.
- Node deps live in the Client project; the build fails hard if they are missing:
  ```
  cd src/A2-Mahle-App.Client
  npm install
  ```
- Tailwind CSS is compiled during build (`npm run build:css` → `wwwroot/app.css`). During UI/CSS work run `npm run watch:css` from the Client folder.
- There are no test projects — `dotnet build` is the only automated verification:
  ```
  dotnet build A2-Mahle-App.sln
  ```
- Both `A2-Mahle-App.sln` and `A2-Mahle-App.slnx` list the same projects; either builds.

## Runtime specifics

- SQLite DB at `AppContext.BaseDirectory/Data/db/mahle.db`, created on startup. EF Core **migrations are not used** — schema comes from `EnsureCreatedAsync()` plus a hand-written SQL migration in `InspectionRepository.InitializeDatabaseAsync()` (it migrated the `Inspections` table from an `Image` column to `EvidenceImagePath`). Do not add EF migrations or delete that manual migration logic.
- `Production` state is loaded once at startup and kept in memory; do not query SQLite per inspection for real-time UI.
- PDF export shells out to Node (`src/A2-Mahle-App.Client/PdfExport/render-history-pdf.cjs`, puppeteer). Requires `npm install` (puppeteer downloads Chromium) and `node` on PATH at runtime.
- The physical Keyence IV4 is unavailable, so `FakeVisionSensorService` (Infrastructure) is wired in. A future real implementation must implement `IVisionSensorService` (Application) and stay in Infrastructure; Client must not distinguish fake from real.
- DI registration: `ApplicationServiceCollectionExtensions.AddApplication` (Application) and `InfrastructureServiceCollectionExtensions.AddInfrastructure` (Infrastructure). Register new services there.
