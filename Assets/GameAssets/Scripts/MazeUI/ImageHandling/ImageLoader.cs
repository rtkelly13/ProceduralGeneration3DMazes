using UnityEngine;

namespace Assets.GameAssets.Scripts.MazeUI.ImageHandling
{
    public class ImageLoader
    {
        public ResourceRequest LoadSprite(string name)
        {
            return Resources.LoadAsync<Sprite>(name);
        }
    }
}
