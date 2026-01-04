using System;
using System.IO;
using Godot;
using ProceduralMaze.Maze.Factory;
using ProceduralMaze.Maze.Model;
using ProceduralMaze.Maze.Serialization;

namespace ProceduralMaze.UI
{
    /// <summary>
    /// Cross-platform utility for importing and exporting maze files.
    /// Handles path resolution, file I/O, and error handling.
    /// </summary>
    public static class MazeImportExport
    {
        public const string FileExtension = ".maze";
        public const string StatsFileExtension = ".stats.json";
        public const string FileFilter = "*.maze";
        public const string DefaultDirectoryName = "ProceduralMazes";

        #region Platform Detection

        /// <summary>
        /// Returns true if running on a web platform where file dialogs are not available.
        /// </summary>
        public static bool IsWebPlatform()
        {
            var platform = OS.GetName();
            return platform == "Web";
        }

        /// <summary>
        /// Returns true if running on macOS.
        /// </summary>
        public static bool IsMacOS()
        {
            return OS.GetName() == "macOS";
        }

        #endregion

        #region Path Resolution

        /// <summary>
        /// Gets the default export directory, creating it if it doesn't exist.
        /// Returns ~/Documents/ProceduralMazes on desktop platforms.
        /// Falls back to user data directory if Documents is unavailable.
        /// </summary>
        public static string GetDefaultExportDirectory()
        {
            string baseDir;

            try
            {
                // Try to get the Documents folder
                baseDir = OS.GetSystemDir(OS.SystemDir.Documents);

                if (string.IsNullOrEmpty(baseDir) || !Directory.Exists(baseDir))
                {
                    // Fall back to user data directory (always available)
                    baseDir = OS.GetUserDataDir();
                }
            }
            catch
            {
                // If anything goes wrong, use user data directory
                baseDir = OS.GetUserDataDir();
            }

            var exportDir = Path.Combine(baseDir, DefaultDirectoryName);

            // Ensure directory exists
            try
            {
                if (!Directory.Exists(exportDir))
                {
                    Directory.CreateDirectory(exportDir);
                }
            }
            catch
            {
                // If we can't create the subdirectory, use the base directory
                return baseDir;
            }

            return exportDir;
        }

        /// <summary>
        /// Generates a default filename for the maze based on its size and current timestamp.
        /// Format: maze_20x20x1_2026-01-05_143052.maze
        /// </summary>
        public static string GenerateDefaultFilename(MazeSize size)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
            return $"maze_{size.X}x{size.Y}x{size.Z}_{timestamp}{FileExtension}";
        }

        /// <summary>
        /// Generates the full default path for exporting a maze.
        /// </summary>
        public static string GetDefaultExportPath(MazeSize size)
        {
            return Path.Combine(GetDefaultExportDirectory(), GenerateDefaultFilename(size));
        }

        #endregion

        #region Export Operations

        /// <summary>
        /// Exports a maze to the default location with auto-generated filename.
        /// </summary>
        public static ExportResult QuickExport(IModel model, IMazeSerializer serializer)
        {
            if (IsWebPlatform())
            {
                return ExportResult.Failed("Export is not available on web platform");
            }

            var path = GetDefaultExportPath(model.Size);
            return ExportTo(model, path, serializer);
        }

        /// <summary>
        /// Exports a maze to the specified path.
        /// </summary>
        public static ExportResult ExportTo(IModel model, string path, IMazeSerializer serializer)
        {
            if (IsWebPlatform())
            {
                return ExportResult.Failed("Export is not available on web platform");
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                return ExportResult.Failed("No file path specified");
            }

            // Ensure file has correct extension
            if (!path.EndsWith(FileExtension, StringComparison.OrdinalIgnoreCase))
            {
                path += FileExtension;
            }

            try
            {
                // Ensure parent directory exists
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Export the maze
                using var stream = new FileStream(path, FileMode.Create, System.IO.FileAccess.Write);
                serializer.Serialize(model, stream);

                return ExportResult.Succeeded(path);
            }
            catch (UnauthorizedAccessException ex)
            {
                return ExportResult.Failed(
                    "Cannot save to this location. Permission denied.",
                    $"Path: {path}\nError: {ex.Message}");
            }
            catch (DirectoryNotFoundException ex)
            {
                return ExportResult.Failed(
                    "The folder does not exist.",
                    $"Path: {path}\nError: {ex.Message}");
            }
            catch (PathTooLongException ex)
            {
                return ExportResult.Failed(
                    "The file path is too long.",
                    $"Path length: {path.Length}\nError: {ex.Message}");
            }
            catch (IOException ex)
            {
                // Check for common I/O issues
                if (ex.Message.Contains("disk full", StringComparison.OrdinalIgnoreCase) ||
                    ex.Message.Contains("no space", StringComparison.OrdinalIgnoreCase))
                {
                    return ExportResult.Failed(
                        "Not enough disk space.",
                        ex.Message);
                }

                return ExportResult.Failed(
                    "Failed to save the file.",
                    $"Path: {path}\nError: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ExportResult.Failed(
                    "An unexpected error occurred while saving.",
                    $"Path: {path}\nError: {ex.GetType().Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Exports maze statistics to a companion .stats.maze file.
        /// </summary>
        /// <param name="results">The maze generation results containing statistics.</param>
        /// <param name="mazePath">The path to the maze file (stats will be saved alongside it).</param>
        /// <param name="statsSerializer">The stats serializer to use.</param>
        public static ExportResult ExportStats(MazeGenerationResults results, string mazePath, IMazeStatsSerializer statsSerializer)
        {
            if (IsWebPlatform())
            {
                return ExportResult.Failed("Export is not available on web platform");
            }

            if (string.IsNullOrWhiteSpace(mazePath))
            {
                return ExportResult.Failed("No file path specified");
            }

            // Generate the stats file path by replacing or appending the extension
            var statsPath = GetStatsPath(mazePath);

            try
            {
                // Ensure parent directory exists
                var directory = Path.GetDirectoryName(statsPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Export the stats
                using var stream = new FileStream(statsPath, FileMode.Create, System.IO.FileAccess.Write);
                statsSerializer.Serialize(results, stream);

                return ExportResult.Succeeded(statsPath);
            }
            catch (UnauthorizedAccessException ex)
            {
                return ExportResult.Failed(
                    "Cannot save stats to this location. Permission denied.",
                    $"Path: {statsPath}\nError: {ex.Message}");
            }
            catch (DirectoryNotFoundException ex)
            {
                return ExportResult.Failed(
                    "The folder does not exist.",
                    $"Path: {statsPath}\nError: {ex.Message}");
            }
            catch (PathTooLongException ex)
            {
                return ExportResult.Failed(
                    "The file path is too long.",
                    $"Path length: {statsPath.Length}\nError: {ex.Message}");
            }
            catch (IOException ex)
            {
                if (ex.Message.Contains("disk full", StringComparison.OrdinalIgnoreCase) ||
                    ex.Message.Contains("no space", StringComparison.OrdinalIgnoreCase))
                {
                    return ExportResult.Failed(
                        "Not enough disk space.",
                        ex.Message);
                }

                return ExportResult.Failed(
                    "Failed to save the stats file.",
                    $"Path: {statsPath}\nError: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ExportResult.Failed(
                    "An unexpected error occurred while saving stats.",
                    $"Path: {statsPath}\nError: {ex.GetType().Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the stats file path for a given maze file path.
        /// Example: maze_20x20x1.maze -> maze_20x20x1.stats.maze
        /// </summary>
        public static string GetStatsPath(string mazePath)
        {
            if (mazePath.EndsWith(FileExtension, StringComparison.OrdinalIgnoreCase))
            {
                return mazePath[..^FileExtension.Length] + StatsFileExtension;
            }
            return mazePath + StatsFileExtension;
        }

        #endregion

        #region Import Operations

        /// <summary>
        /// Imports a maze from the specified path.
        /// </summary>
        public static ImportResult Import(string path, IMazeDeserializer deserializer, IMazeValidator validator)
        {
            if (IsWebPlatform())
            {
                return ImportResult.Failed("Import is not available on web platform");
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                return ImportResult.Failed("No file path specified");
            }

            try
            {
                if (!File.Exists(path))
                {
                    return ImportResult.Failed(
                        "File not found.",
                        $"Path: {path}");
                }

                // Read and parse the file
                using var stream = new FileStream(path, FileMode.Open, System.IO.FileAccess.Read);
                var model = deserializer.Deserialize(stream);

                // Validate the maze
                var validationResult = validator.Validate(model);
                if (!validationResult.IsValid)
                {
                    var errorSummary = string.Join("; ", validationResult.Errors);
                    return ImportResult.Failed(
                        "The maze file is invalid or corrupted.",
                        $"Validation errors: {errorSummary}");
                }

                return ImportResult.Succeeded(model);
            }
            catch (UnauthorizedAccessException ex)
            {
                return ImportResult.Failed(
                    "Cannot read this file. Permission denied.",
                    $"Path: {path}\nError: {ex.Message}");
            }
            catch (FormatException ex)
            {
                return ImportResult.Failed(
                    "Invalid maze file format.",
                    $"Parse error: {ex.Message}");
            }
            catch (IOException ex)
            {
                return ImportResult.Failed(
                    "Failed to read the file.",
                    $"Path: {path}\nError: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ImportResult.Failed(
                    "An unexpected error occurred while loading.",
                    $"Path: {path}\nError: {ex.GetType().Name}: {ex.Message}");
            }
        }

        #endregion
    }
}
