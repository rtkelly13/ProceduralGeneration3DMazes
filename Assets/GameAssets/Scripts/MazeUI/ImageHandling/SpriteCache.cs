using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.GameAssets.Scripts.MazeUI.ImageHandling
{
    public class SpriteCache : ISpriteCache
    {
        private readonly Dictionary<string, Sprite> _imagesDictionary;

        public SpriteCache()
        {
            _imagesDictionary = new Dictionary<string, Sprite>();
        }

        public Sprite GetSprite(string name)
        {
            if (_imagesDictionary.ContainsKey(name))
            {
                return _imagesDictionary[name];
            }
            Sprite sprite = Resources.Load<Sprite>(String.Format("Sprites/{0}", name));
            _imagesDictionary[name] = sprite;
            return sprite;
        }
    }
}
