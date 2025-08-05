using Code.Components;
using Zenject;

namespace Code.Installers
{
    public class GeneralCharacterControllerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<GeneralCharacterController>()
                .FromComponentInParentsAndChildren()
                .AsTransient()
                .Lazy();
        }
    }
}