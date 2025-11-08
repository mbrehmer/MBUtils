## Quick orientation for AI coding agents

This repository is a small .NET utility library. The goal of this file is to provide concise, repository-specific guidance so an AI agent can be productive immediately.

High-level architecture
- Library: `src/` contains reusable pattern implementations. Key areas:
  - Builder pattern: `src/DesignPatterns/Builder/` (see `BuilderBase.cs`, `AsyncBuilderBase.cs`, `Director.cs`). Builders follow Reset/Validate/Build semantics and use a fluent-style generic base `BuilderBase<TBuilder,TProduct>`.
  - Command pattern: `src/DesignPatterns/Command/` (see `ICommand.cs`, `IUndoableCommand.cs`, `IAsyncCommand.cs`, `CommandQueue.cs`). `CommandQueue<T>` is thread-safe and supports sync/async commands plus undo/redo via an internal adapter.

Important conventions and patterns (codebase-specific)
- Builders:
  - Concrete builders inherit `BuilderBase<TBuilder,TProduct>` and must implement `ResetCore()`, `BuildCore()`, and `Validate()`.
  - `Build()` enforces a single-use until `Reset()` is called and throws `InvalidOperationException` on misuse. See `src/DesignPatterns/Builder/BuilderBase.cs`.
  - `Director<TBuilder,TProduct>` centralizes reusable construction flows and calls `Builder.Reset()` before constructing.

- Commands:
  - `ICommand.Execute()` is the synchronous contract. Async variants exist (`IAsyncCommand`, `IAsyncUndoableCommand`). See `src/DesignPatterns/Command/ICommand.cs`.
  - `CommandQueue<T>` is explicitly thread-safe (uses a private `_sync` lock) and uses an internal `UndoableAdapter` to store both sync and async undoable commands on the same stacks. See `src/DesignPatterns/Command/CommandQueue.cs` for behavior details (stack mutation only after successful execution).
  - For sync callers executing async commands, the library intentionally blocks (`GetAwaiter().GetResult()`) to provide sync APIs while preserving async implementations.

Build, test, and packaging
- Build: repository uses dotnet SDK; target frameworks `net8.0;netstandard2.0` are defined in `src/MBUtils.csproj`.
  - Recommended: `dotnet build`
- Tests: `dotnet test` runs all tests. The root README provides test filters for pattern-specific tests, e.g.:
  - Command tests: `dotnet test --filter "FullyQualifiedName~MBUtils.Tests.DesignPatterns.Command"`
  - Builder tests: `dotnet test --filter "FullyQualifiedName~MBUtils.Tests.DesignPatterns.Builder"`
- Packaging: package generation is enabled on build (`<GeneratePackageOnBuild>true</GeneratePackageOnBuild>` in the project file).

Repository conventions and style hints
- XML documentation is used liberally in public types. Keep public XML docs consistent with existing style.
- Always include XML documentation comments for public members to maintain code clarity and usability.
- Exceptions: misuse is signaled with `InvalidOperationException` and null arguments with `ArgumentNullException` — follow these patterns.
- Nullability: `<Nullable>enable</Nullable>` is set in the project. Prefer non-nullable APIs where appropriate and add `?` where null is expected.
- Async patterns: follow standard .NET async conventions (e.g., `Async` suffix for async methods, `CancellationToken` parameters, etc.).
- Never use var. Always use explicit types for variable declarations.
- Whenever adding new public APIs, ensure they are covered by unit tests in the corresponding test project.
- Follow the existing code style and conventions in the repository.
- Keep your changes focused and small; if you need to make large changes, consider breaking them into smaller, manageable tasks.
- Document your changes thoroughly, including any new public APIs, and update the relevant documentation files as needed.
- Do not use Premium Models for code generation or analysis. Stick to free models only.
- Implement interfaces explicitly; do not use implicit interface implementation.
- When working with collections, prefer using `IReadOnlyList<T>` or `IReadOnlyCollection<T>` for public APIs to ensure immutability.
- Ensure that all unit tests pass before submitting any changes.
- Remove all unused usings and references in your code files.
- When writing unit tests, use xUnit and follow the existing test patterns in the repository.
- Always remove any using directives that are not needed.
- The projects always have to be built without warnings or errors. Ensure that your changes do not introduce any new warnings or errors.

Usage of Git
- Write commit messages according to the following structure:
  - The first line should be a concise summary of the change (max 70 characters). It should start with the following prefixes based on the type of change:
    - `[ADD]` for additions
    - `[FIX]` for bug fixes
    - `[REF]` for refactoring or code improvements
    - `[DOC]` for documentation updates
    - `[DEL]` for deletions
    - `[CHG]` for changes that do not fit in the other categories
  - A blank line should separate the summary from the body.
  - The body of the commit message (if needed) should provide additional context and details about the change, wrapped in multiple lines of maximal 72 characters per line.
  - Use gitmoji where appropriate to enhance the commit message.
- Split changes into multiple commits, each with a clear purpose and following the commit message structure.
- Make commits in an order that logically builds up the change, so that each commit is a coherent step towards the final result.
- Use branches for feature development and bug fixes. Name branches descriptively, e.g., `feature/command-enhancement` or `bugfix/fix-null-reference`.
- Before pushing or creating a pull request, ensure that your branch is up to date with the remote tracking branch.
- Before pushing changes, always pull the latest changes from the remote tracking branch, if there is one, and resolve any merge conflicts locally.
- Follow the GitFlow workflow for managing branches and releases.