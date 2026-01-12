# Spec: Algorithm Comparison Dashboard

## Overview
Implement a side-by-side comparison view in the Godot application to evaluate two different maze generation algorithms simultaneously.

## Goals
- Allow users to select two algorithms for comparison.
- Visualize both generation processes in real-time.
- Display live metrics for each algorithm (dead-ends, branching factor, etc.).
- Provide a "heatmap" overlay showing carving frequency or algorithm density.

## Functional Requirements
- **Selection UI:** Dropdowns or toggles to select Algorithm A and Algorithm B.
- **Dual Viewports:** Two side-by-side viewports rendering independent mazes.
- **Sync Controls:** Buttons to start/pause/reset both algorithms simultaneously.
- **Metric Panels:** Real-time data displays for each viewport.
- **Heatmap Toggle:** An option to overlay a heatmap on each maze.

## Technical Constraints
- Must integrate with existing Godot 4.x / C# architecture.
- Performance must remain stable with two concurrent generation processes.
- Reuse existing `IMazeGenerationAlgorithm` implementations.
