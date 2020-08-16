//======= Copyright (c) iNTERVR, All rights reserved. =========================
// Override InteractionSystem from SteamVR Unity Plugin of Valve Corporation
//
// Purpose: Set the blend shape weight based on a linear mapping
//
//=============================================================================

using UnityEngine;
using System.Collections;

namespace InterVR.IF.VR.Plugin.Steam.InteractionSystem
{
	//-------------------------------------------------------------------------
	public class IF_VR_Steam_LinearBlendshape : MonoBehaviour
	{
		public IF_VR_Steam_LinearMapping linearMapping;
		public SkinnedMeshRenderer skinnedMesh;

		private float lastValue;


		//-------------------------------------------------
		void Awake()
		{
			if ( skinnedMesh == null )
			{
				skinnedMesh = GetComponent<SkinnedMeshRenderer>();
			}

			if ( linearMapping == null )
			{
				linearMapping = GetComponent<IF_VR_Steam_LinearMapping>();
			}
		}


		//-------------------------------------------------
		void Update()
		{
			float value = linearMapping.value;

			//No need to set the blend if our value hasn't changed.
			if ( value != lastValue )
			{
				float blendValue = IF_VR_Steam_Util.RemapNumberClamped( value, 0f, 1f, 1f, 100f );
				skinnedMesh.SetBlendShapeWeight( 0, blendValue );
			}

			lastValue = value;
		}
	}
}
