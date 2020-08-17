//======= Copyright (c) iNTERVR, All rights reserved. =========================
// Override InteractionSystem from SteamVR Unity Plugin of Valve Corporation
//
// Purpose: Creates an object and attaches it to the hand
//
//=============================================================================

using UnityEngine;
using System.Collections;

namespace InterVR.IF.VR.Plugin.Steam.InteractionSystem
{
	//-------------------------------------------------------------------------
	public class IF_VR_Steam_SpawnAndAttachToHand : MonoBehaviour
	{
		public IF_VR_Steam_Hand hand;
		public GameObject prefab;


		//-------------------------------------------------
		public void SpawnAndAttach( IF_VR_Steam_Hand passedInhand )
		{
			IF_VR_Steam_Hand handToUse = passedInhand;
			if ( passedInhand == null )
			{
				handToUse = hand;
			}

			if ( handToUse == null )
			{
				return;
			}

			GameObject prefabObject = Instantiate( prefab ) as GameObject;
			handToUse.AttachObject( prefabObject, IF_VR_Steam_GrabTypes.Scripted );
		}
	}
}
