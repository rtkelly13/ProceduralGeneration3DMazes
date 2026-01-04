using BenchmarkDotNet.Attributes;
using ProceduralMaze.Maze.Helper;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Benchmarks;

/// <summary>
/// Benchmarks for helper utility functions.
/// Tests ArrayHelper.Shuffle and DirectionsFlagParser performance.
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class HelperBenchmarks
{
    private int[] _smallArray = null!;
    private int[] _mediumArray = null!;
    private int[] _largeArray = null!;
    private List<int> _smallList = null!;
    private List<int> _mediumList = null!;
    private List<int> _largeList = null!;
    private DirectionsFlagParser _parser = null!;

    [GlobalSetup]
    public void Setup()
    {
        _smallArray = Enumerable.Range(0, 6).ToArray();      // Typical direction count
        _mediumArray = Enumerable.Range(0, 100).ToArray();
        _largeArray = Enumerable.Range(0, 10000).ToArray();
        
        _smallList = Enumerable.Range(0, 6).ToList();
        _mediumList = Enumerable.Range(0, 100).ToList();
        _largeList = Enumerable.Range(0, 10000).ToList();
        
        _parser = new DirectionsFlagParser();
    }

    [IterationSetup]
    public void IterationSetup()
    {
        // Reset arrays/lists before each iteration since shuffle modifies in-place
        _smallArray = Enumerable.Range(0, 6).ToArray();
        _mediumArray = Enumerable.Range(0, 100).ToArray();
        _largeArray = Enumerable.Range(0, 10000).ToArray();
        
        _smallList = Enumerable.Range(0, 6).ToList();
        _mediumList = Enumerable.Range(0, 100).ToList();
        _largeList = Enumerable.Range(0, 10000).ToList();
    }

    // Array shuffle benchmarks
    [Benchmark(Baseline = true)]
    public void Shuffle_Array_Small_6()
    {
        ArrayHelper.Shuffle(_smallArray);
    }

    [Benchmark]
    public void Shuffle_Array_Medium_100()
    {
        ArrayHelper.Shuffle(_mediumArray);
    }

    [Benchmark]
    public void Shuffle_Array_Large_10000()
    {
        ArrayHelper.Shuffle(_largeArray);
    }

    // List shuffle benchmarks
    [Benchmark]
    public void Shuffle_List_Small_6()
    {
        ArrayHelper.Shuffle(_smallList);
    }

    [Benchmark]
    public void Shuffle_List_Medium_100()
    {
        ArrayHelper.Shuffle(_mediumList);
    }

    [Benchmark]
    public void Shuffle_List_Large_10000()
    {
        ArrayHelper.Shuffle(_largeList);
    }

    // DirectionsFlagParser benchmarks
    [Benchmark]
    public Direction[] ParseDirections_Single()
    {
        return _parser.SplitDirectionsFromFlag(Direction.Left);
    }

    [Benchmark]
    public Direction[] ParseDirections_AllSix()
    {
        return _parser.SplitDirectionsFromFlag(
            Direction.Left | Direction.Right | Direction.Up | 
            Direction.Down | Direction.Forward | Direction.Back);
    }

    [Benchmark]
    public Direction[] ParseDirections_ThreeTypical()
    {
        return _parser.SplitDirectionsFromFlag(Direction.Left | Direction.Forward | Direction.Up);
    }
}
