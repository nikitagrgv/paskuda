using Code.Components;
using Zenject;

namespace Code.Installers
{
    public class GameFieldInstaller : MonoInstaller
    {
        public GameField instance;

        public override void InstallBindings()
        {
            Container.Bind<GameField>()
                .FromInstance(instance)
                .AsSingle()
                .NonLazy();
        }
    }
}