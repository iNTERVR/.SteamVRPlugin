using System;
using UnityEngine;
using Zenject;

namespace InterVR.IF.VR.Plugin.Steam.Examples.Throwing.Installer
{
    [CreateAssetMenu(fileName = "IF_VR_Steam_Example_Throwing_Settings", menuName = "InterVR/IF/Plugin/VR/Steam/Example/Throwing/Settings")]
    public class IF_VR_Steam_Example_Throwing_Installer : ScriptableObjectInstaller<IF_VR_Steam_Example_Throwing_Installer>
    {
#pragma warning disable 0649
        [SerializeField]
        Settings settings;
#pragma warning restore 0649

        public override void InstallBindings()
        {
            Container.BindInstance(settings).IfNotBound();
        }

        [Serializable]
        public class Settings
        {
            public string Name = "IF Steam VR Plugin Example Throwing Installer";
        }
    }
}