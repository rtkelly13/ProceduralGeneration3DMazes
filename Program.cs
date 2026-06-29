// WebAssembly entry point.
//
// The experimental .NET web export requires the assembly to be an executable with a
// managed entry point. Godot drives the actual game loop through the engine runtime,
// so this top-level statement is intentionally a no-op — it exists only to satisfy the
// `OutputType=Exe` used for web builds (see ProceduralGeneration3DMazes.csproj and
// docs/WEB_EXPORT.md). It is excluded from desktop/library builds.
{ }
