using UnityEngine;

namespace Assets.GameAssets.Scripts.UI.Helper
{
    public class ResourceLoader : IResourceLoader
    {
        public T InstantiateControl<T>(Transform parent) where T : Component
        {
            T prefab = Resources.Load<T>(typeof(T).Name);
            T control = UnityEngine.Object.Instantiate(prefab);
            control.transform.SetParent(parent, false);
            control.transform.localPosition = Vector3.zero;
            control.transform.localRotation = Quaternion.identity;
            control.transform.localScale = Vector3.one;
            return control;
        }
    }
}
