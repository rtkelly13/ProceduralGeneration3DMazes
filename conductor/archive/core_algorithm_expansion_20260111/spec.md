# Spec: Core Algorithm Expansion (A* & Prim's)

## Overview
This track focuses on expanding the core algorithmic capabilities of the Procedural Maze Generator by implementing the A* pathfinding algorithm with configurable heuristics and adding Prim's algorithm for maze generation. Both implementations must support 3D mazes and integrate seamlessly with the existing visualization and benchmarking systems.

## Goals
-   Implement the A* (A-Star) pathfinding algorithm.
-   Implement Prim's algorithm for maze generation.
-   Ensure both algorithms support 3D maze structures (X, Y, Z).
-   Integrate both algorithms into the existing `IMazeGenerationAlgorithm` and `IShortestPathSolver` frameworks.
-   Expose configurable heuristics for A*.

## Functional Requirements

### A* Pathfinding
-   **Implementation:** Create a new solver class `AStarSolver` implementing `IShortestPathSolver`.
-   **Heuristics:**
    -   Implement a strategy pattern or enum option for heuristics.
    -   Supported heuristics:
        -   **Manhattan Distance:** `|x1 - x2| + |y1 - y2| + |z1 - z2|`
        -   **Euclidean Distance:** `sqrt((x1 - x2)^2 + (y1 - y2)^2 + (z1 - z2)^2)`
-   **Configuration:** Add options to the UI/Settings to select the A* heuristic.
-   **Integration:** Ensure A* can be selected as the solver for any generated maze.

### Prim's Algorithm
-   **Implementation:** Create a new generation class `PrimsAlgorithm` implementing `IMazeGenerationAlgorithm`.
-   **3D Support:** The algorithm must treat the grid as a 3D graph, connecting cells across Z-levels if configured (e.g., stairs/ramps logic).
-   **Visualization:** Support step-by-step metric collection (Metrics, Heatmap) consistent with Phase 3 deliverables.

## Technical Constraints
-   **Performance:** Algorithms should be performant enough for real-time visualization on reasonable grid sizes (e.g., 50x50x5).
-   **Code Structure:** Adhere to the existing dependency injection pattern (`ServiceContainer`) and interface contracts.
-   **Test Coverage:** All new algorithms must be covered by unit tests.

## Out of Scope
-   Bidirectional Search (deferred to a future track).
-   Weighted edges for generation (mazes remain unweighted for now, though A* supports weights).
