//======= Copyright (c) iNTERVR, All rights reserved. =========================
// Override InteractionSystem from SteamVR Unity Plugin of Valve Corporation
//
// Purpose: Custom Unity Events that take in additional parameters
//
//=============================================================================

using UnityEngine.Events;
using System;

namespace InterVR.IF.VR.Plugin.Steam.InteractionSystem
{
	//-------------------------------------------------------------------------
	public static class IF_VR_Steam_CustomEvents
	{
		//-------------------------------------------------
		[System.Serializable]
		public class IF_VR_Steam_UnityEventSingleFloat : UnityEvent<float>
		{
		}


		//-------------------------------------------------
		[System.Serializable]
		public class IF_VR_Steam_UnityEventHand : UnityEvent<IF_VR_Steam_Hand>
		{
		}
	}
}
