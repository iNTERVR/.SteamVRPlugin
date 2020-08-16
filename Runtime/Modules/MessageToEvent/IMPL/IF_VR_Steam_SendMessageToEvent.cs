using EcsRx.Events;
using InterVR.IF.Extensions;
using InterVR.IF.VR.Events;
using System;
using UnityEngine;

namespace InterVR.IF.VR.Plugin.Steam.Modules
{
    public class IF_VR_Steam_SendMessageToEvent : IF_VR_Steam_ISendMessageToEvent
    {
        private readonly IEventSystem eventSystem;

        public IF_VR_Steam_SendMessageToEvent(IEventSystem eventSystem)
        {
            this.eventSystem = eventSystem;
        }

        public void OnHandHoverBegin(GameObject sourceGo, GameObject argGo)
        {
            eventSystem.Publish(new IF_VR_Event_OnHandHoverBegin()
            {
                TargetEntity = sourceGo.GetEntity(),
                HandEntity = argGo.GetEntity()
            });
        }

        public void OnHandHoverEnd(GameObject sourceGo, GameObject argGo)
        {
            eventSystem.Publish(new IF_VR_Event_OnHandHoverEnd()
            {
                TargetEntity = sourceGo.GetEntity(),
                HandEntity = argGo.GetEntity()
            });
        }

        public void HandHoverUpdate(GameObject sourceGo, GameObject argGo)
        {
            eventSystem.Publish(new IF_VR_Event_HandHoverUpdate()
            {
                TargetEntity = sourceGo.GetEntity(),
                HandEntity = argGo.GetEntity()
            });
        }

        public void OnHandFocusAcquired(GameObject sourceGo, GameObject argGo)
        {
            eventSystem.Publish(new IF_VR_Event_OnHandFocusAcquired()
            {
                TargetEntity = sourceGo.GetEntity(),
                HandEntity = argGo.GetEntity()
            });
        }

        public void OnHandFocusLost(GameObject sourceGo, GameObject argGo)
        {
            eventSystem.Publish(new IF_VR_Event_OnHandFocusLost()
            {
                TargetEntity = sourceGo.GetEntity(),
                HandEntity = argGo.GetEntity()
            });
        }

        public void OnAttachedToHand(GameObject sourceGo, GameObject argGo)
        {
            eventSystem.Publish(new IF_VR_Event_OnAttachedToHand()
            {
                TargetEntity = sourceGo.GetEntity(),
                HandEntity = argGo.GetEntity()
            });
        }

        public void OnDetachedFromHand(GameObject sourceGo, GameObject argGo)
        {
            eventSystem.Publish(new IF_VR_Event_OnDetachedToHand()
            {
                TargetEntity = sourceGo.GetEntity(),
                HandEntity = argGo.GetEntity()
            });
        }

        public void HandAttachedUpdate(GameObject sourceGo, GameObject argGo)
        {
            eventSystem.Publish(new IF_VR_Event_HandAttachedUpdate()
            {
                TargetEntity = sourceGo.GetEntity(),
                HandEntity = argGo.GetEntity()
            });
        }

        public void OnThrowableAttachEaseInCompleted(GameObject sourceGo, GameObject argGo)
        {
            eventSystem.Publish(new IF_VR_Event_OnThrowableAttachEaseInCompleted()
            {
                TargetEntity = sourceGo.GetEntity(),
                HandEntity = argGo.GetEntity()
            });
        }
    }
}