using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.GameAssets.Scripts.Maze.Model;

namespace Assets.GameAssets.Scripts.Maze.Agents
{
    public abstract class AgentBase : IAgent
    {
        public AgentResults RunAgent(IMazeJumper mazeJumper)
        {
            mazeJumper.SetState(ModelMode.Standard);
            mazeJumper.JumpToPoint(mazeJumper.StartPoint);
            return RunAgentBase(mazeJumper.JumpingFinished());
        }

        public abstract AgentResults RunAgentBase(IMaze maze);
    }
}
