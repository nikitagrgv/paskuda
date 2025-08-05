using Code.Components;
using Zenject;

namespace Code.Installers
{
    public class GameConstantsInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<GameConstants>()
                .FromComponentInChildrenOf(gameObject)
                .AsSingle()
                .NonLazy();
        }
    }
}