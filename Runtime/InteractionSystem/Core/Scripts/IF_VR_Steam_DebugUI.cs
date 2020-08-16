//======= Copyright (c) iNTERVR, All rights reserved. =========================
// Override InteractionSystem from SteamVR Unity Plugin of Valve Corporation
//
// Purpose: Debug UI shown for the player
//
//=============================================================================

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace InterVR.IF.VR.Plugin.Steam.InteractionSystem
{
	//-------------------------------------------------------------------------
	public class IF_VR_Steam_DebugUI : MonoBehaviour
	{
		private IF_VR_Steam_Player player;

		//-------------------------------------------------
		static private IF_VR_Steam_DebugUI _instance;
		static public IF_VR_Steam_DebugUI instance
		{
			get
			{
				if ( _instance == null )
				{
					_instance = GameObject.FindObjectOfType<IF_VR_Steam_DebugUI>();
				}
				return _instance;
			}
		}


		//-------------------------------------------------
		void Start()
		{
			player = IF_VR_Steam_Player.instance;
		}


#if !HIDE_DEBUG_UI
        //-------------------------------------------------
        private void OnGUI()
		{
            if (Debug.isDebugBuild)
            {
                player.Draw2DDebug();
            }
        }
#endif
    }
}
