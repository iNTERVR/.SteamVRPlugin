//======= Copyright (c) iNTERVR, All rights reserved. =========================
// Override InteractionSystem from SteamVR Unity Plugin of Valve Corporation
//
// Purpose: Animator whose speed is set based on a linear mapping
//
//=============================================================================

using UnityEngine;
using System.Collections;

namespace InterVR.IF.VR.Plugin.Steam.InteractionSystem
{
	//-------------------------------------------------------------------------
	public class IF_VR_Steam_LinearAnimator : MonoBehaviour
	{
		public IF_VR_Steam_LinearMapping linearMapping;
		public Animator animator;

		private float currentLinearMapping = float.NaN;
		private int framesUnchanged = 0;


		//-------------------------------------------------
		void Awake()
		{
			if ( animator == null )
			{
				animator = GetComponent<Animator>();
			}

			animator.speed = 0.0f;

			if ( linearMapping == null )
			{
				linearMapping = GetComponent<IF_VR_Steam_LinearMapping>();
			}
		}


		//-------------------------------------------------
		void Update()
		{
			if ( currentLinearMapping != linearMapping.value )
			{
				currentLinearMapping = linearMapping.value;
				animator.enabled = true;
				animator.Play( 0, 0, currentLinearMapping );
				framesUnchanged = 0;
			}
			else
			{
				framesUnchanged++;
				if ( framesUnchanged > 2 )
				{
					animator.enabled = false;
				}
			}
		}
	}
}
