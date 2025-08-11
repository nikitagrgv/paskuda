using Code.Components;
using Zenject;

namespace Code.Installers
{
    public class GameControllerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<GameController>()
                .FromComponentInChildrenOf(gameObject)
                .AsSingle()
                .NonLazy();
        }
    }
}