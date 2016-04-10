using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Zenject;

namespace Assets.GameAssets.Scripts.MazeUI.ImageHandling
{
    public class LineLoader : ILineLoader
    {
        private readonly ISpriteCache _spriteCache;

        public LineLoader(ISpriteCache spriteCache)
        {
            _spriteCache = spriteCache;
        }

        public SpriteRenderer GetLine(LineOption option, LineColour colour)
        {
            Sprite sprite = null;
            switch (option)
            {
                case LineOption.HalfLine:
                    sprite = _spriteCache.GetSprite(String.Format("halfLine{0}", Enum.GetName(typeof(LineColour), colour)));
                    break;
                case LineOption.FullLine:
                    sprite = _spriteCache.GetSprite(String.Format("fullLine{0}", Enum.GetName(typeof(LineColour), colour)));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            var spriteRendererObj  = new GameObject();
            spriteRendererObj.AddComponent<SpriteRenderer>();
            var spriteRenderer = spriteRendererObj.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            return spriteRenderer;
        }
    }
}
