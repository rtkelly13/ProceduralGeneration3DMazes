# Maze Serialisation Specification

This document describes the `.maze` file format used for saving and loading mazes.

## Overview

The maze file format is a human-readable text format that stores the structure of a 3D maze. It consists of a header section defining maze metadata followed by cell data describing the passages between cells.

## File Extension

`.maze`

## Encoding

UTF-8

## Format Structure

```
SIZE X Y Z
START X Y Z
END X Y Z
CELLS
X Y Z DIRECTIONS
X Y Z DIRECTIONS
...
```

## Header Section

The header must contain the following declarations in any order before the `CELLS` marker:

### SIZE (required)

Defines the dimensions of the maze.

```
SIZE X Y Z
```

- `X` - Width (number of cells along X axis)
- `Y` - Depth (number of cells along Y axis)  
- `Z` - Height (number of cells along Z axis, use 1 for 2D mazes)

All values must be positive integers.

**Example:**
```
SIZE 20 20 1
```

### START (required)

Defines the starting point of the maze.

```
START X Y Z
```

Coordinates are zero-indexed. The start point must be within the maze bounds.

**Example:**
```
START 0 0 0
```

### END (required)

Defines the ending point of the maze.

```
END X Y Z
```

Coordinates are zero-indexed. The end point must be within the maze bounds.

**Example:**
```
END 19 19 0
```

### CELLS (required)

Marks the beginning of the cell data section. All lines after this marker are interpreted as cell definitions.

```
CELLS
```

## Cell Data Section

Each line after `CELLS` defines the connections (passages) for a single cell.

```
X Y Z DIRECTIONS
```

- `X Y Z` - The cell coordinates (zero-indexed)
- `DIRECTIONS` - The directions this cell connects to

### Direction Format

Directions can be specified in two formats:

#### Letter Format (recommended)

A string of direction characters with no separators:

| Character | Direction | Axis Movement |
|-----------|-----------|---------------|
| `L` | Left | X - 1 |
| `R` | Right | X + 1 |
| `B` | Back | Y - 1 |
| `F` | Forward | Y + 1 |
| `D` | Down | Z - 1 |
| `U` | Up | Z + 1 |
| `N` | None | No connection |

Characters are case-insensitive. Order does not matter.

**Examples:**
```
0 0 0 RF      # Connects right and forward
1 1 0 LRBF    # Connects left, right, back, and forward (4-way junction)
0 0 0 RFU     # Connects right, forward, and up (3D connection)
```

#### Numeric Format

A decimal number representing the direction flags as a bitmask:

| Direction | Flag Value |
|-----------|------------|
| Left | 1 |
| Right | 2 |
| Down | 4 |
| Up | 8 |
| Back | 16 |
| Forward | 32 |

The value is the sum of the individual direction flags.

**Examples:**
```
0 0 0 34      # RF: Right(2) + Forward(32) = 34
1 1 0 51      # LRBF: Left(1) + Right(2) + Back(16) + Forward(32) = 51
```

Valid numeric values range from 0 to 63.

### Omitted Cells

Cells with no connections (isolated cells) may be omitted from the file. Any cell not explicitly listed is assumed to have `Direction.None`.

## Comments

Lines beginning with `#` are treated as comments and ignored.

```
# This is a comment
SIZE 10 10 1
# Another comment
START 0 0 0
```

## Blank Lines

Blank lines are ignored and may appear anywhere in the file.

## Complete Example

```
# A simple 3x3 maze
SIZE 3 3 1
START 0 0 0
END 2 2 0

CELLS
# Bottom row
0 0 0 RF
1 0 0 LRF
2 0 0 LF

# Middle row  
0 1 0 BRF
1 1 0 LRBF
2 1 0 LBF

# Top row
0 2 0 BR
1 2 0 LRB
2 2 0 LB
```

## Validation Rules

When importing a maze, the following validations are performed:

### Required Fields
- `SIZE`, `START`, and `END` declarations must be present
- `CELLS` marker must be present (even if no cells follow)

### Boundary Checks
- Start and end points must be within maze bounds (0 to SIZE-1 for each axis)
- Cell coordinates must be within maze bounds
- Direction connections must not point outside maze bounds

### Bidirectional Consistency
- All connections must be bidirectional
- If cell A connects to cell B, cell B must connect back to cell A
- Example: If `(0,0,0)` has `R`, then `(1,0,0)` must have `L`

### Connectivity
- Start point should have at least one connection
- End point should have at least one connection

## Output Format

When serialising a maze, the following conventions are used:

1. Header declarations are written in order: `SIZE`, `START`, `END`, `CELLS`
2. Cells are written in Z, Y, X order (outermost to innermost loop)
3. Directions are written in letter format using the order: `L`, `R`, `D`, `U`, `B`, `F`
4. Cells with no connections are omitted
5. No comments are included in output

## Coordinate System

The coordinate system follows these conventions:

- **X axis**: Left (-X) to Right (+X)
- **Y axis**: Back (-Y) to Forward (+Y)
- **Z axis**: Down (-Z) to Up (+Z)

In Godot's 2D rendering:
- Positive Y goes down on screen
- Forward (Y+1) renders as going down
- Back (Y-1) renders as going up
