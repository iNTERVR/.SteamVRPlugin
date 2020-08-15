using System;
using UnityEngine;
using Zenject;

namespace InterVR.IF.VR.Plugin.Steam.Installer
{
    [CreateAssetMenu(fileName = "IF_VR_Steam_Settings", menuName = "InterVR/IF/Plugin/VR/Steam/Settings")]
    public class IF_VR_Steam_Installer : ScriptableObjectInstaller<IF_VR_Steam_Installer>
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
            public string Name = "IF Steam VR Plugin Installer";
        }
    }
}