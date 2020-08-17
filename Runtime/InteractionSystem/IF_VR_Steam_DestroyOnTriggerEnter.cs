//======= Copyright (c) iNTERVR, All rights reserved. =========================
// Override InteractionSystem from SteamVR Unity Plugin of Valve Corporation
//
// Purpose: Destroys this object when it enters a trigger
//
//=============================================================================

using UnityEngine;
using System.Collections;

namespace InterVR.IF.VR.Plugin.Steam.InteractionSystem
{
	//-------------------------------------------------------------------------
	public class IF_VR_Steam_DestroyOnTriggerEnter : MonoBehaviour
	{
		public string tagFilter;

		private bool useTag;

		//-------------------------------------------------
		void Start()
		{
			if ( !string.IsNullOrEmpty( tagFilter ) )
			{
				useTag = true;
			}
		}


		//-------------------------------------------------
		void OnTriggerEnter( Collider collider )
		{
			if ( !useTag || ( useTag && collider.gameObject.tag == tagFilter ) )
			{
				Destroy( collider.gameObject.transform.root.gameObject );
			}
		}
	}
}
