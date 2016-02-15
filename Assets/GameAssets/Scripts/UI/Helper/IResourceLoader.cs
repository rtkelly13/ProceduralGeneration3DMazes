using UnityEngine;

namespace Assets.GameAssets.Scripts.UI.Helper
{
    public interface IResourceLoader
    {
        T InstantiateControl<T>(Transform parent) where T : Component;
    }
}