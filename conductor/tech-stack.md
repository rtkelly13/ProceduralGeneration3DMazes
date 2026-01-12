# Technology Stack

This project leverages the Godot game engine and the .NET ecosystem to create a high-performance, cross-platform maze generation and visualization tool.

## Core Engine & Runtime
- **Engine:** [Godot 4.5.1](https://godotengine.org/)
    - Utilizes the Godot .NET SDK for high-level engine features and rendering.
- **Runtime:** [.NET 8.0](https://dotnet.microsoft.com/download)
    - Leverages modern C# features and the performance of the CoreCLR.

## Programming Language
- **Primary Language:** **C# 12**
    - Focused on clean, maintainable, and highly performant code.
    - Uses modern patterns like Dependency Injection (via `ServiceContainer.cs`).

## Project Structure & Dependencies
- **Primary Project:** `ProceduralGeneration3DMazes.csproj`
    - Main Godot project containing core maze logic and UI.
- **Testing:** `tests/ProceduralMaze.Tests.csproj`
    - **Framework:** NUnit 3.14.0
    - **Mocking:** Moq 4.20.72
    - **Code Coverage:** Coverlet (via `coverlet.msbuild`)
- **Benchmarking:** `benchmarks/ProceduralMaze.Benchmarks.csproj`
    - Focuses on performance analysis of generation and pathfinding algorithms.
- **Experiments:** `experiments/ProceduralMaze.Experiments.csproj`
    - A sandbox for testing algorithm variations and specific scenarios.

## Development & Build Tools
- **Build System:** `dotnet` CLI / MSBuild
- **Static Analysis:** NUnit.Analyzers for test code quality.
- **Nullable Reference Types:** Enabled project-wide for enhanced type safety.
