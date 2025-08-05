using Code.Components;
using Zenject;

namespace Code.Installers
{
    public class ProjectileManagerInstaller : MonoInstaller
    {
        public ProjectileManager projectileManager;

        public override void InstallBindings()
        {
            Container.Bind<ProjectileManager>()
                .FromInstance(projectileManager)
                .AsSingle()
                .NonLazy();
        }
    }
}