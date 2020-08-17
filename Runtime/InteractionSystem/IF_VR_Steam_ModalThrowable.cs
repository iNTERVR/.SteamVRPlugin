//======= Copyright (c) iNTERVR, All rights reserved. =========================
// Override InteractionSystem from SteamVR Unity Plugin of Valve Corporation
//
// Purpose: Basic throwable object
//
//=============================================================================

using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace InterVR.IF.VR.Plugin.Steam.InteractionSystem
{
    public class IF_VR_Steam_ModalThrowable : IF_VR_Steam_Throwable
    {
        [Tooltip("The local point which acts as a positional and rotational offset to use while held with a grip type grab")]
        public Transform gripOffset;

        [Tooltip("The local point which acts as a positional and rotational offset to use while held with a pinch type grab")]
        public Transform pinchOffset;

        protected override void HandHoverUpdate(IF_VR_Steam_Hand hand)
        {
            IF_VR_Steam_GrabTypes startingGrabType = hand.GetGrabStarting();

            if (startingGrabType != IF_VR_Steam_GrabTypes.None)
            {
                if (startingGrabType == IF_VR_Steam_GrabTypes.Pinch)
                {
                    hand.AttachObject(gameObject, startingGrabType, attachmentFlags, pinchOffset);
                }
                else if (startingGrabType == IF_VR_Steam_GrabTypes.Grip)
                {
                    hand.AttachObject(gameObject, startingGrabType, attachmentFlags, gripOffset);
                }
                else
                {
                    hand.AttachObject(gameObject, startingGrabType, attachmentFlags, attachmentOffset);
                }

                hand.HideGrabHint();
            }
        }
        protected override void HandAttachedUpdate(IF_VR_Steam_Hand hand)
        {
            if (interactable.skeletonPoser != null)
            {
                interactable.skeletonPoser.SetBlendingBehaviourEnabled("PinchPose", hand.currentAttachedObjectInfo.Value.grabbedWithType == IF_VR_Steam_GrabTypes.Pinch);
            }

            base.HandAttachedUpdate(hand);
        }
    }
}