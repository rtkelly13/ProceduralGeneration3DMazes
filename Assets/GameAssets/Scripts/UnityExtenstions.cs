using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.GameAssets.Scripts
{
    public static class UnityExtenstions
    {
        public static void Clear(this Transform transform)
        {
            var children = (from Transform child in transform select child.gameObject).ToList();
            foreach (var child in children)
            {
                Object.Destroy(child);
            }
        }
    }
}
