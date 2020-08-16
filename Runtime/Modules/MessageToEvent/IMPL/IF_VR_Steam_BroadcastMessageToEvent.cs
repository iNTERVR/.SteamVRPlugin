using EcsRx.Events;
using InterVR.IF.Extensions;
using InterVR.IF.VR.Defines;
using InterVR.IF.VR.Events;
using System;
using UnityEngine;
using Valve.VR;

namespace InterVR.IF.VR.Plugin.Steam.Modules
{
    public class IF_VR_Steam_BroadcastMessageToEvent : IF_VR_Steam_IBroadcastMessageToEvent
    {
        private readonly IEventSystem eventSystem;

        public IF_VR_Steam_BroadcastMessageToEvent(IEventSystem eventSystem)
        {
            this.eventSystem = eventSystem;
        }

        public void OnParentHandHoverBegin(GameObject sourceGo, GameObject argGo)
        {
            eventSystem.Publish(new IF_VR_EventOnParentHandHoverBegin()
            {
                SourceEntity = sourceGo.GetEntity(),
                InteractableEntity = argGo.GetEntity()
            });
        }

        public void OnParentHandHoverEnd(GameObject sourceGo, GameObject argGo)
        {
            eventSystem.Publish(new IF_VR_Event_OnParentHandHoverEnd()
            {
                SourceEntity = sourceGo.GetEntity(),
                InteractableEntity = argGo.GetEntity()
            });
        }

        public void OnParentHandInputFocusAcquired(GameObject sourceGo)
        {
            eventSystem.Publish(new IF_VR_Event_OnParentHandInputFocusAcquired()
            {
                SourceEntity = sourceGo.GetEntity()
            });
        }

        public void OnParentHandInputFocusLost(GameObject sourceGo)
        {
            eventSystem.Publish(new IF_VR_Event_OnParentHandInputFocusLost()
            {
                SourceEntity = sourceGo.GetEntity()
            });
        }

        public void SetInputSource(GameObject sourceGo, SteamVR_Input_Sources handType)
        {
            IF_VR_HandType type = IF_VR_HandType.Any;
            if (handType == SteamVR_Input_Sources.LeftHand)
            {
                type = IF_VR_HandType.Left;
            }
            else if (handType == SteamVR_Input_Sources.RightHand)
            {
                type = IF_VR_HandType.Right;
            }

            eventSystem.Publish(new IF_VR_Event_SetInputSource()
            {
                SourceEntity = sourceGo.GetEntity(),
                HandType = type
            });
        }

        public void SetDeviceIndex(GameObject sourceGo, int deviceIndex)
        {
            eventSystem.Publish(new IF_VR_Event_SetDeviceIndex()
            {
                SourceEntity = sourceGo.GetEntity(),
                DeviceIndex = deviceIndex
            });
        }

        public void OnHandInitialized(GameObject sourceGo, int deviceIndex)
        {
            SetDeviceIndex(sourceGo, deviceIndex);
        }
    }
}