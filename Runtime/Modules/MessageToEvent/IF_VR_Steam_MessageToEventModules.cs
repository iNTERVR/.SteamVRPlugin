using EcsRx.Infrastructure.Dependencies;
using EcsRx.Infrastructure.Extensions;

namespace InterVR.IF.VR.Plugin.Steam.Modules
{
    public class IF_VR_Steam_MessageToEventModules : IDependencyModule
    {
        public void Setup(IDependencyContainer container)
        {
            container.Bind<IF_VR_Steam_ISendMessageToEvent, IF_VR_Steam_SendMessageToEvent>();
            container.Bind<IF_VR_Steam_IBroadcastMessageToEvent, IF_VR_Steam_BroadcastMessageToEvent>();
        }
    }
}