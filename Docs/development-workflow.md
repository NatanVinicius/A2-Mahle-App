# Development Workflow

## 1. Start From an Issue

Every feature should have a clear GitHub Issue.

The Issue should define:

- objective;
- scope;
- constraints;
- acceptance criteria.

The AI should implement one feature at a time.

## 2. Analyze Before Coding

Before changing code, the AI should:

1. Read the issue.
2. Read the relevant existing implementation.
3. Identify affected projects.
4. Identify existing contracts and services.
5. Determine the minimum required changes.

For complex changes, the AI should present the plan before implementation.

## 3. Implement

Implementation should:

- follow the existing architecture;
- reuse existing code;
- minimize changes;
- avoid unrelated refactoring;
- register new dependencies in DI when required.

## 4. Validate

After implementation:

- build the solution;
- verify compilation;
- verify dependency injection;
- verify the affected flow;
- use Fake implementations when hardware is unavailable.

## 5. Review

Before considering the feature complete, verify:

- no unnecessary abstractions were introduced;
- no architectural boundaries were violated;
- Client does not access Infrastructure;
- external SDK code remains in Infrastructure;
- persistence remains behind Application contracts.

## 6. Commit

Create a focused commit after a feature or meaningful implementation step.

Prefer commits that describe one logical change.

Examples:

feat: add inspection persistence

feat: add production repository

feat: add history filters

fix: preserve production state on navigation

## 7. Issue Completion

A feature is complete only when its acceptance criteria are satisfied and the solution builds successfully.