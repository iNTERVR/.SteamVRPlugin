using EcsRx.Infrastructure.Dependencies;
using EcsRx.Infrastructure.Extensions;

namespace InterVR.IF.VR.Plugin.Steam.Modules
{
    public class IF_VR_Steam_ComponentBuilderModule : IDependencyModule
    {
        public void Setup(IDependencyContainer container)
        {
            container.Bind<IF_VR_Steam_IComponentBuilder, IF_VR_Steam_ComponentBuilder>();
        }
    }
}