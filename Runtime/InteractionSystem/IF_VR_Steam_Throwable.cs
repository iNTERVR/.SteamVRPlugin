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
    //-------------------------------------------------------------------------
    [RequireComponent( typeof( IF_VR_Steam_Interactable ) )]
	[RequireComponent( typeof( Rigidbody ) )]
	public class IF_VR_Steam_Throwable : MonoBehaviour
	{
		[IF_VR_Steam_EnumFlags]
		[Tooltip( "The flags used to attach this object to the hand." )]
		public IF_VR_Steam_Hand.AttachmentFlags attachmentFlags = IF_VR_Steam_Hand.AttachmentFlags.ParentToHand | IF_VR_Steam_Hand.AttachmentFlags.DetachFromOtherHand | IF_VR_Steam_Hand.AttachmentFlags.TurnOnKinematic;

        [Tooltip("The local point which acts as a positional and rotational offset to use while held")]
        public Transform attachmentOffset;

		[Tooltip( "How fast must this object be moving to attach due to a trigger hold instead of a trigger press? (-1 to disable)" )]
        public float catchingSpeedThreshold = -1;

        public ReleaseStyle releaseVelocityStyle = ReleaseStyle.GetFromHand;

        [Tooltip("The time offset used when releasing the object with the RawFromHand option")]
        public float releaseVelocityTimeOffset = -0.011f;

        public float scaleReleaseVelocity = 1.1f;

        [Tooltip("The release velocity magnitude representing the end of the scale release velocity curve. (-1 to disable)")]
        public float scaleReleaseVelocityThreshold = -1.0f;
        [Tooltip("Use this curve to ease into the scaled release velocity based on the magnitude of the measured release velocity. This allows greater differentiation between a drop, toss, and throw.")]
        public AnimationCurve scaleReleaseVelocityCurve = AnimationCurve.EaseInOut(0.0f, 0.1f, 1.0f, 1.0f);

        [Tooltip( "When detaching the object, should it return to its original parent?" )]
		public bool restoreOriginalParent = false;



		protected IF_VR_Steam_VelocityEstimator velocityEstimator;
        protected bool attached = false;
        protected float attachTime;
        protected Vector3 attachPosition;
        protected Quaternion attachRotation;
        protected Transform attachEaseInTransform;

		public UnityEvent onPickUp;
        public UnityEvent onDetachFromHand;
        public IF_VR_Steam_HandEvent onHeldUpdate;


        protected RigidbodyInterpolation hadInterpolation = RigidbodyInterpolation.None;

        protected new Rigidbody rigidbody;

        [HideInInspector]
        public IF_VR_Steam_Interactable interactable;


        //-------------------------------------------------
        protected virtual void Awake()
		{
			velocityEstimator = GetComponent<IF_VR_Steam_VelocityEstimator>();
            interactable = GetComponent<IF_VR_Steam_Interactable>();



            rigidbody = GetComponent<Rigidbody>();
            rigidbody.maxAngularVelocity = 50.0f;


            if(attachmentOffset != null)
            {
                // remove?
                //interactable.handFollowTransform = attachmentOffset;
            }

		}


        //-------------------------------------------------
        protected virtual void OnHandHoverBegin( IF_VR_Steam_Hand hand )
		{
			bool showHint = false;

            // "Catch" the throwable by holding down the interaction button instead of pressing it.
            // Only do this if the throwable is moving faster than the prescribed threshold speed,
            // and if it isn't attached to another hand
            if ( !attached && catchingSpeedThreshold != -1)
            {
                float catchingThreshold = catchingSpeedThreshold * SteamVR_Utils.GetLossyScale(IF_VR_Steam_Player.instance.trackingOriginTransform);

                IF_VR_Steam_GrabTypes bestGrabType = hand.GetBestGrabbingType();

                if ( bestGrabType != IF_VR_Steam_GrabTypes.None )
				{
					if (rigidbody.velocity.magnitude >= catchingThreshold)
					{
						hand.AttachObject( gameObject, bestGrabType, attachmentFlags );
						showHint = false;
					}
				}
			}

			if ( showHint )
			{
                hand.ShowGrabHint();
			}
		}


        //-------------------------------------------------
        protected virtual void OnHandHoverEnd( IF_VR_Steam_Hand hand )
		{
            hand.HideGrabHint();
		}


        //-------------------------------------------------
        protected virtual void HandHoverUpdate( IF_VR_Steam_Hand hand )
        {
            IF_VR_Steam_GrabTypes startingGrabType = hand.GetGrabStarting();

            if (startingGrabType != IF_VR_Steam_GrabTypes.None)
            {
				hand.AttachObject( gameObject, startingGrabType, attachmentFlags, attachmentOffset );
                hand.HideGrabHint();
            }
		}

        //-------------------------------------------------
        protected virtual void OnAttachedToHand( IF_VR_Steam_Hand hand )
		{
            //Debug.Log("<b>[SteamVR Interaction]</b> Pickup: " + hand.GetGrabStarting().ToString());

            hadInterpolation = this.rigidbody.interpolation;

            attached = true;

			onPickUp.Invoke();

			hand.HoverLock( null );

            rigidbody.interpolation = RigidbodyInterpolation.None;

            if (velocityEstimator != null)
		        velocityEstimator.BeginEstimatingVelocity();

			attachTime = Time.time;
			attachPosition = transform.position;
			attachRotation = transform.rotation;

		}


        //-------------------------------------------------
        protected virtual void OnDetachedFromHand(IF_VR_Steam_Hand hand)
        {
            attached = false;

            onDetachFromHand.Invoke();

            hand.HoverUnlock(null);

            rigidbody.interpolation = hadInterpolation;

            Vector3 velocity;
            Vector3 angularVelocity;

            GetReleaseVelocities(hand, out velocity, out angularVelocity);

            rigidbody.velocity = velocity;
            rigidbody.angularVelocity = angularVelocity;
        }


        public virtual void GetReleaseVelocities(IF_VR_Steam_Hand hand, out Vector3 velocity, out Vector3 angularVelocity)
        {
            if (hand.noSteamVRFallbackCamera && releaseVelocityStyle != ReleaseStyle.NoChange)
                releaseVelocityStyle = ReleaseStyle.ShortEstimation; // only type that works with fallback hand is short estimation.

            switch (releaseVelocityStyle)
            {
                case ReleaseStyle.ShortEstimation:
                    if (velocityEstimator != null)
                    {
                        velocityEstimator.FinishEstimatingVelocity();
                        velocity = velocityEstimator.GetVelocityEstimate();
                        angularVelocity = velocityEstimator.GetAngularVelocityEstimate();
                    }
                    else
                    {
                        Debug.LogWarning("[SteamVR Interaction System] Throwable: No Velocity Estimator component on object but release style set to short estimation. Please add one or change the release style.");

                        velocity = rigidbody.velocity;
                        angularVelocity = rigidbody.angularVelocity;
                    }
                    break;
                case ReleaseStyle.AdvancedEstimation:
                    hand.GetEstimatedPeakVelocities(out velocity, out angularVelocity);
                    break;
                case ReleaseStyle.GetFromHand:
                    velocity = hand.GetTrackedObjectVelocity(releaseVelocityTimeOffset);
                    angularVelocity = hand.GetTrackedObjectAngularVelocity(releaseVelocityTimeOffset);
                    break;
                default:
                case ReleaseStyle.NoChange:
                    velocity = rigidbody.velocity;
                    angularVelocity = rigidbody.angularVelocity;
                    break;
            }

            if (releaseVelocityStyle != ReleaseStyle.NoChange)
            {
                    float scaleFactor = 1.0f;
                    if (scaleReleaseVelocityThreshold > 0)
                    {
                        scaleFactor = Mathf.Clamp01(scaleReleaseVelocityCurve.Evaluate(velocity.magnitude / scaleReleaseVelocityThreshold));
                    }

                    velocity *= (scaleFactor * scaleReleaseVelocity);
            }
        }

        //-------------------------------------------------
        protected virtual void HandAttachedUpdate(IF_VR_Steam_Hand hand)
        {


            if (hand.IsGrabEnding(this.gameObject))
            {
                hand.DetachObject(gameObject, restoreOriginalParent);

                // Uncomment to detach ourselves late in the frame.
                // This is so that any vehicles the player is attached to
                // have a chance to finish updating themselves.
                // If we detach now, our position could be behind what it
                // will be at the end of the frame, and the object may appear
                // to teleport behind the hand when the player releases it.
                //StartCoroutine( LateDetach( hand ) );
            }

            if (onHeldUpdate != null)
                onHeldUpdate.Invoke(hand);
        }


        //-------------------------------------------------
        protected virtual IEnumerator LateDetach( IF_VR_Steam_Hand hand )
		{
			yield return new WaitForEndOfFrame();

			hand.DetachObject( gameObject, restoreOriginalParent );
		}


        //-------------------------------------------------
        protected virtual void OnHandFocusAcquired( IF_VR_Steam_Hand hand )
		{
			gameObject.SetActive( true );

            if (velocityEstimator != null)
			    velocityEstimator.BeginEstimatingVelocity();
		}


        //-------------------------------------------------
        protected virtual void OnHandFocusLost( IF_VR_Steam_Hand hand )
		{
			gameObject.SetActive( false );

            if (velocityEstimator != null)
                velocityEstimator.FinishEstimatingVelocity();
		}
	}

    public enum ReleaseStyle
    {
        NoChange,
        GetFromHand,
        ShortEstimation,
        AdvancedEstimation,
    }
}
