namespace ProceduralMaze.Maze.Agents
{
    public interface IAgentFactory
    {
        IAgent MakeAgent(AgentType type);
    }
}
