//======= Copyright (c) iNTERVR, All rights reserved. =========================
// Override InteractionSystem from SteamVR Unity Plugin of Valve Corporation
//
// Purpose: IF_VR_Steam_UIElement that responds to VR hands and generates UnityEvents
//
//=============================================================================

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;

namespace InterVR.IF.VR.Plugin.Steam.InteractionSystem
{
	//-------------------------------------------------------------------------
	[RequireComponent( typeof( IF_VR_Steam_Interactable ) )]
	public class IF_VR_Steam_UIElement : MonoBehaviour
	{
		public IF_VR_Steam_CustomEvents.IF_VR_Steam_UnityEventHand onHandClick;

        protected IF_VR_Steam_Hand currentHand;

		//-------------------------------------------------
		protected virtual void Awake()
		{
			Button button = GetComponent<Button>();
			if ( button )
			{
				button.onClick.AddListener( OnButtonClick );
			}
		}


		//-------------------------------------------------
		protected virtual void OnHandHoverBegin( IF_VR_Steam_Hand hand )
		{
			currentHand = hand;
			IF_VR_Steam_InputModule.instance.HoverBegin( gameObject );
			// ckbang - Hints
			//IF_VR_Steam_ControllerButtonHints.ShowButtonHint( hand, hand.uiInteractAction);
		}


        //-------------------------------------------------
        protected virtual void OnHandHoverEnd( IF_VR_Steam_Hand hand )
		{
			IF_VR_Steam_InputModule.instance.HoverEnd( gameObject );
			// ckbang - Hints
			//IF_VR_Steam_ControllerButtonHints.HideButtonHint( hand, hand.uiInteractAction);
			currentHand = null;
		}


        //-------------------------------------------------
        protected virtual void HandHoverUpdate( IF_VR_Steam_Hand hand )
		{
			if ( hand.uiInteractAction != null && hand.uiInteractAction.GetStateDown(hand.handType) )
			{
				IF_VR_Steam_InputModule.instance.Submit( gameObject );
				// ckbang - Hints
				//IF_VR_Steam_ControllerButtonHints.HideButtonHint( hand, hand.uiInteractAction);
			}
		}


        //-------------------------------------------------
        protected virtual void OnButtonClick()
		{
			onHandClick.Invoke( currentHand );
		}
	}

#if UNITY_EDITOR
	//-------------------------------------------------------------------------
	[UnityEditor.CustomEditor( typeof( IF_VR_Steam_UIElement ) )]
	public class UIElementEditor : UnityEditor.Editor
	{
		//-------------------------------------------------
		// Custom Inspector GUI allows us to click from within the UI
		//-------------------------------------------------
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			IF_VR_Steam_UIElement uiElement = (IF_VR_Steam_UIElement)target;
			if ( GUILayout.Button( "Click" ) )
			{
				IF_VR_Steam_InputModule.instance.Submit( uiElement.gameObject );
			}
		}
	}
#endif
}
