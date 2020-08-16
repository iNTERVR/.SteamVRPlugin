//======= Copyright (c) iNTERVR, All rights reserved. =========================
// Override InteractionSystem from SteamVR Unity Plugin of Valve Corporation
//
// Purpose: Spawns and attaches an object to the hand after the controller has
//			tracking
//
//=============================================================================

using UnityEngine;
using System.Collections;

namespace InterVR.IF.VR.Plugin.Steam.InteractionSystem
{
	//-------------------------------------------------------------------------
	public class IF_VR_Steam_SpawnAndAttachAfterControllerIsTracking : MonoBehaviour
	{
		private IF_VR_Steam_Hand hand;
		public GameObject itemPrefab;


		//-------------------------------------------------
		void Start()
		{
			hand = GetComponentInParent<IF_VR_Steam_Hand>();
		}


		//-------------------------------------------------
		void Update()
		{
			if ( itemPrefab != null )
			{
                if (hand.isActive && hand.isPoseValid)
                {
                    GameObject objectToAttach = GameObject.Instantiate(itemPrefab);
                    objectToAttach.SetActive(true);
                    hand.AttachObject(objectToAttach, IF_VR_Steam_GrabTypes.Scripted);
                    hand.TriggerHapticPulse(800);
                    Destroy(gameObject);

                    // If the player's scale has been changed the object to attach will be the wrong size.
                    // To fix this we change the object's scale back to its original, pre-attach scale.
                    objectToAttach.transform.localScale = itemPrefab.transform.localScale;
                }
			}
		}
	}
}
