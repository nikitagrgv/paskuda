using UnityEngine;
using UnityEngine.Assertions;
using Zenject;

namespace Code.Installers
{
    public static class InstallUtils
    {
        public static ScopeConcreteIdArgConditionCopyNonLazyBinder FromComponentInChildrenOf<TContract>(
            this FromBinderGeneric<TContract> self, GameObject root) where TContract : MonoBehaviour
        {
            TContract c = root.GetComponentInChildren<TContract>();
            Assert.IsNotNull(c);
            return self.FromInstance(c);
        }

        public static ScopeConcreteIdArgConditionCopyNonLazyBinder FromComponentInParentsAndChildren<TContract>(
            this FromBinderGeneric<TContract> self) where TContract : MonoBehaviour
        {
            return self.FromComponentsOn(ctx =>
            {
                MonoBehaviour monoBehaviour = (MonoBehaviour)ctx.ObjectInstance;
                GameObject go = monoBehaviour.gameObject;
                Assert.IsNotNull(go);

                TContract c = go.GetComponentInParent<TContract>();
                if (!c)
                {
                    c = go.GetComponentInChildren<TContract>();
                }

                Assert.IsNotNull(c);

                // TODO: Shitty
                return c.gameObject;
            });
        }
    }
}