using NUnit.Framework;
using System;
using System.IO;

namespace ProceduralMaze.Tests;

[TestFixture]
public class MainMenuTests
{
    private string GetProjectRoot()
    {
        var dir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "ProceduralGeneration3DMazes.sln")))
        {
            dir = dir.Parent;
        }
        return dir?.FullName ?? throw new DirectoryNotFoundException("Could not find project root");
    }

    [Test]
    public void MainMenuScene_HasComparisonButton()
    {
        // Arrange
        var projectRoot = GetProjectRoot();
        var scenePath = Path.Combine(projectRoot, "scenes", "menu.tscn");
        
        // Act
        var content = File.ReadAllText(scenePath);

        // Assert
        Assert.That(content, Does.Contain("ComparisonButton"), "Should contain ComparisonButton node");
    }
}
