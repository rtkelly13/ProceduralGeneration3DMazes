using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.UI
{
    /// <summary>
    /// Result of an export operation.
    /// </summary>
    public class ExportResult
    {
        public bool Success { get; }
        public string? FilePath { get; }
        public string? ErrorMessage { get; }
        public string? TechnicalDetails { get; }

        private ExportResult(bool success, string? filePath, string? errorMessage, string? technicalDetails)
        {
            Success = success;
            FilePath = filePath;
            ErrorMessage = errorMessage;
            TechnicalDetails = technicalDetails;
        }

        public static ExportResult Succeeded(string filePath) =>
            new(true, filePath, null, null);

        public static ExportResult Failed(string errorMessage, string? technicalDetails = null) =>
            new(false, null, errorMessage, technicalDetails);
    }

    /// <summary>
    /// Result of an import operation.
    /// </summary>
    public class ImportResult
    {
        public bool Success { get; }
        public IModel? Model { get; }
        public string? ErrorMessage { get; }
        public string? TechnicalDetails { get; }

        private ImportResult(bool success, IModel? model, string? errorMessage, string? technicalDetails)
        {
            Success = success;
            Model = model;
            ErrorMessage = errorMessage;
            TechnicalDetails = technicalDetails;
        }

        public static ImportResult Succeeded(IModel model) =>
            new(true, model, null, null);

        public static ImportResult Failed(string errorMessage, string? technicalDetails = null) =>
            new(false, null, errorMessage, technicalDetails);
    }
}
