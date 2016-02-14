using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Assets.GameAssets.Scripts.UI
{
    public static class DependencyContainer
    {
        private static IDependencyResolver _container;
        public static IDependencyResolver Container
        {
            get { return _container ?? (_container = new DependencyResolver()); }
        }
    }
}
