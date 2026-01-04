using NUnit.Framework;

// Enable parallel test execution at the assembly level
// ParallelScope.All runs test fixtures and their children in parallel
[assembly: Parallelizable(ParallelScope.All)]
