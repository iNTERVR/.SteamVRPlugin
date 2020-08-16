//======= Copyright (c) iNTERVR, All rights reserved. =========================
// Override InteractionSystem from SteamVR Unity Plugin of Valve Corporation
//
// Purpose: Destroys this object when its particle system dies
//
//=============================================================================

using UnityEngine;
using System.Collections;

namespace InterVR.IF.VR.Plugin.Steam.InteractionSystem
{
	//-------------------------------------------------------------------------
	[RequireComponent( typeof( ParticleSystem ) )]
	public class IF_VR_Steam_DestroyOnParticleSystemDeath : MonoBehaviour
	{
		private ParticleSystem particles;

		//-------------------------------------------------
		void Awake()
		{
			particles = GetComponent<ParticleSystem>();

			InvokeRepeating( "CheckParticleSystem", 0.1f, 0.1f );
		}


		//-------------------------------------------------
		private void CheckParticleSystem()
		{
			if ( !particles.IsAlive() )
			{
				Destroy( this.gameObject );
			}
		}
	}
}
