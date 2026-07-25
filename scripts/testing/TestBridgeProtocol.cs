using System;
using System.Globalization;
using System.Text;
using ProceduralMaze.Maze;

namespace ProceduralMaze.Testing
{
    /// <summary>
    /// The wire format for the test bridge: query-string and command parsing, and JSON
    /// writing. Deliberately free of any Godot dependency.
    /// </summary>
    /// <remarks>
    /// Split out from <c>TestBridge</c> so it can be unit-tested by the ordinary NUnit
    /// project, which compiles without the Godot SDK. The node itself can only be exercised
    /// inside the engine; this is where the fiddly logic lives, so this is the part worth
    /// covering with fast tests.
    ///
    /// Parsers are hand-rolled rather than using System.Text.Json on purpose: the web build
    /// is trimmed (see the csproj TrimmerRootAssembly entries), and reflection-based
    /// serialization is exactly what trimming breaks — a failure that would appear only in
    /// the browser. The command surface is a handful of scalar fields, so the trade is cheap.
    ///
    /// Every numeric conversion pins InvariantCulture because the web build forces
    /// InvariantGlobalization; culture-dependent parsing would behave differently on desktop.
    /// </remarks>
    public static class TestBridgeProtocol
    {
        /// <summary>Reads a parameter from a `?a=1&amp;b=2` query string. Null when absent.</summary>
        public static string? GetParam(string? query, string name)
        {
            if (string.IsNullOrEmpty(query))
            {
                return null;
            }

            foreach (var pair in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var eq = pair.IndexOf('=');
                if (eq <= 0)
                {
                    continue;
                }

                if (pair.AsSpan(0, eq).SequenceEqual(name))
                {
                    return Uri.UnescapeDataString(pair[(eq + 1)..]);
                }
            }

            return null;
        }

        /// <summary>Reads an integer parameter. Null when absent or not an integer.</summary>
        public static int? GetIntParam(string? query, string name) =>
            int.TryParse(GetParam(query, name), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)
                ? v
                : null;

        /// <summary>Extracts `"name":"value"` from a flat JSON object.</summary>
        public static string? GetJsonString(string? json, string name)
        {
            var i = FieldValueStart(json, name);
            if (i < 0 || json![i] != '"')
            {
                return null;
            }

            var sb = new StringBuilder();
            for (var k = i + 1; k < json.Length; k++)
            {
                var c = json[k];
                if (c == '\\' && k + 1 < json.Length)
                {
                    k++;
                    sb.Append(json[k] switch
                    {
                        'n' => '\n',
                        'r' => '\r',
                        't' => '\t',
                        var other => other,
                    });
                    continue;
                }

                if (c == '"')
                {
                    return sb.ToString();
                }

                sb.Append(c);
            }

            return null; // unterminated string
        }

        /// <summary>Extracts `"name":123` from a flat JSON object.</summary>
        public static int? GetJsonInt(string? json, string name)
        {
            var i = FieldValueStart(json, name);
            if (i < 0)
            {
                return null;
            }

            var end = i;
            if (end < json!.Length && (json[end] == '-' || json[end] == '+'))
            {
                end++;
            }

            while (end < json.Length && char.IsDigit(json[end]))
            {
                end++;
            }

            return int.TryParse(json.AsSpan(i, end - i), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)
                ? v
                : null;
        }

        /// <summary>Extracts `"name":true|false` from a flat JSON object.</summary>
        public static bool? GetJsonBool(string? json, string name)
        {
            var i = FieldValueStart(json, name);
            if (i < 0)
            {
                return null;
            }

            if (string.CompareOrdinal(json, i, "true", 0, 4) == 0)
            {
                return true;
            }

            return string.CompareOrdinal(json, i, "false", 0, 5) == 0 ? false : null;
        }

        /// <summary>Index of the first character of the value for `"name":`, or -1 if absent.</summary>
        private static int FieldValueStart(string? json, string name)
        {
            if (string.IsNullOrEmpty(json))
            {
                return -1;
            }

            var key = "\"" + name + "\"";
            var k = json.IndexOf(key, StringComparison.Ordinal);
            if (k < 0)
            {
                return -1;
            }

            var i = k + key.Length;
            while (i < json.Length && (json[i] == ' ' || json[i] == ':'))
            {
                i++;
            }

            return i >= json.Length ? -1 : i;
        }

        /// <summary>
        /// Maps the URL/command spelling of an algorithm to the enum. Null when unrecognised,
        /// so callers can leave the current setting alone rather than guessing.
        /// </summary>
        public static Algorithm? ParseAlgorithm(string? name) => name?.ToLowerInvariant() switch
        {
            "backtracker" or "recursivebacktracker" => Algorithm.RecursiveBacktrackerAlgorithm,
            "growingtree" => Algorithm.GrowingTreeAlgorithm,
            "binarytree" => Algorithm.BinaryTreeAlgorithm,
            "prims" => Algorithm.PrimsAlgorithm,
            _ => null,
        };

        /// <summary>Maps a scene alias to its resource path. Null when unrecognised.</summary>
        public static string? ResolveScenePath(string? scene) => scene switch
        {
            "maze" => "res://scenes/maze.tscn",
            "menu" => "res://scenes/menu.tscn",
            "comparison" => "res://scenes/comparison_dashboard.tscn",
            "loader" => "res://scenes/maze_loader.tscn",
            _ => null,
        };

        #region JSON writing

        public static void AppendInt(StringBuilder sb, string name, int value) =>
            sb.Append('"').Append(name).Append("\":").Append(value.ToString(CultureInfo.InvariantCulture));

        public static void AppendBool(StringBuilder sb, string name, bool value) =>
            sb.Append('"').Append(name).Append("\":").Append(value ? "true" : "false");

        /// <summary>Writes `"name":"value"` with JSON string escaping.</summary>
        public static void AppendString(StringBuilder sb, string name, string? value)
        {
            sb.Append('"').Append(name).Append("\":\"");
            foreach (var c in value ?? "")
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < ' ')
                        {
                            sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            sb.Append(c);
                        }

                        break;
                }
            }

            sb.Append('"');
        }

        #endregion
    }
}
