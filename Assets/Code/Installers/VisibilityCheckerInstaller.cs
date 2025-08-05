using Code.Components;
using Zenject;

namespace Code.Installers
{
    public class VisibilityCheckerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<VisibilityChecker>()
                .FromComponentInParentsAndChildren()
                .AsTransient()
                .Lazy();
        }
    }
}