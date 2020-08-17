//======= Copyright (c) iNTERVR, All rights reserved. =========================
// Override InteractionSystem from SteamVR Unity Plugin of Valve Corporation
//
// Purpose: Triggers haptic pulses based on a linear mapping
//
//=============================================================================

using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace InterVR.IF.VR.Plugin.Steam.InteractionSystem
{
	//-------------------------------------------------------------------------
	[RequireComponent( typeof( IF_VR_Steam_Interactable ) )]
	public class IF_VR_Steam_HapticRack : MonoBehaviour
	{
		[Tooltip( "The linear mapping driving the haptic rack" )]
		public IF_VR_Steam_LinearMapping linearMapping;

		[Tooltip( "The number of haptic pulses evenly distributed along the mapping" )]
		public int teethCount = 128;

		[Tooltip( "Minimum duration of the haptic pulse" )]
		public int minimumPulseDuration = 500;

		[Tooltip( "Maximum duration of the haptic pulse" )]
		public int maximumPulseDuration = 900;

		[Tooltip( "This event is triggered every time a haptic pulse is made" )]
		public UnityEvent onPulse;

		private IF_VR_Steam_Hand hand;
		private int previousToothIndex = -1;

		//-------------------------------------------------
		void Awake()
		{
			if ( linearMapping == null )
			{
				linearMapping = GetComponent<IF_VR_Steam_LinearMapping>();
			}
		}


		//-------------------------------------------------
		private void OnHandHoverBegin( IF_VR_Steam_Hand hand )
		{
			this.hand = hand;
		}


		//-------------------------------------------------
		private void OnHandHoverEnd( IF_VR_Steam_Hand hand )
		{
			this.hand = null;
		}


		//-------------------------------------------------
		void Update()
		{
			int currentToothIndex = Mathf.RoundToInt( linearMapping.value * teethCount - 0.5f );
			if ( currentToothIndex != previousToothIndex )
			{
				Pulse();
				previousToothIndex = currentToothIndex;
			}
		}


		//-------------------------------------------------
		private void Pulse()
		{
			if ( hand && (hand.isActive) && ( hand.GetBestGrabbingType() != IF_VR_Steam_GrabTypes.None ) )
			{
				ushort duration = (ushort)Random.Range( minimumPulseDuration, maximumPulseDuration + 1 );
				hand.TriggerHapticPulse( duration );

				onPulse.Invoke();
			}
		}
	}
}
