# C# Instructions

## General

- Follow the existing coding style.
- Prefer explicit and readable code.
- Use nullable reference types.
- Use dependency injection.
- Prefer sealed concrete services when appropriate.
- Use async/await for I/O operations.
- Use CancellationToken for cancellable asynchronous operations.

## Naming

Use descriptive names.

Avoid unnecessary abbreviations.

Keep existing project naming conventions.

## Services

Services should have a clear responsibility.

Do not create a service merely to wrap a single trivial operation unless there is a concrete architectural reason.

## Interfaces

Create an interface when:

- Application requires a contract;
- Infrastructure needs to implement an external dependency;
- the abstraction provides a real architectural boundary.

Do not create interfaces for every class automatically.

## Error Handling

Do not silently swallow unexpected exceptions.

Handle errors at the appropriate application boundary.

Do not add broad try/catch blocks without a concrete reason.