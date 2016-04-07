using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.GameAssets.Scripts.MazeUI.ImageHandling
{
    public class CircleLoader : ICircleLoader
    {
        private readonly ISpriteCache _spriteCache;

        public CircleLoader(ISpriteCache spriteCache)
        {
            _spriteCache = spriteCache;
        }

        public CirclePrefab GetPrefab(string circleName)
        {
            var resource = Resources.Load<CirclePrefab>("Circle");
            var prefab = UnityEngine.Object.Instantiate(resource);
            prefab.Renderer.sprite = _spriteCache.GetSprite(String.Format("{0}Circle", circleName));
            return prefab;
        }
    }
}
