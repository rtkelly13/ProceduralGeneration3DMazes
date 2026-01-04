using System;
using ProceduralMaze.Maze.Helper;
using ProceduralMaze.Maze.Generation;

namespace ProceduralMaze.Maze.Agents
{
    public class AgentFactory : IAgentFactory
    {
        private readonly IDirectionsFlagParser _directionsFlagParser;
        private readonly IPointsAndDirectionsRetriever _pointsAndDirectionsRetriever;

        public AgentFactory(IDirectionsFlagParser directionsFlagParser, IPointsAndDirectionsRetriever pointsAndDirectionsRetriever)
        {
            _directionsFlagParser = directionsFlagParser;
            _pointsAndDirectionsRetriever = pointsAndDirectionsRetriever;
        }

        public IAgent MakeAgent(AgentType type)
        {
            switch (type)
            {
                case AgentType.Random:
                    return new RandomAgent(_pointsAndDirectionsRetriever, _directionsFlagParser);
                case AgentType.Perfect:
                    return new PerfectAgent(_directionsFlagParser);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
