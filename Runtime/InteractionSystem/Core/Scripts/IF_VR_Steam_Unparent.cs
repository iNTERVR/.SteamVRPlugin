//======= Copyright (c) iNTERVR, All rights reserved. =========================
// Override InteractionSystem from SteamVR Unity Plugin of Valve Corporation
//
// Purpose: Unparents an object and keeps track of the old parent
//
//=============================================================================

using UnityEngine;
using System.Collections;

namespace InterVR.IF.VR.Plugin.Steam.InteractionSystem
{
	//-------------------------------------------------------------------------
	public class IF_VR_Steam_Unparent : MonoBehaviour
	{
		Transform oldParent;

		//-------------------------------------------------
		void Start()
		{
			oldParent = transform.parent;
			transform.parent = null;
			gameObject.name = oldParent.gameObject.name + "." + gameObject.name;
		}


		//-------------------------------------------------
		void Update()
		{
			if ( oldParent == null )
				Object.Destroy( gameObject );
		}


		//-------------------------------------------------
		public Transform GetOldParent()
		{
			return oldParent;
		}
	}
}
