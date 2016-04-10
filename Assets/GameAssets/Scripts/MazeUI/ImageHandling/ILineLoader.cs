using UnityEngine;

namespace Assets.GameAssets.Scripts.MazeUI.ImageHandling
{
    public interface ILineLoader
    {
        SpriteRenderer GetLine(LineOption option, LineColour colour);
    }
}