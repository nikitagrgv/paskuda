using Code.Components;
using Zenject;

namespace Code.Installers
{
    public class ArsenalInstaller : MonoInstaller
    {
        public Arsenal instance;

        public override void InstallBindings()
        {
            Container.Bind<Arsenal>()
                .FromInstance(instance)
                .AsSingle()
                .NonLazy();
        }
    }
}