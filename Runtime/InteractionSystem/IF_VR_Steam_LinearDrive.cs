//======= Copyright (c) iNTERVR, All rights reserved. =========================
// Override InteractionSystem from SteamVR Unity Plugin of Valve Corporation
//
// Purpose: Drives a linear mapping based on position between 2 positions
//
//=============================================================================

using UnityEngine;
using System.Collections;

namespace InterVR.IF.VR.Plugin.Steam.InteractionSystem
{
	//-------------------------------------------------------------------------
	[RequireComponent( typeof( IF_VR_Steam_Interactable ) )]
	public class IF_VR_Steam_LinearDrive : MonoBehaviour
	{
		public Transform startPosition;
		public Transform endPosition;
		public IF_VR_Steam_LinearMapping linearMapping;
		public bool repositionGameObject = true;
		public bool maintainMomemntum = true;
		public float momemtumDampenRate = 5.0f;

        protected IF_VR_Steam_Hand.AttachmentFlags attachmentFlags = IF_VR_Steam_Hand.AttachmentFlags.DetachFromOtherHand;

        protected float initialMappingOffset;
        protected int numMappingChangeSamples = 5;
        protected float[] mappingChangeSamples;
        protected float prevMapping = 0.0f;
        protected float mappingChangeRate;
        protected int sampleCount = 0;

        protected IF_VR_Steam_Interactable interactable;


        protected virtual void Awake()
        {
            mappingChangeSamples = new float[numMappingChangeSamples];
            interactable = GetComponent<IF_VR_Steam_Interactable>();
        }

        protected virtual void Start()
		{
			if ( linearMapping == null )
			{
				linearMapping = GetComponent<IF_VR_Steam_LinearMapping>();
			}

			if ( linearMapping == null )
			{
				linearMapping = gameObject.AddComponent<IF_VR_Steam_LinearMapping>();
			}

            initialMappingOffset = linearMapping.value;

			if ( repositionGameObject )
			{
				UpdateLinearMapping( transform );
			}
		}

        protected virtual void HandHoverUpdate( IF_VR_Steam_Hand hand )
        {
            IF_VR_Steam_GrabTypes startingGrabType = hand.GetGrabStarting();

            if (interactable.attachedToHand == null && startingGrabType != IF_VR_Steam_GrabTypes.None)
            {
                initialMappingOffset = linearMapping.value - CalculateLinearMapping( hand.transform );
				sampleCount = 0;
				mappingChangeRate = 0.0f;

                hand.AttachObject(gameObject, startingGrabType, attachmentFlags);
            }
		}

        protected virtual void HandAttachedUpdate(IF_VR_Steam_Hand hand)
        {
            UpdateLinearMapping(hand.transform);

            if (hand.IsGrabEnding(this.gameObject))
            {
                hand.DetachObject(gameObject);
            }
        }

        protected virtual void OnDetachedFromHand(IF_VR_Steam_Hand hand)
        {
            CalculateMappingChangeRate();
        }


        protected void CalculateMappingChangeRate()
		{
			//Compute the mapping change rate
			mappingChangeRate = 0.0f;
			int mappingSamplesCount = Mathf.Min( sampleCount, mappingChangeSamples.Length );
			if ( mappingSamplesCount != 0 )
			{
				for ( int i = 0; i < mappingSamplesCount; ++i )
				{
					mappingChangeRate += mappingChangeSamples[i];
				}
				mappingChangeRate /= mappingSamplesCount;
			}
		}

        protected void UpdateLinearMapping( Transform updateTransform )
		{
			prevMapping = linearMapping.value;
			linearMapping.value = Mathf.Clamp01( initialMappingOffset + CalculateLinearMapping( updateTransform ) );

			mappingChangeSamples[sampleCount % mappingChangeSamples.Length] = ( 1.0f / Time.deltaTime ) * ( linearMapping.value - prevMapping );
			sampleCount++;

			if ( repositionGameObject )
			{
				transform.position = Vector3.Lerp( startPosition.position, endPosition.position, linearMapping.value );
			}
		}

        protected float CalculateLinearMapping( Transform updateTransform )
		{
			Vector3 direction = endPosition.position - startPosition.position;
			float length = direction.magnitude;
			direction.Normalize();

			Vector3 displacement = updateTransform.position - startPosition.position;

			return Vector3.Dot( displacement, direction ) / length;
		}


		protected virtual void Update()
        {
            if ( maintainMomemntum && mappingChangeRate != 0.0f )
			{
				//Dampen the mapping change rate and apply it to the mapping
				mappingChangeRate = Mathf.Lerp( mappingChangeRate, 0.0f, momemtumDampenRate * Time.deltaTime );
				linearMapping.value = Mathf.Clamp01( linearMapping.value + ( mappingChangeRate * Time.deltaTime ) );

				if ( repositionGameObject )
				{
					transform.position = Vector3.Lerp( startPosition.position, endPosition.position, linearMapping.value );
				}
			}
		}
	}
}
