using InterVR.IF.VR.Defines;
using EcsRx.Entities;
using UniRx;
using UnityEngine;
using Valve.VR;

namespace InterVR.IF.VR.Plugin.Steam.Modules
{
    public interface IF_VR_Steam_IBroadcastMessageToEvent
    {
        void OnParentHandHoverBegin(GameObject sourceGo, GameObject argGo);
        void OnParentHandHoverEnd(GameObject sourceGo, GameObject argGo);
        void OnParentHandInputFocusAcquired(GameObject sourceGo);
        void OnParentHandInputFocusLost(GameObject sourceGo);
        void SetInputSource(GameObject sourceGo, SteamVR_Input_Sources handType);
        void SetDeviceIndex(GameObject sourceGo, int deviceIndex);
        void OnHandInitialized(GameObject sourceGo, int deviceIndex);
    }
}