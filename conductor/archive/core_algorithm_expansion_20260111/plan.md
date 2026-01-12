# Plan: Core Algorithm Expansion (A* & Prim's)

## Phase 1: A* Pathfinding Implementation [checkpoint: cc05ca9]

- [x] Task: Create `IHeuristicStrategy` interface and implementations (Manhattan, Euclidean) [f300fae]
- [x] Task: Implement `AStarSolver` class implementing `IShortestPathSolver`
- [x] Task: Write Unit Tests for A* Solver (2D and 3D cases)
- [x] Task: Integrate A* Solver into `ServiceContainer` and `MazeGenerationSettings`
- [x] Task: Add UI controls for Solver Selection and Heuristic Configuration
- [x] Task: Conductor - User Manual Verification 'A* Pathfinding Implementation' (Protocol in workflow.md)

## Phase 2: Prim's Algorithm Implementation [checkpoint: 2169759]

- [x] Task: Implement `PrimsAlgorithm` class implementing `IMazeGenerationAlgorithm`
- [x] Task: Implement 3D neighbor selection logic for Prim's
- [x] Task: Write Unit Tests for Prim's Algorithm (Connectivity, 3D support)
- [x] Task: Integrate Prim's Algorithm into `ServiceContainer` and `MazeGenerationFactory`
- [x] Task: Add Prim's Algorithm to UI Algorithm Selection Dropdown
- [x] Task: Ensure Metric Collection and Heatmap support for Prim's
- [x] Task: Conductor - User Manual Verification 'Prim's Algorithm Implementation' (Protocol in workflow.md)
