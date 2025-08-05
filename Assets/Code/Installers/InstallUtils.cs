using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Code.Installers
{
    public static class InstallUtils
    {
        public static ScopeConcreteIdArgConditionCopyNonLazyBinder FromComponentInChildrenOf<TContract>(
            this FromBinderGeneric<TContract> self, GameObject root)
        {
            TContract c = root.GetComponentInChildren<TContract>();
            Assert.IsNotNull(c);
            return self.FromInstance(c);
        }
    }
}