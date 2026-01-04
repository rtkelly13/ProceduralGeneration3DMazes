using System;
using System.IO;
using System.Text;
using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Serialization
{
    /// <summary>
    /// Serializes maze models to a text-based format.
    /// </summary>
    public class MazeSerializer : IMazeSerializer
    {
        public void Serialize(IModel model, Stream output)
        {
            using var writer = new StreamWriter(output, Encoding.UTF8, leaveOpen: true);
            
            // Write header
            writer.WriteLine($"SIZE {model.Size.X} {model.Size.Y} {model.Size.Z}");
            writer.WriteLine($"START {model.StartPoint.X} {model.StartPoint.Y} {model.StartPoint.Z}");
            writer.WriteLine($"END {model.EndPoint.X} {model.EndPoint.Y} {model.EndPoint.Z}");
            writer.WriteLine("CELLS");

            // Write cells with connections (skip cells with no connections)
            Span<char> dirBuffer = stackalloc char[6];
            for (int z = 0; z < model.Size.Z; z++)
            {
                for (int y = 0; y < model.Size.Y; y++)
                {
                    for (int x = 0; x < model.Size.X; x++)
                    {
                        var point = new MazePoint(x, y, z);
                        var directions = model.GetFlagFromPoint(point);
                        
                        if (directions != Direction.None)
                        {
                            int len = FormatDirections(directions, dirBuffer);
                            writer.WriteLine($"{x} {y} {z} {dirBuffer[..len].ToString()}");
                        }
                    }
                }
            }
        }

        public string SerializeToString(IModel model)
        {
            using var memoryStream = new MemoryStream();
            Serialize(model, memoryStream);
            memoryStream.Position = 0;
            using var reader = new StreamReader(memoryStream, Encoding.UTF8);
            return reader.ReadToEnd();
        }

        private static int FormatDirections(Direction directions, Span<char> buffer)
        {
            int index = 0;
            
            if ((directions & Direction.Left) != 0)
                buffer[index++] = 'L';
            if ((directions & Direction.Right) != 0)
                buffer[index++] = 'R';
            if ((directions & Direction.Down) != 0)
                buffer[index++] = 'D';
            if ((directions & Direction.Up) != 0)
                buffer[index++] = 'U';
            if ((directions & Direction.Back) != 0)
                buffer[index++] = 'B';
            if ((directions & Direction.Forward) != 0)
                buffer[index++] = 'F';
            
            return index;
        }
    }
}
