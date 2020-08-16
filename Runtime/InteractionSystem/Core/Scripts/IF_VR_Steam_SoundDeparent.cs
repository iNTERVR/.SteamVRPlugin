//======= Copyright (c) iNTERVR, All rights reserved. =========================
// Override InteractionSystem from SteamVR Unity Plugin of Valve Corporation
//
// Purpose: Unparents this object and optionally destroys it after the sound
//			has played
//
//=============================================================================

using UnityEngine;
using System.Collections;

namespace InterVR.IF.VR.Plugin.Steam.InteractionSystem
{
	//-------------------------------------------------------------------------
	public class IF_VR_Steam_SoundDeparent : MonoBehaviour
	{
		public bool destroyAfterPlayOnce = true;
		private AudioSource thisAudioSource;


		//-------------------------------------------------
		void Awake()
		{
			thisAudioSource = GetComponent<AudioSource>();

		}


		//-------------------------------------------------
		void Start()
		{
			// move the sound object out from under the parent
			gameObject.transform.parent = null;

			if ( destroyAfterPlayOnce )
				Destroy( gameObject, thisAudioSource.clip.length );
		}
	}
}
