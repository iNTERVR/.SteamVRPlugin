using EcsRx.Infrastructure.Dependencies;
using EcsRx.Infrastructure.Extensions;
using InterVR.IF.VR.Modules;

namespace InterVR.IF.VR.Plugin.Steam.Modules
{
    public class IF_VR_Steam_StatusModules : IDependencyModule
    {
        public void Setup(IDependencyContainer container)
        {
            if (container.HasBinding<IF_VR_IGrabStatus>())
            {
                container.Unbind<IF_VR_IGrabStatus>();
            }
            container.Bind<IF_VR_IGrabStatus, IF_VR_Steam_GrabStatus>();
        }
    }
}