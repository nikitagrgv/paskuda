using Code.Components;
using Code.Factories;
using Zenject;

namespace Code.Installers
{
    public class ActorFactoryInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<ActorFactory>()
                .FromComponentInChildrenOf(gameObject)
                .AsSingle()
                .NonLazy();
        }
    }
}