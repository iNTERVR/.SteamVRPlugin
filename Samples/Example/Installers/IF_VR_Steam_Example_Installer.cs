using System;
using UnityEngine;
using Zenject;

namespace InterVR.IF.VR.Plugin.Steam.Example.Installer
{
    [CreateAssetMenu(fileName = "IF_VR_Steam_Example_Settings", menuName = "InterVR/IF/Plugin/VR/Steam/Example/Settings")]
    public class IF_VR_Steam_Example_Installer : ScriptableObjectInstaller<IF_VR_Steam_Example_Installer>
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
            public string Name = "IF Steam VR Plugin Example IF_VR_Steam_Interactable Installer";
        }
    }
}