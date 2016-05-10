using System;
using Assets.GameAssets.Scripts.Maze.Helper;
using Assets.GameAssets.Scripts.Maze.MazeGeneration;

namespace Assets.GameAssets.Scripts.Maze.Agents
{
    public class AgentFactory : IAgentFactory
    {
        private readonly IDirectionsFlagParser _directionsFlagParser;
        private IPointsAndDirectionsRetriever _pointsAndDirectionsRetriever;
        private readonly IArrayHelper _arrayHelper;

        public AgentFactory(IDirectionsFlagParser directionsFlagParser, IPointsAndDirectionsRetriever pointsAndDirectionsRetriever, IArrayHelper arrayHelper)
        {
            _directionsFlagParser = directionsFlagParser;
            _pointsAndDirectionsRetriever = pointsAndDirectionsRetriever;
            _arrayHelper = arrayHelper;
        }

        public IAgent MakeAgent(AgentType type)
        {
            switch (type)
            {
                case AgentType.Random:
                    return new RandomAgent2(_pointsAndDirectionsRetriever,_directionsFlagParser, _arrayHelper);
                case AgentType.Perfect:
                    return new PerfectAgent(_directionsFlagParser, _arrayHelper);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}