//======= Copyright (c) iNTERVR, All rights reserved. =========================
// Override InteractionSystem from SteamVR Unity Plugin of Valve Corporation
//
// Purpose: Collider dangling from the player's head
//
//=============================================================================

using UnityEngine;
using System.Collections;

namespace InterVR.IF.VR.Plugin.Steam.InteractionSystem
{
	//-------------------------------------------------------------------------
	[RequireComponent( typeof( CapsuleCollider ) )]
	public class IF_VR_Steam_BodyCollider : MonoBehaviour
	{
		public Transform head;

		private CapsuleCollider capsuleCollider;

		//-------------------------------------------------
		void Awake()
		{
			capsuleCollider = GetComponent<CapsuleCollider>();
		}


		//-------------------------------------------------
		void FixedUpdate()
		{
			float distanceFromFloor = Vector3.Dot( head.localPosition, Vector3.up );
			capsuleCollider.height = Mathf.Max( capsuleCollider.radius, distanceFromFloor );
			transform.localPosition = head.localPosition - 0.5f * distanceFromFloor * Vector3.up;
		}
	}
}
