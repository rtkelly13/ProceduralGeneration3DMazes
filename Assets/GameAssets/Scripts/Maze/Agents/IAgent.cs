using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.Agents
{
    public interface IAgent
    {
        AgentResults RunAgent(IMazeJumper mazeJumper);
    }
}