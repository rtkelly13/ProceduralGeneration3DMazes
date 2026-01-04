using ProceduralMaze.Maze.Model;

namespace ProceduralMaze.Maze.Agents
{
    public interface IAgent
    {
        AgentResults RunAgent(IMazeJumper mazeJumper);
    }
}
