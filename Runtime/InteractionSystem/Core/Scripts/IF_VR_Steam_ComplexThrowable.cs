//======= Copyright (c) iNTERVR, All rights reserved. =========================
// Override InteractionSystem from SteamVR Unity Plugin of Valve Corporation
//
// Purpose: Throwable that uses physics joints to attach instead of just
//			parenting
//
//=============================================================================

using UnityEngine;
using System.Collections.Generic;

namespace InterVR.IF.VR.Plugin.Steam.InteractionSystem
{
	//-------------------------------------------------------------------------
	[RequireComponent( typeof( IF_VR_Steam_Interactable ) )]
	public class IF_VR_Steam_ComplexThrowable : MonoBehaviour
	{
		public enum AttachMode
		{
			FixedJoint,
			Force,
		}

		public float attachForce = 800.0f;
		public float attachForceDamper = 25.0f;

		public AttachMode attachMode = AttachMode.FixedJoint;

		[IF_VR_Steam_EnumFlags]
		public IF_VR_Steam_Hand.AttachmentFlags attachmentFlags = 0;

		private List<IF_VR_Steam_Hand> holdingHands = new List<IF_VR_Steam_Hand>();
		private List<Rigidbody> holdingBodies = new List<Rigidbody>();
		private List<Vector3> holdingPoints = new List<Vector3>();

		private List<Rigidbody> rigidBodies = new List<Rigidbody>();

		//-------------------------------------------------
		void Awake()
		{
			GetComponentsInChildren<Rigidbody>( rigidBodies );
		}


		//-------------------------------------------------
		void Update()
		{
			for ( int i = 0; i < holdingHands.Count; i++ )
			{
                if (holdingHands[i].IsGrabEnding(this.gameObject))
                {
					PhysicsDetach( holdingHands[i] );
				}
			}
		}


		//-------------------------------------------------
		private void OnHandHoverBegin( IF_VR_Steam_Hand hand )
		{
			if ( holdingHands.IndexOf( hand ) == -1 )
			{
				if ( hand.isActive )
				{
					hand.TriggerHapticPulse( 800 );
				}
			}
		}


		//-------------------------------------------------
		private void OnHandHoverEnd( IF_VR_Steam_Hand hand )
		{
			if ( holdingHands.IndexOf( hand ) == -1 )
			{
				if (hand.isActive)
				{
					hand.TriggerHapticPulse( 500 );
				}
			}
		}


		//-------------------------------------------------
		private void HandHoverUpdate( IF_VR_Steam_Hand hand )
		{
            IF_VR_Steam_GrabTypes startingGrabType = hand.GetGrabStarting();

            if (startingGrabType != IF_VR_Steam_GrabTypes.None)
			{
				PhysicsAttach( hand, startingGrabType );
			}
		}


		//-------------------------------------------------
		private void PhysicsAttach( IF_VR_Steam_Hand hand, IF_VR_Steam_GrabTypes startingGrabType )
		{
			PhysicsDetach( hand );

			Rigidbody holdingBody = null;
			Vector3 holdingPoint = Vector3.zero;

			// The hand should grab onto the nearest rigid body
			float closestDistance = float.MaxValue;
			for ( int i = 0; i < rigidBodies.Count; i++ )
			{
				float distance = Vector3.Distance( rigidBodies[i].worldCenterOfMass, hand.transform.position );
				if ( distance < closestDistance )
				{
					holdingBody = rigidBodies[i];
					closestDistance = distance;
				}
			}

			// Couldn't grab onto a body
			if ( holdingBody == null )
				return;

			// Create a fixed joint from the hand to the holding body
			if ( attachMode == AttachMode.FixedJoint )
			{
				Rigidbody handRigidbody = IF_VR_Steam_Util.FindOrAddComponent<Rigidbody>( hand.gameObject );
				handRigidbody.isKinematic = true;

				FixedJoint handJoint = hand.gameObject.AddComponent<FixedJoint>();
				handJoint.connectedBody = holdingBody;
			}

			// Don't let the hand interact with other things while it's holding us
			hand.HoverLock( null );

			// Affix this point
			Vector3 offset = hand.transform.position - holdingBody.worldCenterOfMass;
			offset = Mathf.Min( offset.magnitude, 1.0f ) * offset.normalized;
			holdingPoint = holdingBody.transform.InverseTransformPoint( holdingBody.worldCenterOfMass + offset );

			hand.AttachObject( this.gameObject, startingGrabType, attachmentFlags );

			// Update holding list
			holdingHands.Add( hand );
			holdingBodies.Add( holdingBody );
			holdingPoints.Add( holdingPoint );
		}


		//-------------------------------------------------
		private bool PhysicsDetach( IF_VR_Steam_Hand hand )
		{
			int i = holdingHands.IndexOf( hand );

			if ( i != -1 )
			{
				// Detach this object from the hand
				holdingHands[i].DetachObject( this.gameObject, false );

				// Allow the hand to do other things
				holdingHands[i].HoverUnlock( null );

				// Delete any existing joints from the hand
				if ( attachMode == AttachMode.FixedJoint )
				{
					Destroy( holdingHands[i].GetComponent<FixedJoint>() );
				}

				IF_VR_Steam_Util.FastRemove( holdingHands, i );
				IF_VR_Steam_Util.FastRemove( holdingBodies, i );
				IF_VR_Steam_Util.FastRemove( holdingPoints, i );

				return true;
			}

			return false;
		}


		//-------------------------------------------------
		void FixedUpdate()
		{
			if ( attachMode == AttachMode.Force )
			{
				for ( int i = 0; i < holdingHands.Count; i++ )
				{
					Vector3 targetPoint = holdingBodies[i].transform.TransformPoint( holdingPoints[i] );
					Vector3 vdisplacement = holdingHands[i].transform.position - targetPoint;

					holdingBodies[i].AddForceAtPosition( attachForce * vdisplacement, targetPoint, ForceMode.Acceleration );
					holdingBodies[i].AddForceAtPosition( -attachForceDamper * holdingBodies[i].GetPointVelocity( targetPoint ), targetPoint, ForceMode.Acceleration );
				}
			}
		}
	}
}
