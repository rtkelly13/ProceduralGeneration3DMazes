using System;
using ProceduralMaze.Maze.Helper;
using ProceduralMaze.Maze.Generation;

namespace ProceduralMaze.Maze.Agents
{
    public class AgentFactory : IAgentFactory
    {
        private readonly IDirectionsFlagParser _directionsFlagParser;
        private readonly IPointsAndDirectionsRetriever _pointsAndDirectionsRetriever;
        private readonly IRandomValueGenerator _randomValueGenerator;

        public AgentFactory(IDirectionsFlagParser directionsFlagParser,
            IPointsAndDirectionsRetriever pointsAndDirectionsRetriever,
            IRandomValueGenerator randomValueGenerator)
        {
            _directionsFlagParser = directionsFlagParser;
            _pointsAndDirectionsRetriever = pointsAndDirectionsRetriever;
            _randomValueGenerator = randomValueGenerator;
        }

        public IAgent MakeAgent(AgentType type)
        {
            switch (type)
            {
                case AgentType.Random:
                    return new RandomAgent(_pointsAndDirectionsRetriever, _directionsFlagParser, _randomValueGenerator);
                case AgentType.Perfect:
                    return new PerfectAgent(_directionsFlagParser, _randomValueGenerator);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
