# Plan: Core Algorithm Expansion (A* & Prim's)

## Phase 1: A* Pathfinding Implementation

- [ ] Task: Create `IHeuristicStrategy` interface and implementations (Manhattan, Euclidean)
- [ ] Task: Implement `AStarSolver` class implementing `IShortestPathSolver`
- [ ] Task: Write Unit Tests for A* Solver (2D and 3D cases)
- [ ] Task: Integrate A* Solver into `ServiceContainer` and `MazeGenerationSettings`
- [ ] Task: Add UI controls for Solver Selection and Heuristic Configuration
- [ ] Task: Conductor - User Manual Verification 'A* Pathfinding Implementation' (Protocol in workflow.md)

## Phase 2: Prim's Algorithm Implementation

- [ ] Task: Implement `PrimsAlgorithm` class implementing `IMazeGenerationAlgorithm`
- [ ] Task: Implement 3D neighbor selection logic for Prim's
- [ ] Task: Write Unit Tests for Prim's Algorithm (Connectivity, 3D support)
- [ ] Task: Integrate Prim's Algorithm into `ServiceContainer` and `MazeGenerationFactory`
- [ ] Task: Add Prim's Algorithm to UI Algorithm Selection Dropdown
- [ ] Task: Ensure Metric Collection and Heatmap support for Prim's
- [ ] Task: Conductor - User Manual Verification 'Prim's Algorithm Implementation' (Protocol in workflow.md)
