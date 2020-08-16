using EcsRx.Events;
using EcsRx.Extensions;
using EcsRx.Zenject;
using InterVR.IF.Extensions;
using InterVR.IF.VR.Defines;
using InterVR.IF.VR.Events;
using UniRx;
using UnityEngine;
using Valve.VR;

namespace InterVR.IF.VR.Plugin.Steam.MessageToEvents
{
    // Broadcast
    // Check first before dispatch
    //  The parent is SourceEntityView.transform (same level is true)
    //  TargetEntityView.transform.IsChildOf(SourceEntityView.transform) is true
    public class IF_VR_Steam_BroadcastMessageToEvent : MonoBehaviour
    {
        private IEventSystem eventSystem;

        private void Start()
        {
            Observable.EveryUpdate()
                .Where(x => EcsRxApplicationBehaviour.Instance.EventSystem != null)
                .First()
                .Subscribe(x =>
                {
                    eventSystem = EcsRxApplicationBehaviour.Instance.EventSystem;
                }).AddTo(this);
        }

        public void OnParentHandHoverBegin(GameObject interactable)
        {
            if (eventSystem == null)
                return;

            eventSystem.Publish(new IF_VR_EventOnParentHandHoverBegin()
            {
                SourceEntity = this.gameObject.GetEntity(),
                InteractableEntity = interactable.GetEntity()
            });
        }

        public void OnParentHandHoverEnd(GameObject interactable)
        {
            if (eventSystem == null)
                return;

            eventSystem.Publish(new IF_VR_Event_OnParentHandHoverEnd()
            {
                SourceEntity = this.gameObject.GetEntity(),
                InteractableEntity = interactable.GetEntity()
            });
        }

        public void OnParentHandInputFocusAcquired()
        {
            if (eventSystem == null)
                return;

            eventSystem.Publish(new IF_VR_Event_OnParentHandInputFocusAcquired()
            {
                SourceEntity = this.gameObject.GetEntity()
            });
        }

        public void OnParentHandInputFocusLost()
        {
            if (eventSystem == null)
                return;

            eventSystem.Publish(new IF_VR_Event_OnParentHandInputFocusLost()
            {
                SourceEntity = this.gameObject.GetEntity()
            });
        }

        public void SetInputSource(SteamVR_Input_Sources handType)
        {
            if (eventSystem == null)
                return;

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
                SourceEntity = this.gameObject.GetEntity(),
                HandType = type
            });
        }

        public void SetDeviceIndex(int deviceIndex)
        {
            if (eventSystem == null)
                return;

            eventSystem.Publish(new IF_VR_Event_SetDeviceIndex()
            {
                SourceEntity = this.gameObject.GetEntity(),
                DeviceIndex = deviceIndex
            });
        }

        public void OnHandInitialized(int deviceIndex)
        {
            if (eventSystem == null)
                return;

            eventSystem.Publish(new IF_VR_Event_SetDeviceIndex()
            {
                SourceEntity = this.gameObject.GetEntity(),
                DeviceIndex = deviceIndex
            });
        }
    }
}
