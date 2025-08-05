using Code.Components;
using Zenject;

namespace Code.Installers
{
    public class HealthInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<Health>()
                .FromComponentInParentsAndChildren()
                .AsTransient()
                .Lazy();
        }
    }
}