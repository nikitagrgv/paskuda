using Code.Components;
using Zenject;

namespace Code.Installers
{
    public class TimeControllerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<TimeController>()
                .FromComponentInChildrenOf(gameObject)
                .AsSingle()
                .NonLazy();
        }
    }
}