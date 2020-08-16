//======= Copyright (c) iNTERVR, All rights reserved. =========================
// Override InteractionSystem from SteamVR Unity Plugin of Valve Corporation
//
// Purpose: Keeps track of the ItemPackage this object is a part of
//
//=============================================================================

using UnityEngine;
using System.Collections;

namespace InterVR.IF.VR.Plugin.Steam.InteractionSystem
{
	//-------------------------------------------------------------------------
	public class IF_VR_Steam_ItemPackageReference : MonoBehaviour
	{
		public IF_VR_Steam_ItemPackage itemPackage;
	}
}
