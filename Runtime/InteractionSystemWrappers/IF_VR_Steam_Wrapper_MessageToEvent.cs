//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using EcsRx.Events;
using EcsRx.Extensions;
using EcsRx.Zenject;
using InterVR.IF.Extensions;
using InterVR.IF.VR.Components;
using InterVR.IF.VR.Events;
using UniRx;
using UnityEngine;
using Valve.VR.InteractionSystem;
using Zenject;

namespace InterVR.IF.VR.Plugin.Steam.InteractionSystemWrapper
{
    public class IF_VR_Steam_Wrapper_MessageToEvent : MonoBehaviour
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
                    eventSystem.Receive<IF_VR_OnHandHoverEndEvent>().Subscribe(evt =>
                    {
                        Debug.Log("IF_VR_OnHandHoverEnd Event");
                    }).AddTo(gameObject);
                }).AddTo(this);
        }

        public void HandHoverUpdate(Hand hand)
        {
            if (eventSystem == null)
                return;

            eventSystem.Publish(new IF_VR_OnHandHoverUpdateEvent()
            {
                HandEntity = hand.gameObject.GetEntity()
            });
        }

        public void OnHandHoverBegin(Hand hand)
        {
            if (eventSystem == null)
                return;

            eventSystem.Publish(new IF_VR_OnHandHoverBeginEvent()
            {
                HandEntity = hand.gameObject.GetEntity(),
                HoveringEntity = this.gameObject.GetEntity()
            });
        }

        public void OnHandHoverEnd(Hand hand)
        {
            if (eventSystem == null)
                return;

            eventSystem.Publish(new IF_VR_OnHandHoverEndEvent()
            {
                HandEntity = hand.gameObject.GetEntity(),
                HoveringEntity = this.gameObject.GetEntity()
            });
        }

        public void OnHandFocusLost(Hand hand)
        {
            if (eventSystem == null)
                return;

            eventSystem.Publish(new IF_VR_OnHandFocusLostEvent()
            {
                HandEntity = hand.gameObject.GetEntity(),
            });
        }

        public void OnAttachedToHand(Hand hand)
        {
            if (eventSystem == null)
                return;

            var handEntity = hand.gameObject.GetEntity();

            var interactableEntity = this.gameObject.GetEntity();
            var interactable = interactableEntity.GetComponent<IF_VR_Interactable>();
            interactable.AttachedToHandEntity = handEntity;

            eventSystem.Publish(new IF_VR_OnAttachedToHandEvent()
            {
                HandEntity = handEntity
            });
        }

        public void OnDetachedFromHand(Hand hand)
        {
            if (eventSystem == null)
                return;

            eventSystem.Publish(new IF_VR_OnDetachedToHandEvent()
            {
                HandEntity = hand.gameObject.GetEntity()
            });
        }

        public void OnHandFocusAcquired(Hand hand)
        {
            if (eventSystem == null)
                return;

            eventSystem.Publish(new IF_VR_OnHandFocusAcquiredEvent()
            {
                HandEntity = hand.gameObject.GetEntity()
            });
        }

        public void HandAttachedUpdate(Hand hand)
        {
            if (eventSystem == null)
                return;

            eventSystem.Publish(new IF_VR_OnHandAttachedUpdateEvent()
            {
                HandEntity = hand.gameObject.GetEntity()
            });
        }

        public void OnThrowableAttachEaseInCompleted(Hand hand)
        {
            if (eventSystem == null)
                return;
        }
    }
}
