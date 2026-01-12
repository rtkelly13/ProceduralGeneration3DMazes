# Product Guidelines

These guidelines define the visual, interactive, and communication style of the Procedural 3D Maze Generator to ensure a consistent and effective educational experience.

## Visual Identity
- **Aesthetic:** Minimalist and Schematic.
    - Focus on clear lines, distinct symbols, and high-contrast color palettes.
    - Prioritize readability of the algorithm's state (e.g., current cell, frontier, path) over decorative elements.
    - Use color coding consistently across all visualizations to represent specific algorithm behaviors or cell states.

## User Interface & Information Architecture
- **Information Layering:**
    - **Overlay HUD:** Use for high-level, real-time statistics that need to be visible at a glance (e.g., current generation step, elapsed time, global maze stats).
    - **Sidebars:** Use for persistent configuration controls, detailed algorithm descriptions, and deep-dive metrics of selected nodes or the entire maze.
    - **In-World Tooltips:** Provide context-sensitive data (e.g., cell coordinates, specific weights, node-specific solver data) that appears only when the user hovers over a maze element.
- **Clarity:** Ensure that UI elements do not obscure the maze visualization unless explicitly requested by the user.

## Navigation & Camera
- **Control Schemes:**
    - **Orbit/Turntable:** The primary mode for overviewing the entire 3D structure.
    - **Top-Down/RTS:** A secondary mode for tracking the algorithm's progress across large or complex 2D slices of the maze.
- **Transitions:** Use smooth, predictable camera movements when switching between views or focusing on specific maze areas.

## Tone & Messaging
- **Keyword:** **Accessible & Clear.**
- **Voice:** Helpful, educational, and straightforward. Avoid unnecessary jargon where possible, or provide clear definitions via tooltips/sidebars.
- **Focus:** Empower the user to understand complex procedural concepts through intuitive interactions and clear visual feedback.
- **Robustness:** While accessible, the tool should feel reliable and capable of handling complex configurations without performance degradation.
