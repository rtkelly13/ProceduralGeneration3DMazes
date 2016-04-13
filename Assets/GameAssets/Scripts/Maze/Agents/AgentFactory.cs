using System;
using Assets.GameAssets.Scripts.Maze.Helper;
using Assets.GameAssets.Scripts.Maze.MazeGeneration;

namespace Assets.GameAssets.Scripts.Maze.Agents
{
    public class AgentFactory : IAgentFactory
    {
        private readonly IDirectionsFlagParser _directionsFlagParser;
        private IPointsAndDirectionsRetriever _pointsAndDirectionsRetriever;

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
                    return new RandomAgent2(_pointsAndDirectionsRetriever,_directionsFlagParser);
                case AgentType.Perfect:
                    return new PerfectAgent(_directionsFlagParser);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}