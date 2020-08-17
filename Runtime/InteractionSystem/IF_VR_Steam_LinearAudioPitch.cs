//======= Copyright (c) iNTERVR, All rights reserved. =========================
// Override InteractionSystem from SteamVR Unity Plugin of Valve Corporation
//
// Purpose: Changes the pitch of this audio source based on a linear mapping
//			and a curve
//
//=============================================================================

using UnityEngine;
using System.Collections;

namespace InterVR.IF.VR.Plugin.Steam.InteractionSystem
{
	//-------------------------------------------------------------------------
	public class IF_VR_Steam_LinearAudioPitch : MonoBehaviour
	{
		public IF_VR_Steam_LinearMapping linearMapping;
		public AnimationCurve pitchCurve;
		public float minPitch;
		public float maxPitch;
		public bool applyContinuously = true;

		private AudioSource audioSource;


		//-------------------------------------------------
		void Awake()
		{
			if ( audioSource == null )
			{
				audioSource = GetComponent<AudioSource>();
			}

			if ( linearMapping == null )
			{
				linearMapping = GetComponent<IF_VR_Steam_LinearMapping>();
			}
		}


		//-------------------------------------------------
		void Update()
		{
			if ( applyContinuously )
			{
				Apply();
			}
		}


		//-------------------------------------------------
		private void Apply()
		{
			float y = pitchCurve.Evaluate( linearMapping.value );

			audioSource.pitch = Mathf.Lerp( minPitch, maxPitch, y );
		}
	}
}
