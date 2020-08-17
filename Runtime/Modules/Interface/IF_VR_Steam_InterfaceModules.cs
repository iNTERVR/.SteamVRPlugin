using EcsRx.Infrastructure.Dependencies;
using EcsRx.Infrastructure.Extensions;
using InterVR.IF.VR.Modules;

namespace InterVR.IF.VR.Plugin.Steam.Modules
{
    public class IF_VR_Steam_InterfaceModules : IDependencyModule
    {
        public void Setup(IDependencyContainer container)
        {
            if (container.HasBinding<IF_VR_IInterface>())
            {
                container.Unbind<IF_VR_IInterface>();
            }
            container.Bind<IF_VR_IInterface, IF_VR_Steam_Interface>();
        }
    }
}