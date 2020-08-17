using EcsRx.Infrastructure.Extensions;
using InterVR.IF.Installer;
using InterVR.IF.Modules;
using InterVR.IF.VR.Modules;
using InterVR.IF.VR.Plugin.Steam.Example.Installer;
using InterVR.IF.VR.Plugin.Steam.Modules;
using UnityEngine;

namespace InterVR.IF.VR.Plugin.Steam.Example
{
    [DefaultExecutionOrder(-20000)]
    public class IF_VR_Steam_Example_ApplicationBehaviour : IF_ApplicationBehaviour
    {
        protected override void BindSystems()
        {
            base.BindSystems();
        }

        protected override void LoadModules()
        {
            base.LoadModules();

            Container.LoadModule<IF_ToolModules>();
            Container.LoadModule<IF_VR_ToolModules>();
            Container.LoadModule<IF_VR_Steam_InterfaceModules>();
            Container.LoadModule<IF_VR_Steam_ComponentBuilderModules>();
            Container.LoadModule<IF_VR_Steam_MessageToEventModules>();
        }

        protected override void LoadPlugins()
        {
            base.LoadPlugins();
        }

        protected override void ApplicationStarted()
        {
            base.ApplicationStarted();

            var settings = Container.Resolve<IF_VR_Steam_Example_Installer.Settings>();
            var interSettings = Container.Resolve<IF_Installer.Settings>();
            Debug.Log($"settings.Name is {settings.Name} in {interSettings.Name}");
        }

        private void OnDestroy()
        {
            StopAndUnbindAllSystems();
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause == false)
            {
            }
        }

        private void OnApplicationFocus(bool focus)
        {

        }
    }
}