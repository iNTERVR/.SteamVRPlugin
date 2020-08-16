//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Demonstrates how to create a simple interactable object
//
//=============================================================================

using UnityEngine;
using System.Collections;
using InterVR.IF.VR.Plugin.Steam.InteractionSystem;

namespace InterVR.IF.VR.Plugin.Steam.Example
{
	//-------------------------------------------------------------------------
	[RequireComponent( typeof( IF_VR_Steam_Interactable ) )]
	public class IF_VR_Steam_InteractableExample : MonoBehaviour
    {
        private TextMesh generalText;
        private TextMesh hoveringText;
        private Vector3 oldPosition;
		private Quaternion oldRotation;

		private float attachTime;

		private IF_VR_Steam_Hand.AttachmentFlags attachmentFlags = IF_VR_Steam_Hand.defaultAttachmentFlags & ( ~IF_VR_Steam_Hand.AttachmentFlags.SnapOnAttach ) & (~IF_VR_Steam_Hand.AttachmentFlags.DetachOthers) & (~IF_VR_Steam_Hand.AttachmentFlags.VelocityMovement);

        private IF_VR_Steam_Interactable interactable;

		//-------------------------------------------------
		void Awake()
		{
			var textMeshs = GetComponentsInChildren<TextMesh>();
            generalText = textMeshs[0];
            hoveringText = textMeshs[1];

            generalText.text = "No IF_VR_Steam_Hand Hovering";
            hoveringText.text = "Hovering: False";

            interactable = this.GetComponent<IF_VR_Steam_Interactable>();
		}


		//-------------------------------------------------
		// Called when a IF_VR_Steam_Hand starts hovering over this object
		//-------------------------------------------------
		private void OnHandHoverBegin( IF_VR_Steam_Hand hand )
		{
			generalText.text = "Hovering hand: " + hand.name;
		}


		//-------------------------------------------------
		// Called when a IF_VR_Steam_Hand stops hovering over this object
		//-------------------------------------------------
		private void OnHandHoverEnd( IF_VR_Steam_Hand hand )
		{
			generalText.text = "No IF_VR_Steam_Hand Hovering";
		}


		//-------------------------------------------------
		// Called every Update() while a IF_VR_Steam_Hand is hovering over this object
		//-------------------------------------------------
		private void HandHoverUpdate( IF_VR_Steam_Hand hand )
		{
            IF_VR_Steam_GrabTypes startingGrabType = hand.GetGrabStarting();
            bool isGrabEnding = hand.IsGrabEnding(this.gameObject);

            if (interactable.attachedToHand == null && startingGrabType != IF_VR_Steam_GrabTypes.None)
            {
                // Save our position/rotation so that we can restore it when we detach
                oldPosition = transform.position;
                oldRotation = transform.rotation;

                // Call this to continue receiving HandHoverUpdate messages,
                // and prevent the hand from hovering over anything else
                hand.HoverLock(interactable);

                // Attach this object to the hand
                hand.AttachObject(gameObject, startingGrabType, attachmentFlags);
            }
            else if (isGrabEnding)
            {
                // Detach this object from the hand
                hand.DetachObject(gameObject);

                // Call this to undo HoverLock
                hand.HoverUnlock(interactable);

                // Restore position/rotation
                transform.position = oldPosition;
                transform.rotation = oldRotation;
            }
		}


		//-------------------------------------------------
		// Called when this GameObject becomes attached to the hand
		//-------------------------------------------------
		private void OnAttachedToHand( IF_VR_Steam_Hand hand )
        {
            generalText.text = string.Format("Attached: {0}", hand.name);
            attachTime = Time.time;
		}



		//-------------------------------------------------
		// Called when this GameObject is detached from the hand
		//-------------------------------------------------
		private void OnDetachedFromHand( IF_VR_Steam_Hand hand )
		{
            generalText.text = string.Format("Detached: {0}", hand.name);
		}


		//-------------------------------------------------
		// Called every Update() while this GameObject is attached to the hand
		//-------------------------------------------------
		private void HandAttachedUpdate( IF_VR_Steam_Hand hand )
		{
            generalText.text = string.Format("Attached: {0} :: Time: {1:F2}", hand.name, (Time.time - attachTime));
		}

        private bool lastHovering = false;
        private void Update()
        {
            if (interactable.isHovering != lastHovering) //save on the .tostrings a bit
            {
                hoveringText.text = string.Format("Hovering: {0}", interactable.isHovering);
                lastHovering = interactable.isHovering;
            }
        }


		//-------------------------------------------------
		// Called when this attached GameObject becomes the primary attached object
		//-------------------------------------------------
		private void OnHandFocusAcquired( IF_VR_Steam_Hand hand )
		{
		}


		//-------------------------------------------------
		// Called when another attached GameObject becomes the primary attached object
		//-------------------------------------------------
		private void OnHandFocusLost( IF_VR_Steam_Hand hand )
		{
		}
	}
}
