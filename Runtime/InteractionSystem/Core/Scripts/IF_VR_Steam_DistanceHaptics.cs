//======= Copyright (c) iNTERVR, All rights reserved. =========================
// Override InteractionSystem from SteamVR Unity Plugin of Valve Corporation
//
// Purpose: Triggers haptic pulses based on distance between 2 positions
//
//=============================================================================

using UnityEngine;
using System.Collections;

namespace InterVR.IF.VR.Plugin.Steam.InteractionSystem
{
	//-------------------------------------------------------------------------
	public class IF_VR_Steam_DistanceHaptics : MonoBehaviour
	{
		public Transform firstTransform;
		public Transform secondTransform;

		public AnimationCurve distanceIntensityCurve = AnimationCurve.Linear( 0.0f, 800.0f, 1.0f, 800.0f );
		public AnimationCurve pulseIntervalCurve = AnimationCurve.Linear( 0.0f, 0.01f, 1.0f, 0.0f );

		//-------------------------------------------------
		IEnumerator Start()
		{
			while ( true )
			{
				float distance = Vector3.Distance( firstTransform.position, secondTransform.position );

                IF_VR_Steam_Hand hand = GetComponentInParent<IF_VR_Steam_Hand>();
                if (hand != null)
                {
					float pulse = distanceIntensityCurve.Evaluate( distance );
                    hand.TriggerHapticPulse((ushort)pulse);

                    //SteamVR_Controller.Input( (int)trackedObject.index ).TriggerHapticPulse( (ushort)pulse );
				}

				float nextPulse = pulseIntervalCurve.Evaluate( distance );

				yield return new WaitForSeconds( nextPulse );
			}

		}
	}
}
