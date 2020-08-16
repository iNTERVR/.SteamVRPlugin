//======= Copyright (c) iNTERVR, All rights reserved. =========================
// Override InteractionSystem from SteamVR Unity Plugin of Valve Corporation
//
// Purpose: Play one-shot sounds as opposed to continuos/looping ones
//
//=============================================================================

using UnityEngine;
using System.Collections;

namespace InterVR.IF.VR.Plugin.Steam.InteractionSystem
{
	//-------------------------------------------------------------------------
	public class IF_VR_Steam_SoundPlayOneshot : MonoBehaviour
	{
		public AudioClip[] waveFiles;
		private AudioSource thisAudioSource;

		public float volMin;
		public float volMax;

		public float pitchMin;
		public float pitchMax;

		public bool playOnAwake;


		//-------------------------------------------------
		void Awake()
		{
			thisAudioSource = GetComponent<AudioSource>();

			if ( playOnAwake )
			{
				Play();
			}
		}


		//-------------------------------------------------
		public void Play()
		{
			if ( thisAudioSource != null && thisAudioSource.isActiveAndEnabled && !IF_VR_Steam_Util.IsNullOrEmpty( waveFiles ) )
			{
				//randomly apply a volume between the volume min max
				thisAudioSource.volume = Random.Range( volMin, volMax );

				//randomly apply a pitch between the pitch min max
				thisAudioSource.pitch = Random.Range( pitchMin, pitchMax );

				// play the sound
				thisAudioSource.PlayOneShot( waveFiles[Random.Range( 0, waveFiles.Length )] );
			}
		}


		//-------------------------------------------------
		public void Pause()
		{
			if ( thisAudioSource != null )
			{
				thisAudioSource.Pause();
			}
		}


		//-------------------------------------------------
		public void UnPause()
		{
			if ( thisAudioSource != null )
			{
				thisAudioSource.UnPause();
			}
		}
	}
}
