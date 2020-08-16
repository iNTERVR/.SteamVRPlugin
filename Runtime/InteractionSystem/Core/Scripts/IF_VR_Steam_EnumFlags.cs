//======= Copyright (c) iNTERVR, All rights reserved. =========================
// Override InteractionSystem from SteamVR Unity Plugin of Valve Corporation
//
// Purpose: Allows Enums to be shown in the inspector as flags
//
//=============================================================================

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace InterVR.IF.VR.Plugin.Steam.InteractionSystem
{
	//-------------------------------------------------------------------------
	public class IF_VR_Steam_EnumFlags : PropertyAttribute
	{
		public IF_VR_Steam_EnumFlags() { }
	}


#if UNITY_EDITOR
	//-------------------------------------------------------------------------
	[CustomPropertyDrawer( typeof( IF_VR_Steam_EnumFlags ) )]
	public class IF_VR_Steam_EnumFlagsPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
		{
			property.intValue = EditorGUI.MaskField( position, label, property.intValue, property.enumNames );
		}
	}
#endif
}
