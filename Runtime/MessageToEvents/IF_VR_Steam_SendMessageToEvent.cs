using EcsRx.Events;
using EcsRx.Extensions;
using EcsRx.Zenject;
using InterVR.IF.Extensions;
using InterVR.IF.VR.Events;
using UniRx;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace InterVR.IF.VR.Plugin.Steam.MessageToEvents
{
    public class IF_VR_Steam_SendMessageToEvent : MonoBehaviour
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

        public void OnHandHoverBegin(Hand hand)
        {
            if (eventSystem == null)
                return;

            eventSystem.Publish(new IF_VR_Event_OnHandHoverBegin()
            {
                TargetEntity = this.gameObject.GetEntity(),
                HandEntity = hand.gameObject.GetEntity()
            });
        }

        public void OnHandHoverEnd(Hand hand)
        {
            if (eventSystem == null)
                return;

            eventSystem.Publish(new IF_VR_Event_OnHandHoverEnd()
            {
                TargetEntity = this.gameObject.GetEntity(),
                HandEntity = hand.gameObject.GetEntity()
            });
        }

        public void HandHoverUpdate(Hand hand)
        {
            if (eventSystem == null)
                return;

            eventSystem.Publish(new IF_VR_Event_HandHoverUpdate()
            {
                TargetEntity = this.gameObject.GetEntity(),
                HandEntity = hand.gameObject.GetEntity()
            });
        }

        public void OnHandFocusAcquired(Hand hand)
        {
            if (eventSystem == null)
                return;

            eventSystem.Publish(new IF_VR_Event_OnHandFocusAcquired()
            {
                TargetEntity = this.gameObject.GetEntity(),
                HandEntity = hand.gameObject.GetEntity()
            });
        }

        public void OnHandFocusLost(Hand hand)
        {
            if (eventSystem == null)
                return;

            eventSystem.Publish(new IF_VR_Event_OnHandFocusLost()
            {
                TargetEntity = this.gameObject.GetEntity(),
                HandEntity = hand.gameObject.GetEntity()
            });
        }

        public void OnAttachedToHand(Hand hand)
        {
            if (eventSystem == null)
                return;

            eventSystem.Publish(new IF_VR_Event_OnAttachedToHand()
            {
                TargetEntity = this.gameObject.GetEntity(),
                HandEntity = hand.gameObject.GetEntity()
            });
        }

        public void OnDetachedFromHand(Hand hand)
        {
            if (eventSystem == null)
                return;

            eventSystem.Publish(new IF_VR_Event_OnDetachedToHand()
            {
                TargetEntity = this.gameObject.GetEntity(),
                HandEntity = hand.gameObject.GetEntity()
            });
        }

        public void HandAttachedUpdate(Hand hand)
        {
            if (eventSystem == null)
                return;

            eventSystem.Publish(new IF_VR_Event_HandAttachedUpdate()
            {
                TargetEntity = this.gameObject.GetEntity(),
                HandEntity = hand.gameObject.GetEntity()
            });
        }

        public void OnThrowableAttachEaseInCompleted(Hand hand)
        {
            if (eventSystem == null)
                return;
        }

        // Broadcast
        // Check first before dispatch
        //  The parent is SourceEntityView.transform (same level is true)
        //  TargetEntityView.transform.IsChildOf(SourceEntityView.transform) is true
        public void OnParentHandHoverBegin()
        {
            if (eventSystem == null)
                return;

            eventSystem.Publish(new IF_VR_EventOnParentHandHoverBegin()
            {
                SourceEntity = this.gameObject.GetEntity()
            });
        }

        public void SetDeviceIndex(int deviceIndex)
        {
            if (eventSystem == null)
                return;
        }
    }
}
