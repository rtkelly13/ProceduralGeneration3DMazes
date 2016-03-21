using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.GameAssets.Scripts.MazeUI
{
    public class ImageLoader
    {
        public ResourceRequest LoadSprite(string name)
        {
            return Resources.LoadAsync<Sprite>(name);
        }
    }
}
