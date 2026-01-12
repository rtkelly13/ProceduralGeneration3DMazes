using NUnit.Framework;
using System;
using System.IO;

namespace ProceduralMaze.Tests;

[TestFixture]
public class ComparisonDashboardTests
{
    private string GetProjectRoot()
    {
        // Start from the test directory and go up until we find the solution file or a specific marker
        var dir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "ProceduralGeneration3DMazes.sln")))
        {
            dir = dir.Parent;
        }
        return dir?.FullName ?? throw new DirectoryNotFoundException("Could not find project root");
    }

    [Test]
    public void ComparisonDashboardScene_FileExists()
    {
        // Arrange
        var projectRoot = GetProjectRoot();
        var scenePath = Path.Combine(projectRoot, "scenes", "comparison_dashboard.tscn");
        
        // Act & Assert
        Assert.That(File.Exists(scenePath), Is.True, 
            $"comparison_dashboard.tscn should exist at {scenePath}");
    }

    [Test]
    public void ComparisonController_ScriptFileExists()
    {
        // Arrange
        var projectRoot = GetProjectRoot();
        var scriptPath = Path.Combine(projectRoot, "scripts", "ui", "ComparisonController.cs");

        // Act & Assert
        Assert.That(File.Exists(scriptPath), Is.True, 
            $"ComparisonController.cs should exist at {scriptPath}");
    }

    [Test]
    public void ComparisonDashboardScene_HasDualViewportLayout()
    {
        // Arrange
        var projectRoot = GetProjectRoot();
        var scenePath = Path.Combine(projectRoot, "scenes", "comparison_dashboard.tscn");
        
        // Act
        var content = File.ReadAllText(scenePath);

        // Assert
        Assert.That(content, Does.Contain("SubViewportContainer"), "Should contain SubViewportContainer");
        Assert.That(content, Does.Contain("LeftViewport"), "Should contain LeftViewport");
        Assert.That(content, Does.Contain("RightViewport"), "Should contain RightViewport");
    }

    [Test]
    public void ComparisonDashboardScene_HasControlUI()
    {
        // Arrange
        var projectRoot = GetProjectRoot();
        var scenePath = Path.Combine(projectRoot, "scenes", "comparison_dashboard.tscn");
        
        // Act
        var content = File.ReadAllText(scenePath);

        // Assert
        // Check for Algorithm Selection Dropdowns (OptionButtons)
        Assert.That(content, Does.Contain("AlgorithmAOption"), "Should contain Algorithm A selection option");
        Assert.That(content, Does.Contain("AlgorithmBOption"), "Should contain Algorithm B selection option");
        
        // Check for Controls
        Assert.That(content, Does.Contain("StartButton"), "Should contain Start button");
        Assert.That(content, Does.Contain("ResetButton"), "Should contain Reset button");
        Assert.That(content, Does.Contain("BackButton"), "Should contain Back button");
    }

    [Test]
    public void ComparisonController_HasRequiredMethods()
    {
        // Arrange
        var projectRoot = GetProjectRoot();
        var scriptPath = Path.Combine(projectRoot, "scripts", "ui", "ComparisonController.cs");
        
        // Act
        var content = File.ReadAllText(scriptPath);

        // Assert
        Assert.That(content, Does.Contain("void StartComparison"), "Should have StartComparison method");
        Assert.That(content, Does.Contain("void Reset"), "Should have Reset method");
    }

    [Test]
    public void ComparisonDashboardScene_HasMetricPanels()
    {
        // Arrange
        var projectRoot = GetProjectRoot();
        var scenePath = Path.Combine(projectRoot, "scenes", "comparison_dashboard.tscn");
        
        // Act
        var content = File.ReadAllText(scenePath);

        // Assert
        Assert.That(content, Does.Contain("MetricsALabel"), "Should contain Metrics A label");
        Assert.That(content, Does.Contain("MetricsBLabel"), "Should contain Metrics B label");
    }

    [Test]
    public void ComparisonController_HasSyncControlMethods()
    {
        // Arrange
        var projectRoot = GetProjectRoot();
        var scriptPath = Path.Combine(projectRoot, "scripts(ui/ComparisonController.cs");
        
        // Fix typo in path for robust lookup if needed, but projectRoot logic is better
        scriptPath = Path.Combine(projectRoot, "scripts", "ui", "ComparisonController.cs");
        var content = File.ReadAllText(scriptPath);

        // Assert
        Assert.That(content, Does.Contain("void TogglePlayPause"), "Should have TogglePlayPause method");
        Assert.That(content, Does.Contain("void StepForward"), "Should have StepForward method");
    }

    [Test]
    public void ComparisonDashboardScene_HasHeatmapNodes()
    {
        // Arrange
        var projectRoot = GetProjectRoot();
        var scenePath = Path.Combine(projectRoot, "scenes", "comparison_dashboard.tscn");
        
        // Act
        var content = File.ReadAllText(scenePath);

        // Assert
        Assert.That(content, Does.Contain("HeatmapA"), "Should contain Heatmap A overlay node");
        Assert.That(content, Does.Contain("HeatmapB"), "Should contain Heatmap B overlay node");
    }

    [Test]
    public void ComparisonDashboardScene_Has3DContextAndTiming()
    {
        // Arrange
        var projectRoot = GetProjectRoot();
        var scenePath = Path.Combine(projectRoot, "scenes", "comparison_dashboard.tscn");
        
        // Act
        var content = File.ReadAllText(scenePath);

        // Assert
        Assert.That(content, Does.Contain("2D"), "Should specify 2D representation context");
        Assert.That(content, Does.Contain("Time"), "Should contain placeholder for timing information");
    }
}
