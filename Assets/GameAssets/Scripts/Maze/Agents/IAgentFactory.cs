namespace Assets.GameAssets.Scripts.Maze.Agents
{
    public interface IAgentFactory
    {
        IAgent MakeAgent(AgentType type);
    }
}