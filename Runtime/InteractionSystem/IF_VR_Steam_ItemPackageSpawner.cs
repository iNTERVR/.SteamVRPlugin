//======= Copyright (c) iNTERVR, All rights reserved. =========================
// Override InteractionSystem from SteamVR Unity Plugin of Valve Corporation
//
// Purpose: Handles the spawning and returning of the IF_VR_Steam_ItemPackage
//
//=============================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace InterVR.IF.VR.Plugin.Steam.InteractionSystem
{
	//-------------------------------------------------------------------------
	[RequireComponent( typeof( IF_VR_Steam_Interactable ) )]
	public class ItemPackageSpawner : MonoBehaviour
	{
		public IF_VR_Steam_ItemPackage itemPackage
		{
			get
			{
				return _itemPackage;
			}
			set
			{
				CreatePreviewObject();
			}
		}

		public IF_VR_Steam_ItemPackage _itemPackage;

		public bool useItemPackagePreview = true;
        private bool useFadedPreview = false;
		private GameObject previewObject;

		public bool requireGrabActionToTake = false;
		public bool requireReleaseActionToReturn = false;
		public bool showTriggerHint = false;

		[IF_VR_Steam_EnumFlags]
		public IF_VR_Steam_Hand.AttachmentFlags attachmentFlags = IF_VR_Steam_Hand.defaultAttachmentFlags;

		public bool takeBackItem = false; // if a hand enters this trigger and has the item this spawner dispenses at the top of the stack, remove it from the stack

		public bool acceptDifferentItems = false;

		private GameObject spawnedItem;
		private bool itemIsSpawned = false;

		public UnityEvent pickupEvent;
		public UnityEvent dropEvent;

		public bool justPickedUpItem = false;


		//-------------------------------------------------
		private void CreatePreviewObject()
		{
			if ( !useItemPackagePreview )
			{
				return;
			}

			ClearPreview();

			if ( useItemPackagePreview )
			{
				if ( itemPackage == null )
				{
					return;
				}

				if ( useFadedPreview == false ) // if we don't have a spawned item out there, use the regular preview
				{
					if ( itemPackage.previewPrefab != null )
					{
						previewObject = Instantiate( itemPackage.previewPrefab, transform.position, Quaternion.identity ) as GameObject;
						previewObject.transform.parent = transform;
						previewObject.transform.localRotation = Quaternion.identity;
					}
				}
				else // there's a spawned item out there. Use the faded preview
				{
					if ( itemPackage.fadedPreviewPrefab != null )
					{
						previewObject = Instantiate( itemPackage.fadedPreviewPrefab, transform.position, Quaternion.identity ) as GameObject;
						previewObject.transform.parent = transform;
						previewObject.transform.localRotation = Quaternion.identity;
					}
				}
			}
		}


		//-------------------------------------------------
		void Start()
		{
			VerifyItemPackage();
		}


		//-------------------------------------------------
		private void VerifyItemPackage()
		{
			if ( itemPackage == null )
			{
				ItemPackageNotValid();
			}

			if ( itemPackage.itemPrefab == null )
			{
				ItemPackageNotValid();
			}
		}


		//-------------------------------------------------
		private void ItemPackageNotValid()
		{
			Debug.LogError("<b>[SteamVR Interaction]</b> IF_VR_Steam_ItemPackage assigned to " + gameObject.name + " is not valid. Destroying this game object.", this);
			Destroy( gameObject );
		}


		//-------------------------------------------------
		private void ClearPreview()
		{
			foreach ( Transform child in transform )
			{
				if ( Time.time > 0 )
				{
					GameObject.Destroy( child.gameObject );
				}
				else
				{
					GameObject.DestroyImmediate( child.gameObject );
				}
			}
		}


		//-------------------------------------------------
		void Update()
		{
			if ( ( itemIsSpawned == true ) && ( spawnedItem == null ) )
			{
				itemIsSpawned = false;
				useFadedPreview = false;
				dropEvent.Invoke();
				CreatePreviewObject();
			}
		}


		//-------------------------------------------------
		private void OnHandHoverBegin( IF_VR_Steam_Hand hand )
		{
			IF_VR_Steam_ItemPackage currentAttachedItemPackage = GetAttachedItemPackage( hand );

			if ( currentAttachedItemPackage == itemPackage ) // the item at the top of the hand's stack has an associated IF_VR_Steam_ItemPackage
			{
				if ( takeBackItem && !requireReleaseActionToReturn ) // if we want to take back matching items and aren't waiting for a trigger press
				{
					TakeBackItem( hand );
				}
			}

			if (!requireGrabActionToTake) // we don't require trigger press for pickup. Spawn and attach object.
			{
				SpawnAndAttachObject( hand, IF_VR_Steam_GrabTypes.Scripted );
			}

			if (requireGrabActionToTake && showTriggerHint )
			{
                hand.ShowGrabHint("PickUp");
			}
		}


		//-------------------------------------------------
		private void TakeBackItem( IF_VR_Steam_Hand hand )
		{
			RemoveMatchingItemsFromHandStack( itemPackage, hand );

			if ( itemPackage.packageType == IF_VR_Steam_ItemPackage.ItemPackageType.TwoHanded )
			{
				RemoveMatchingItemsFromHandStack( itemPackage, hand.otherHand );
			}
		}


		//-------------------------------------------------
		private IF_VR_Steam_ItemPackage GetAttachedItemPackage( IF_VR_Steam_Hand hand )
		{
			GameObject currentAttachedObject = hand.currentAttachedObject;

			if ( currentAttachedObject == null ) // verify the hand is holding something
			{
				return null;
			}

			IF_VR_Steam_ItemPackageReference packageReference = hand.currentAttachedObject.GetComponent<IF_VR_Steam_ItemPackageReference>();
			if ( packageReference == null ) // verify the item in the hand is matchable
			{
				return null;
			}

			IF_VR_Steam_ItemPackage attachedItemPackage = packageReference.itemPackage; // return the IF_VR_Steam_ItemPackage reference we find.

			return attachedItemPackage;
		}


		//-------------------------------------------------
		private void HandHoverUpdate( IF_VR_Steam_Hand hand )
		{
			if ( takeBackItem && requireReleaseActionToReturn )
			{
                if (hand.isActive)
				{
					IF_VR_Steam_ItemPackage currentAttachedItemPackage = GetAttachedItemPackage( hand );
                    if (currentAttachedItemPackage == itemPackage && hand.IsGrabEnding(currentAttachedItemPackage.gameObject))
					{
						TakeBackItem( hand );
						return; // So that we don't pick up an IF_VR_Steam_ItemPackage the same frame that we return it
					}
				}
			}

			if ( requireGrabActionToTake )
			{
                IF_VR_Steam_GrabTypes startingGrab = hand.GetGrabStarting();

				if (startingGrab != IF_VR_Steam_GrabTypes.None)
				{
					SpawnAndAttachObject( hand, IF_VR_Steam_GrabTypes.Scripted);
				}
			}
		}


		//-------------------------------------------------
		private void OnHandHoverEnd( IF_VR_Steam_Hand hand )
		{
			if ( !justPickedUpItem && requireGrabActionToTake && showTriggerHint )
			{
                hand.HideGrabHint();
			}

			justPickedUpItem = false;
		}


		//-------------------------------------------------
		private void RemoveMatchingItemsFromHandStack( IF_VR_Steam_ItemPackage package, IF_VR_Steam_Hand hand )
		{
            if (hand == null)
                return;

			for ( int i = 0; i < hand.AttachedObjects.Count; i++ )
			{
				IF_VR_Steam_ItemPackageReference packageReference = hand.AttachedObjects[i].attachedObject.GetComponent<IF_VR_Steam_ItemPackageReference>();
				if ( packageReference != null )
				{
					IF_VR_Steam_ItemPackage attachedObjectItemPackage = packageReference.itemPackage;
					if ( ( attachedObjectItemPackage != null ) && ( attachedObjectItemPackage == package ) )
					{
						GameObject detachedItem = hand.AttachedObjects[i].attachedObject;
						hand.DetachObject( detachedItem );
					}
				}
			}
		}


		//-------------------------------------------------
		private void RemoveMatchingItemTypesFromHand( IF_VR_Steam_ItemPackage.ItemPackageType packageType, IF_VR_Steam_Hand hand )
		{
			for ( int i = 0; i < hand.AttachedObjects.Count; i++ )
			{
				IF_VR_Steam_ItemPackageReference packageReference = hand.AttachedObjects[i].attachedObject.GetComponent<IF_VR_Steam_ItemPackageReference>();
				if ( packageReference != null )
				{
					if ( packageReference.itemPackage.packageType == packageType )
					{
						GameObject detachedItem = hand.AttachedObjects[i].attachedObject;
						hand.DetachObject( detachedItem );
					}
				}
			}
		}


		//-------------------------------------------------
		private void SpawnAndAttachObject( IF_VR_Steam_Hand hand, IF_VR_Steam_GrabTypes grabType )
		{
			if ( hand.otherHand != null )
			{
				//If the other hand has this item package, take it back from the other hand
				IF_VR_Steam_ItemPackage otherHandItemPackage = GetAttachedItemPackage( hand.otherHand );
				if ( otherHandItemPackage == itemPackage )
				{
					TakeBackItem( hand.otherHand );
				}
			}

			if ( showTriggerHint )
			{
                hand.HideGrabHint();
			}

			if ( itemPackage.otherHandItemPrefab != null )
			{
				if ( hand.otherHand.hoverLocked )
				{
                    Debug.Log( "<b>[SteamVR Interaction]</b> Not attaching objects because other hand is hoverlocked and we can't deliver both items." );
                    return;
				}
			}

			// if we're trying to spawn a one-handed item, remove one and two-handed items from this hand and two-handed items from both hands
			if ( itemPackage.packageType == IF_VR_Steam_ItemPackage.ItemPackageType.OneHanded )
			{
				RemoveMatchingItemTypesFromHand( IF_VR_Steam_ItemPackage.ItemPackageType.OneHanded, hand );
				RemoveMatchingItemTypesFromHand( IF_VR_Steam_ItemPackage.ItemPackageType.TwoHanded, hand );
				RemoveMatchingItemTypesFromHand( IF_VR_Steam_ItemPackage.ItemPackageType.TwoHanded, hand.otherHand );
			}

			// if we're trying to spawn a two-handed item, remove one and two-handed items from both hands
			if ( itemPackage.packageType == IF_VR_Steam_ItemPackage.ItemPackageType.TwoHanded )
			{
				RemoveMatchingItemTypesFromHand( IF_VR_Steam_ItemPackage.ItemPackageType.OneHanded, hand );
				RemoveMatchingItemTypesFromHand( IF_VR_Steam_ItemPackage.ItemPackageType.OneHanded, hand.otherHand );
				RemoveMatchingItemTypesFromHand( IF_VR_Steam_ItemPackage.ItemPackageType.TwoHanded, hand );
				RemoveMatchingItemTypesFromHand( IF_VR_Steam_ItemPackage.ItemPackageType.TwoHanded, hand.otherHand );
			}

			spawnedItem = GameObject.Instantiate( itemPackage.itemPrefab );
			spawnedItem.SetActive( true );
			hand.AttachObject( spawnedItem, grabType, attachmentFlags );

			if ( ( itemPackage.otherHandItemPrefab != null ) && ( hand.otherHand.isActive ) )
			{
				GameObject otherHandObjectToAttach = GameObject.Instantiate( itemPackage.otherHandItemPrefab );
				otherHandObjectToAttach.SetActive( true );
				hand.otherHand.AttachObject( otherHandObjectToAttach, grabType, attachmentFlags );
			}

			itemIsSpawned = true;

			justPickedUpItem = true;

			if ( takeBackItem )
			{
				useFadedPreview = true;
				pickupEvent.Invoke();
				CreatePreviewObject();
			}
		}
	}
}
