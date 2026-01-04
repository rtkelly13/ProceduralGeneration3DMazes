using System;
using System.Collections.Generic;
using System.IO;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Serialization
{
    /// <summary>
    /// Deserializes maze models from a text-based format.
    /// </summary>
    public class MazeDeserializer : IMazeDeserializer
    {
        public ReadOnlyMazeModel Deserialize(Stream input)
        {
            using var reader = new StreamReader(input, leaveOpen: true);
            var content = reader.ReadToEnd();
            return Deserialize(content.AsSpan());
        }

        public ReadOnlyMazeModel DeserializeFromString(string content)
        {
            return Deserialize(content.AsSpan());
        }

        public ReadOnlyMazeModel Deserialize(ReadOnlySpan<char> content)
        {
            MazeSize? size = null;
            MazePoint? startPoint = null;
            MazePoint? endPoint = null;
            var cells = new Dictionary<MazePoint, Direction>();
            bool inCells = false;
            int lineNumber = 0;

            foreach (var line in new LineEnumerator(content))
            {
                lineNumber++;
                var trimmed = line.Trim();

                // Skip empty lines and comments
                if (trimmed.IsEmpty || trimmed[0] == '#')
                    continue;

                if (TryParseKeyword(trimmed, "SIZE", out var rest))
                {
                    size = ParseSize(rest, lineNumber);
                }
                else if (TryParseKeyword(trimmed, "START", out rest))
                {
                    startPoint = ParsePoint(rest, lineNumber);
                }
                else if (TryParseKeyword(trimmed, "END", out rest))
                {
                    endPoint = ParsePoint(rest, lineNumber);
                }
                else if (trimmed.Equals("CELLS", StringComparison.OrdinalIgnoreCase))
                {
                    inCells = true;
                }
                else if (inCells)
                {
                    var (point, directions) = ParseCell(trimmed, lineNumber);
                    cells[point] = directions;
                }
                else
                {
                    throw new FormatException($"Line {lineNumber}: Unexpected content before CELLS section: '{trimmed.ToString()}'");
                }
            }

            // Validate required fields
            if (size == null)
                throw new FormatException("Missing required SIZE declaration");
            if (startPoint == null)
                throw new FormatException("Missing required START declaration");
            if (endPoint == null)
                throw new FormatException("Missing required END declaration");

            return new ReadOnlyMazeModel(size, startPoint, endPoint, cells);
        }

        private static bool TryParseKeyword(ReadOnlySpan<char> line, string keyword, out ReadOnlySpan<char> rest)
        {
            if (line.StartsWith(keyword.AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                var afterKeyword = line.Slice(keyword.Length);
                if (afterKeyword.IsEmpty || char.IsWhiteSpace(afterKeyword[0]))
                {
                    rest = afterKeyword.Trim();
                    return true;
                }
            }
            rest = default;
            return false;
        }

        private static MazeSize ParseSize(ReadOnlySpan<char> span, int lineNumber)
        {
            Span<int> values = stackalloc int[3];
            if (!TryParseThreeInts(span, values))
                throw new FormatException($"Line {lineNumber}: Invalid SIZE format. Expected: SIZE X Y Z");

            return new MazeSize { X = values[0], Y = values[1], Z = values[2] };
        }

        private static MazePoint ParsePoint(ReadOnlySpan<char> span, int lineNumber)
        {
            Span<int> values = stackalloc int[3];
            if (!TryParseThreeInts(span, values))
                throw new FormatException($"Line {lineNumber}: Invalid point format. Expected: X Y Z");

            return new MazePoint(values[0], values[1], values[2]);
        }

        private static (MazePoint point, Direction directions) ParseCell(ReadOnlySpan<char> span, int lineNumber)
        {
            // Format: X Y Z Directions
            Span<int> coords = stackalloc int[3];
            int consumed = 0;
            int coordIndex = 0;

            // Parse three integers
            while (coordIndex < 3 && consumed < span.Length)
            {
                // Skip whitespace
                while (consumed < span.Length && char.IsWhiteSpace(span[consumed]))
                    consumed++;

                if (consumed >= span.Length)
                    break;

                // Find end of number
                int start = consumed;
                while (consumed < span.Length && (char.IsDigit(span[consumed]) || span[consumed] == '-'))
                    consumed++;

                if (start == consumed)
                    throw new FormatException($"Line {lineNumber}: Expected coordinate value");

                if (!int.TryParse(span.Slice(start, consumed - start), out coords[coordIndex]))
                    throw new FormatException($"Line {lineNumber}: Invalid coordinate value");

                coordIndex++;
            }

            if (coordIndex < 3)
                throw new FormatException($"Line {lineNumber}: Invalid cell format. Expected: X Y Z Directions");

            // Skip whitespace to get to directions
            while (consumed < span.Length && char.IsWhiteSpace(span[consumed]))
                consumed++;

            if (consumed >= span.Length)
                throw new FormatException($"Line {lineNumber}: Missing direction value");

            var directionsSpan = span.Slice(consumed).Trim();
            var directions = ParseDirections(directionsSpan, lineNumber);

            return (new MazePoint(coords[0], coords[1], coords[2]), directions);
        }

        private static Direction ParseDirections(ReadOnlySpan<char> span, int lineNumber)
        {
            // Try numeric first
            if (int.TryParse(span, out int numeric))
            {
                if (numeric < 0 || numeric > 63)
                    throw new FormatException($"Line {lineNumber}: Direction value must be between 0 and 63");
                return (Direction)numeric;
            }

            // Parse letters
            Direction result = Direction.None;
            foreach (char c in span)
            {
                Direction dir = char.ToUpperInvariant(c) switch
                {
                    'L' => Direction.Left,
                    'R' => Direction.Right,
                    'D' => Direction.Down,
                    'U' => Direction.Up,
                    'B' => Direction.Back,
                    'F' => Direction.Forward,
                    'N' => Direction.None,
                    _ => throw new FormatException($"Line {lineNumber}: Invalid direction character: '{c}'")
                };
                result |= dir;
            }
            return result;
        }

        private static bool TryParseThreeInts(ReadOnlySpan<char> span, Span<int> values)
        {
            int index = 0;
            int valueIndex = 0;

            while (valueIndex < 3 && index < span.Length)
            {
                // Skip whitespace
                while (index < span.Length && char.IsWhiteSpace(span[index]))
                    index++;

                if (index >= span.Length)
                    break;

                // Find end of number
                int start = index;
                while (index < span.Length && (char.IsDigit(span[index]) || span[index] == '-'))
                    index++;

                if (start == index)
                    return false;

                if (!int.TryParse(span.Slice(start, index - start), out values[valueIndex]))
                    return false;

                valueIndex++;
            }

            return valueIndex == 3;
        }

        /// <summary>
        /// Simple line enumerator that works with ReadOnlySpan without allocations.
        /// </summary>
        private ref struct LineEnumerator
        {
            private ReadOnlySpan<char> _remaining;
            private ReadOnlySpan<char> _current;
            private bool _started;

            public LineEnumerator(ReadOnlySpan<char> content)
            {
                _remaining = content;
                _current = default;
                _started = false;
            }

            public readonly LineEnumerator GetEnumerator() => this;

            public readonly ReadOnlySpan<char> Current => _current;

            public bool MoveNext()
            {
                if (!_started)
                {
                    _started = true;
                }

                if (_remaining.IsEmpty)
                    return false;

                int newlineIndex = _remaining.IndexOfAny('\r', '\n');
                if (newlineIndex == -1)
                {
                    _current = _remaining;
                    _remaining = default;
                    return true;
                }

                _current = _remaining.Slice(0, newlineIndex);
                
                // Handle \r\n, \r, or \n
                if (newlineIndex + 1 < _remaining.Length && 
                    _remaining[newlineIndex] == '\r' && 
                    _remaining[newlineIndex + 1] == '\n')
                {
                    _remaining = _remaining.Slice(newlineIndex + 2);
                }
                else
                {
                    _remaining = _remaining.Slice(newlineIndex + 1);
                }

                return true;
            }
        }
    }
}
