using Code.Components;
using Zenject;

namespace Code.Installers
{
    public class CameraManagerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<CameraManager>()
                .FromComponentInChildrenOf(gameObject)
                .AsSingle()
                .NonLazy();
        }
    }
}