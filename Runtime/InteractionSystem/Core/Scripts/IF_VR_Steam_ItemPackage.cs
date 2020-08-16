//======= Copyright (c) iNTERVR, All rights reserved. =========================
// Override InteractionSystem from SteamVR Unity Plugin of Valve Corporation
//
// Purpose: A package of items that can interact with the hands and be returned
//
//=============================================================================

using UnityEngine;
using System.Collections;

namespace InterVR.IF.VR.Plugin.Steam.InteractionSystem
{
	//-------------------------------------------------------------------------
	public class IF_VR_Steam_ItemPackage : MonoBehaviour
	{
		public enum ItemPackageType { Unrestricted, OneHanded, TwoHanded }

		public new string name;
		public ItemPackageType packageType = ItemPackageType.Unrestricted;
		public GameObject itemPrefab; // object to be spawned on tracked controller
		public GameObject otherHandItemPrefab; // object to be spawned in Other Hand
		public GameObject previewPrefab; // used to preview inputObject
		public GameObject fadedPreviewPrefab; // used to preview insubstantial inputObject
	}
}
