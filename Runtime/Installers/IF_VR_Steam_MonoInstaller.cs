using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace InterVR.IF.VR.Plugin.Steam.Installer
{
    public class IF_VR_Steam_MonoInstaller : MonoInstaller<IF_VR_Steam_MonoInstaller>
    {
        public List<ScriptableObjectInstaller> settings;

        public override void InstallBindings()
        {
            var settingsInstaller = settings.Cast<IInstaller>();
            foreach (var installer in settingsInstaller)
            {
                Container.Inject(installer);
                installer.InstallBindings();
            }
        }
    }
}