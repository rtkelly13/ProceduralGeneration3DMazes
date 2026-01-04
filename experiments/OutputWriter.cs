namespace ProceduralMaze.Experiments;

public class OutputWriter : IOutputWriter
{
    public void Print(string val)
    {
        Console.Write(val);
    }

    public void PrintLn(string val)
    {
        Console.WriteLine(val);
    }
}
