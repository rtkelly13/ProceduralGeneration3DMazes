using UnityEngine;

namespace Assets.GameAssets.Scripts.MazeUI.ImageHandling
{
    public interface ISpriteCache
    {
        Sprite GetSprite(string name);
    }
}