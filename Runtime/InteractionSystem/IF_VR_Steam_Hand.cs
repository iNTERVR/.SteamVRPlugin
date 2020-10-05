﻿//======= Copyright (c) iNTERVR, All rights reserved. =========================
// Override InteractionSystem from SteamVR Unity Plugin of Valve Corporation
//
// Purpose: The hands used by the player in the vr interaction system
//
//=============================================================================

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine.Events;
using Valve.VR;
using EcsRx.Zenject;
using EcsRx.Infrastructure.Extensions;
using InterVR.IF.VR.Plugin.Steam.Modules;
using InterVR.IF.VR.Modules;
using InterVR.IF.Extensions;
using InterVR.IF.VR.Components;
using InterVR.IF.VR.Plugin.Steam.Extensions;
using EcsRx.Extensions;

namespace InterVR.IF.VR.Plugin.Steam.InteractionSystem
{
    //-------------------------------------------------------------------------
    // Links with an appropriate SteamVR controller and facilitates
    // interactions with objects in the virtual world.
    //-------------------------------------------------------------------------
    public class IF_VR_Steam_Hand : MonoBehaviour
    {
        // The flags used to determine how an object is attached to the hand.
        [Flags]
        public enum AttachmentFlags
        {
            SnapOnAttach = 1 << 0, // The object should snap to the position of the specified attachment point on the hand.
            DetachOthers = 1 << 1, // Other objects attached to this hand will be detached.
            DetachFromOtherHand = 1 << 2, // This object will be detached from the other hand.
            ParentToHand = 1 << 3, // The object will be parented to the hand.
            VelocityMovement = 1 << 4, // The object will attempt to move to match the position and rotation of the hand.
            TurnOnKinematic = 1 << 5, // The object will not respond to external physics.
            TurnOffGravity = 1 << 6, // The object will not respond to external physics.
            AllowSidegrade = 1 << 7, // The object is able to switch from a pinch grab to a grip grab. Decreases likelyhood of a good throw but also decreases likelyhood of accidental drop
        };

        public const AttachmentFlags defaultAttachmentFlags = AttachmentFlags.ParentToHand |
                                                              AttachmentFlags.DetachOthers |
                                                              AttachmentFlags.DetachFromOtherHand |
                                                              AttachmentFlags.TurnOnKinematic |
                                                              AttachmentFlags.SnapOnAttach;

        public IF_VR_Steam_Hand otherHand;
        public SteamVR_Input_Sources handType;

        public SteamVR_Behaviour_Pose trackedObject;

        public SteamVR_Action_Boolean grabPinchAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabPinch");

        public SteamVR_Action_Boolean grabGripAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabGrip");

        public SteamVR_Action_Vibration hapticAction = SteamVR_Input.GetAction<SteamVR_Action_Vibration>("Haptic");

        public SteamVR_Action_Boolean uiInteractAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("InteractUI");

        public bool useHoverSphere = true;
        public Transform hoverSphereTransform;
        public float hoverSphereRadius = 0.05f;
        public LayerMask hoverLayerMask = -1;
        public float hoverUpdateInterval = 0.1f;

        public bool useControllerHoverComponent = true;
        public string controllerHoverComponent = "tip";
        public float controllerHoverRadius = 0.075f;

        public bool useFingerJointHover = true;
        public SteamVR_Skeleton_JointIndexEnum fingerJointHover = SteamVR_Skeleton_JointIndexEnum.indexTip;
        public float fingerJointHoverRadius = 0.025f;

        [Tooltip("A transform on the hand to center attached objects on")]
        public Transform objectAttachmentPoint;

        public Camera noSteamVRFallbackCamera;
        public float noSteamVRFallbackMaxDistanceNoItem = 10.0f;
        public float noSteamVRFallbackMaxDistanceWithItem = 0.5f;
        private float noSteamVRFallbackInteractorDistance = -1.0f;

        public GameObject renderModelPrefab;
        [HideInInspector]
        public List<IF_VR_Steam_RenderModel> renderModels = new List<IF_VR_Steam_RenderModel>();
        [HideInInspector]
        public IF_VR_Steam_RenderModel mainRenderModel;
        [HideInInspector]
        public IF_VR_Steam_RenderModel hoverhighlightRenderModel;

        public bool showDebugText = false;
        public bool spewDebugText = false;
        public bool showDebugInteractables = false;

        public struct AttachedObject
        {
            public GameObject attachedObject;
            public IF_VR_Steam_Interactable interactable;
            public Rigidbody attachedRigidbody;
            public CollisionDetectionMode collisionDetectionMode;
            public bool attachedRigidbodyWasKinematic;
            public bool attachedRigidbodyUsedGravity;
            public GameObject originalParent;
            public bool isParentedToHand;
            public IF_VR_Steam_GrabTypes grabbedWithType;
            public AttachmentFlags attachmentFlags;
            public Vector3 initialPositionalOffset;
            public Quaternion initialRotationalOffset;
            public Transform attachedOffsetTransform;
            public Transform handAttachmentPointTransform;
            public Vector3 easeSourcePosition;
            public Quaternion easeSourceRotation;
            public float attachTime;
            // ckbang - Teleport
            //public IF_VR_Steam_AllowTeleportWhileAttachedToHand allowTeleportWhileAttachedToHand;

            public bool HasAttachFlag(AttachmentFlags flag)
            {
                return (attachmentFlags & flag) == flag;
            }
        }

        private List<AttachedObject> attachedObjects = new List<AttachedObject>();

        public ReadOnlyCollection<AttachedObject> AttachedObjects
        {
            get { return attachedObjects.AsReadOnly(); }
        }

        public bool hoverLocked { get; private set; }

        private IF_VR_Steam_Interactable _hoveringInteractable;

        private TextMesh debugText;
        private int prevOverlappingColliders = 0;

        private const int ColliderArraySize = 32;
        private Collider[] overlappingColliders;

        private IF_VR_Steam_Player playerInstance;

        private GameObject applicationLostFocusObject;

        private SteamVR_Events.Action inputFocusAction;

        public bool isActive
        {
            get
            {
                if (trackedObject != null)
                    return trackedObject.isActive;

                return this.gameObject.activeInHierarchy;
            }
        }

        public bool isPoseValid
        {
            get
            {
                return trackedObject != null && trackedObject.isValid;
            }
        }


        //-------------------------------------------------
        // The IF_VR_Steam_Interactable object this IF_VR_Steam_Hand is currently hovering over
        //-------------------------------------------------
        public IF_VR_Steam_Interactable hoveringInteractable
        {
            get { return _hoveringInteractable; }
            set
            {
                if (_hoveringInteractable != value)
                {
                    if (_hoveringInteractable != null)
                    {
                        if (spewDebugText)
                            HandDebugLog("HoverEnd " + _hoveringInteractable.gameObject);
                        _hoveringInteractable.SendMessage("OnHandHoverEnd", this, SendMessageOptions.DontRequireReceiver);
                        {
                            var sendMessageToEvent = EcsRxApplicationBehaviour.Instance.Container.Resolve<IF_VR_Steam_ISendMessageToEvent>();
                            sendMessageToEvent.OnHandHoverEnd(_hoveringInteractable.gameObject, this.gameObject);
                        }

                        //Note: The _hoveringInteractable can change after sending the OnHandHoverEnd message so we need to check it again before broadcasting this message
                        if (_hoveringInteractable != null)
                        {
                            this.BroadcastMessage("OnParentHandHoverEnd", _hoveringInteractable, SendMessageOptions.DontRequireReceiver); // let objects attached to the hand know that a hover has ended
                            {
                                var broadcastMessageToEvent = EcsRxApplicationBehaviour.Instance.Container.Resolve<IF_VR_Steam_IBroadcastMessageToEvent>();
                                broadcastMessageToEvent.OnParentHandHoverEnd(this.gameObject, _hoveringInteractable.gameObject);
                            }
                        }
                    }

                    _hoveringInteractable = value;

                    if (_hoveringInteractable != null)
                    {
                        if (spewDebugText)
                            HandDebugLog("HoverBegin " + _hoveringInteractable.gameObject);
                        _hoveringInteractable.SendMessage("OnHandHoverBegin", this, SendMessageOptions.DontRequireReceiver);
                        {
                            var sendMessageToEvent = EcsRxApplicationBehaviour.Instance.Container.Resolve<IF_VR_Steam_ISendMessageToEvent>();
                            sendMessageToEvent.OnHandHoverBegin(_hoveringInteractable.gameObject, this.gameObject);
                        }

                        //Note: The _hoveringInteractable can change after sending the OnHandHoverBegin message so we need to check it again before broadcasting this message
                        if (_hoveringInteractable != null)
                        {
                            this.BroadcastMessage("OnParentHandHoverBegin", _hoveringInteractable, SendMessageOptions.DontRequireReceiver); // let objects attached to the hand know that a hover has begun
                            var broadcastMessageToEvent = EcsRxApplicationBehaviour.Instance.Container.Resolve<IF_VR_Steam_IBroadcastMessageToEvent>();
                            broadcastMessageToEvent.OnParentHandHoverBegin(this.gameObject, _hoveringInteractable.gameObject);
                        }
                    }
                }
            }
        }


        //-------------------------------------------------
        // Active GameObject attached to this IF_VR_Steam_Hand
        //-------------------------------------------------
        public GameObject currentAttachedObject
        {
            get
            {
                CleanUpAttachedObjectStack();

                if (attachedObjects.Count > 0)
                {
                    return attachedObjects[attachedObjects.Count - 1].attachedObject;
                }

                return null;
            }
        }

        public AttachedObject? currentAttachedObjectInfo
        {
            get
            {
                CleanUpAttachedObjectStack();

                if (attachedObjects.Count > 0)
                {
                    return attachedObjects[attachedObjects.Count - 1];
                }

                return null;
            }
        }

        // ckbang - Teleport
        //public IF_VR_Steam_AllowTeleportWhileAttachedToHand currentAttachedTeleportManager
        //{
        //    get
        //    {
        //        if (currentAttachedObjectInfo.HasValue)
        //            return currentAttachedObjectInfo.Value.allowTeleportWhileAttachedToHand;
        //        return null;
        //    }
        //}

        public SteamVR_Behaviour_Skeleton skeleton
        {
            get
            {
                if (mainRenderModel != null)
                    return mainRenderModel.GetSkeleton();

                return null;
            }
        }

        public void ShowController(bool permanent = false)
        {
            if (mainRenderModel != null)
                mainRenderModel.SetControllerVisibility(true, permanent);

            if (hoverhighlightRenderModel != null)
                hoverhighlightRenderModel.SetControllerVisibility(true, permanent);
        }

        public void HideController(bool permanent = false)
        {
            if (mainRenderModel != null)
                mainRenderModel.SetControllerVisibility(false, permanent);

            if (hoverhighlightRenderModel != null)
                hoverhighlightRenderModel.SetControllerVisibility(false, permanent);
        }

        public void ShowSkeleton(bool permanent = false)
        {
            if (mainRenderModel != null)
                mainRenderModel.SetHandVisibility(true, permanent);

            if (hoverhighlightRenderModel != null)
                hoverhighlightRenderModel.SetHandVisibility(true, permanent);
        }

        public void HideSkeleton(bool permanent = false)
        {
            if (mainRenderModel != null)
                mainRenderModel.SetHandVisibility(false, permanent);

            if (hoverhighlightRenderModel != null)
                hoverhighlightRenderModel.SetHandVisibility(false, permanent);
        }

        public bool HasSkeleton()
        {
            return mainRenderModel != null && mainRenderModel.GetSkeleton() != null;
        }

        public void Show()
        {
            SetVisibility(true);
        }

        public void Hide()
        {
            SetVisibility(false);
        }

        public void SetVisibility(bool visible)
        {
            if (mainRenderModel != null)
                mainRenderModel.SetVisibility(visible);
        }

        public void SetSkeletonRangeOfMotion(EVRSkeletalMotionRange newRangeOfMotion, float blendOverSeconds = 0.1f)
        {
            for (int renderModelIndex = 0; renderModelIndex < renderModels.Count; renderModelIndex++)
            {
                renderModels[renderModelIndex].SetSkeletonRangeOfMotion(newRangeOfMotion, blendOverSeconds);
            }
        }

        public void SetTemporarySkeletonRangeOfMotion(SkeletalMotionRangeChange temporaryRangeOfMotionChange, float blendOverSeconds = 0.1f)
        {
            for (int renderModelIndex = 0; renderModelIndex < renderModels.Count; renderModelIndex++)
            {
                renderModels[renderModelIndex].SetTemporarySkeletonRangeOfMotion(temporaryRangeOfMotionChange, blendOverSeconds);
            }
        }

        public void ResetTemporarySkeletonRangeOfMotion(float blendOverSeconds = 0.1f)
        {
            for (int renderModelIndex = 0; renderModelIndex < renderModels.Count; renderModelIndex++)
            {
                renderModels[renderModelIndex].ResetTemporarySkeletonRangeOfMotion(blendOverSeconds);
            }
        }

        public void SetAnimationState(int stateValue)
        {
            for (int renderModelIndex = 0; renderModelIndex < renderModels.Count; renderModelIndex++)
            {
                renderModels[renderModelIndex].SetAnimationState(stateValue);
            }
        }

        public void StopAnimation()
        {
            for (int renderModelIndex = 0; renderModelIndex < renderModels.Count; renderModelIndex++)
            {
                renderModels[renderModelIndex].StopAnimation();
            }
        }


        //-------------------------------------------------
        // Attach a GameObject to this GameObject
        //
        // objectToAttach - The GameObject to attach
        // flags - The flags to use for attaching the object
        // attachmentPoint - Name of the GameObject in the hierarchy of this IF_VR_Steam_Hand which should act as the attachment point for this GameObject
        //-------------------------------------------------
        public void AttachObject(GameObject objectToAttach, IF_VR_Steam_GrabTypes grabbedWithType, AttachmentFlags flags = defaultAttachmentFlags, Transform attachmentOffset = null)
        {
            AttachedObject attachedObject = new AttachedObject();
            attachedObject.attachmentFlags = flags;
            attachedObject.attachedOffsetTransform = attachmentOffset;
            attachedObject.attachTime = Time.time;

            if (flags == 0)
            {
                flags = defaultAttachmentFlags;
            }

            //Make sure top object on stack is non-null
            CleanUpAttachedObjectStack();

            //Detach the object if it is already attached so that it can get re-attached at the top of the stack
            if (ObjectIsAttached(objectToAttach))
                DetachObject(objectToAttach);

            //Detach from the other hand if requested
            if (attachedObject.HasAttachFlag(AttachmentFlags.DetachFromOtherHand))
            {
                if (otherHand != null)
                    otherHand.DetachObject(objectToAttach);
            }

            if (attachedObject.HasAttachFlag(AttachmentFlags.DetachOthers))
            {
                //Detach all the objects from the stack
                while (attachedObjects.Count > 0)
                {
                    DetachObject(attachedObjects[0].attachedObject);
                }
            }

            if (currentAttachedObject)
            {
                currentAttachedObject.SendMessage("OnHandFocusLost", this, SendMessageOptions.DontRequireReceiver);
                {
                    var sendMessageToEvent = EcsRxApplicationBehaviour.Instance.Container.Resolve<IF_VR_Steam_ISendMessageToEvent>();
                    sendMessageToEvent.OnHandFocusLost(currentAttachedObject.gameObject, this.gameObject);
                }
            }

            attachedObject.attachedObject = objectToAttach;
            attachedObject.interactable = objectToAttach.GetComponent<IF_VR_Steam_Interactable>();
            // ckbang - Teleport
            //attachedObject.allowTeleportWhileAttachedToHand = objectToAttach.GetComponent<IF_VR_Steam_AllowTeleportWhileAttachedToHand>();
            attachedObject.handAttachmentPointTransform = this.transform;

            if (attachedObject.interactable != null)
            {
                if (attachedObject.interactable.attachEaseIn)
                {
                    attachedObject.easeSourcePosition = attachedObject.attachedObject.transform.position;
                    attachedObject.easeSourceRotation = attachedObject.attachedObject.transform.rotation;
                    attachedObject.interactable.snapAttachEaseInCompleted = false;
                }

                if (attachedObject.interactable.useHandObjectAttachmentPoint)
                    attachedObject.handAttachmentPointTransform = objectAttachmentPoint;

                if (attachedObject.interactable.hideHandOnAttach)
                    Hide();

                if (attachedObject.interactable.hideSkeletonOnAttach && mainRenderModel != null && mainRenderModel.displayHandByDefault)
                    HideSkeleton();

                if (attachedObject.interactable.hideControllerOnAttach && mainRenderModel != null && mainRenderModel.displayControllerByDefault)
                    HideController();

                if (attachedObject.interactable.handAnimationOnPickup != 0)
                    SetAnimationState(attachedObject.interactable.handAnimationOnPickup);

                if (attachedObject.interactable.setRangeOfMotionOnPickup != SkeletalMotionRangeChange.None)
                    SetTemporarySkeletonRangeOfMotion(attachedObject.interactable.setRangeOfMotionOnPickup);

            }

            attachedObject.originalParent = objectToAttach.transform.parent != null ? objectToAttach.transform.parent.gameObject : null;

            attachedObject.attachedRigidbody = objectToAttach.GetComponent<Rigidbody>();
            if (attachedObject.attachedRigidbody != null)
            {
                if (attachedObject.interactable.attachedToHand != null) //already attached to another hand
                {
                    //if it was attached to another hand, get the flags from that hand

                    for (int attachedIndex = 0; attachedIndex < attachedObject.interactable.attachedToHand.attachedObjects.Count; attachedIndex++)
                    {
                        AttachedObject attachedObjectInList = attachedObject.interactable.attachedToHand.attachedObjects[attachedIndex];
                        if (attachedObjectInList.interactable == attachedObject.interactable)
                        {
                            attachedObject.attachedRigidbodyWasKinematic = attachedObjectInList.attachedRigidbodyWasKinematic;
                            attachedObject.attachedRigidbodyUsedGravity = attachedObjectInList.attachedRigidbodyUsedGravity;
                            attachedObject.originalParent = attachedObjectInList.originalParent;
                        }
                    }
                }
                else
                {
                    attachedObject.attachedRigidbodyWasKinematic = attachedObject.attachedRigidbody.isKinematic;
                    attachedObject.attachedRigidbodyUsedGravity = attachedObject.attachedRigidbody.useGravity;
                }
            }

            attachedObject.grabbedWithType = grabbedWithType;

            if (attachedObject.HasAttachFlag(AttachmentFlags.ParentToHand))
            {
                //Parent the object to the hand
                objectToAttach.transform.parent = this.transform;
                attachedObject.isParentedToHand = true;
            }
            else
            {
                attachedObject.isParentedToHand = false;
            }

            if (attachedObject.HasAttachFlag(AttachmentFlags.SnapOnAttach))
            {
                if (attachedObject.interactable != null && attachedObject.interactable.skeletonPoser != null && HasSkeleton())
                {
                    SteamVR_Skeleton_PoseSnapshot pose = attachedObject.interactable.skeletonPoser.GetBlendedPose(skeleton);

                    //snap the object to the center of the attach point
                    objectToAttach.transform.position = this.transform.TransformPoint(pose.position);
                    objectToAttach.transform.rotation = this.transform.rotation * pose.rotation;

                    attachedObject.initialPositionalOffset = attachedObject.handAttachmentPointTransform.InverseTransformPoint(objectToAttach.transform.position);
                    attachedObject.initialRotationalOffset = Quaternion.Inverse(attachedObject.handAttachmentPointTransform.rotation) * objectToAttach.transform.rotation;
                }
                else
                {
                    if (attachmentOffset != null)
                    {
                        //offset the object from the hand by the positional and rotational difference between the offset transform and the attached object
                        Quaternion rotDiff = Quaternion.Inverse(attachmentOffset.transform.rotation) * objectToAttach.transform.rotation;
                        objectToAttach.transform.rotation = attachedObject.handAttachmentPointTransform.rotation * rotDiff;

                        Vector3 posDiff = objectToAttach.transform.position - attachmentOffset.transform.position;
                        objectToAttach.transform.position = attachedObject.handAttachmentPointTransform.position + posDiff;
                    }
                    else
                    {
                        //snap the object to the center of the attach point
                        objectToAttach.transform.rotation = attachedObject.handAttachmentPointTransform.rotation;
                        objectToAttach.transform.position = attachedObject.handAttachmentPointTransform.position;
                    }

                    Transform followPoint = objectToAttach.transform;

                    attachedObject.initialPositionalOffset = attachedObject.handAttachmentPointTransform.InverseTransformPoint(followPoint.position);
                    attachedObject.initialRotationalOffset = Quaternion.Inverse(attachedObject.handAttachmentPointTransform.rotation) * followPoint.rotation;
                }
            }
            else
            {
                if (attachedObject.interactable != null && attachedObject.interactable.skeletonPoser != null && HasSkeleton())
                {
                    attachedObject.initialPositionalOffset = attachedObject.handAttachmentPointTransform.InverseTransformPoint(objectToAttach.transform.position);
                    attachedObject.initialRotationalOffset = Quaternion.Inverse(attachedObject.handAttachmentPointTransform.rotation) * objectToAttach.transform.rotation;
                }
                else
                {
                    if (attachmentOffset != null)
                    {
                        //get the initial positional and rotational offsets between the hand and the offset transform
                        Quaternion rotDiff = Quaternion.Inverse(attachmentOffset.transform.rotation) * objectToAttach.transform.rotation;
                        Quaternion targetRotation = attachedObject.handAttachmentPointTransform.rotation * rotDiff;
                        Quaternion rotationPositionBy = targetRotation * Quaternion.Inverse(objectToAttach.transform.rotation);

                        Vector3 posDiff = (rotationPositionBy * objectToAttach.transform.position) - (rotationPositionBy * attachmentOffset.transform.position);

                        attachedObject.initialPositionalOffset = attachedObject.handAttachmentPointTransform.InverseTransformPoint(attachedObject.handAttachmentPointTransform.position + posDiff);
                        attachedObject.initialRotationalOffset = Quaternion.Inverse(attachedObject.handAttachmentPointTransform.rotation) * (attachedObject.handAttachmentPointTransform.rotation * rotDiff);
                    }
                    else
                    {
                        attachedObject.initialPositionalOffset = attachedObject.handAttachmentPointTransform.InverseTransformPoint(objectToAttach.transform.position);
                        attachedObject.initialRotationalOffset = Quaternion.Inverse(attachedObject.handAttachmentPointTransform.rotation) * objectToAttach.transform.rotation;
                    }
                }
            }



            if (attachedObject.HasAttachFlag(AttachmentFlags.TurnOnKinematic))
            {
                if (attachedObject.attachedRigidbody != null)
                {
                    attachedObject.collisionDetectionMode = attachedObject.attachedRigidbody.collisionDetectionMode;
                    if (attachedObject.collisionDetectionMode == CollisionDetectionMode.Continuous)
                        attachedObject.attachedRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;

                    attachedObject.attachedRigidbody.isKinematic = true;
                }
            }

            if (attachedObject.HasAttachFlag(AttachmentFlags.TurnOffGravity))
            {
                if (attachedObject.attachedRigidbody != null)
                {
                    attachedObject.attachedRigidbody.useGravity = false;
                }
            }

            if (attachedObject.interactable != null && attachedObject.interactable.attachEaseIn)
            {
                attachedObject.attachedObject.transform.position = attachedObject.easeSourcePosition;
                attachedObject.attachedObject.transform.rotation = attachedObject.easeSourceRotation;
            }

            attachedObjects.Add(attachedObject);

            UpdateHovering();

            if (spewDebugText)
                HandDebugLog("AttachObject " + objectToAttach);
            objectToAttach.SendMessage("OnAttachedToHand", this, SendMessageOptions.DontRequireReceiver);
            {
                var sendMessageToEvent = EcsRxApplicationBehaviour.Instance.Container.Resolve<IF_VR_Steam_ISendMessageToEvent>();
                sendMessageToEvent.OnAttachedToHand(objectToAttach.gameObject, this.gameObject);
            }
        }

        public bool ObjectIsAttached(GameObject go)
        {
            for (int attachedIndex = 0; attachedIndex < attachedObjects.Count; attachedIndex++)
            {
                if (attachedObjects[attachedIndex].attachedObject == go)
                    return true;
            }

            return false;
        }

        public void ForceHoverUnlock()
        {
            hoverLocked = false;
        }

        //-------------------------------------------------
        // Detach this GameObject from the attached object stack of this IF_VR_Steam_Hand
        //
        // objectToDetach - The GameObject to detach from this IF_VR_Steam_Hand
        //-------------------------------------------------
        public void DetachObject(GameObject objectToDetach, bool restoreOriginalParent = true)
        {
            int index = attachedObjects.FindIndex(l => l.attachedObject == objectToDetach);
            if (index != -1)
            {
                if (spewDebugText)
                    HandDebugLog("DetachObject " + objectToDetach);

                GameObject prevTopObject = currentAttachedObject;


                if (attachedObjects[index].interactable != null)
                {
                    if (attachedObjects[index].interactable.hideHandOnAttach)
                        Show();

                    if (attachedObjects[index].interactable.hideSkeletonOnAttach && mainRenderModel != null && mainRenderModel.displayHandByDefault)
                        ShowSkeleton();

                    if (attachedObjects[index].interactable.hideControllerOnAttach && mainRenderModel != null && mainRenderModel.displayControllerByDefault)
                        ShowController();

                    if (attachedObjects[index].interactable.handAnimationOnPickup != 0)
                        StopAnimation();

                    if (attachedObjects[index].interactable.setRangeOfMotionOnPickup != SkeletalMotionRangeChange.None)
                        ResetTemporarySkeletonRangeOfMotion();
                }

                Transform parentTransform = null;
                if (attachedObjects[index].isParentedToHand)
                {
                    if (restoreOriginalParent && (attachedObjects[index].originalParent != null))
                    {
                        parentTransform = attachedObjects[index].originalParent.transform;
                    }

                    if (attachedObjects[index].attachedObject != null)
                    {
                        attachedObjects[index].attachedObject.transform.parent = parentTransform;
                    }
                }

                if (attachedObjects[index].HasAttachFlag(AttachmentFlags.TurnOnKinematic))
                {
                    if (attachedObjects[index].attachedRigidbody != null)
                    {
                        attachedObjects[index].attachedRigidbody.isKinematic = attachedObjects[index].attachedRigidbodyWasKinematic;
                        attachedObjects[index].attachedRigidbody.collisionDetectionMode = attachedObjects[index].collisionDetectionMode;
                    }
                }

                if (attachedObjects[index].HasAttachFlag(AttachmentFlags.TurnOffGravity))
                {
                    if (attachedObjects[index].attachedObject != null)
                    {
                        if (attachedObjects[index].attachedRigidbody != null)
                            attachedObjects[index].attachedRigidbody.useGravity = attachedObjects[index].attachedRigidbodyUsedGravity;
                    }
                }

                if (attachedObjects[index].interactable != null && attachedObjects[index].interactable.handFollowTransform && HasSkeleton())
                {
                    skeleton.transform.localPosition = Vector3.zero;
                    skeleton.transform.localRotation = Quaternion.identity;
                }

                if (attachedObjects[index].attachedObject != null)
                {
                    if (attachedObjects[index].interactable == null || (attachedObjects[index].interactable != null && attachedObjects[index].interactable.isDestroying == false))
                        attachedObjects[index].attachedObject.SetActive(true);

                    attachedObjects[index].attachedObject.SendMessage("OnDetachedFromHand", this, SendMessageOptions.DontRequireReceiver);
                    {
                        var sendMessageToEvent = EcsRxApplicationBehaviour.Instance.Container.Resolve<IF_VR_Steam_ISendMessageToEvent>();
                        sendMessageToEvent.OnDetachedFromHand(attachedObjects[index].attachedObject.gameObject, this.gameObject);
                    }
                }

                attachedObjects.RemoveAt(index);

                CleanUpAttachedObjectStack();

                GameObject newTopObject = currentAttachedObject;

                hoverLocked = false;


                //Give focus to the top most object on the stack if it changed
                if (newTopObject != null && newTopObject != prevTopObject)
                {
                    newTopObject.SetActive(true);
                    newTopObject.SendMessage("OnHandFocusAcquired", this, SendMessageOptions.DontRequireReceiver);
                    {
                        var sendMessageToEvent = EcsRxApplicationBehaviour.Instance.Container.Resolve<IF_VR_Steam_ISendMessageToEvent>();
                        sendMessageToEvent.OnHandFocusAcquired(newTopObject, this.gameObject);
                    }
                }
            }

            CleanUpAttachedObjectStack();

            if (mainRenderModel != null)
                mainRenderModel.MatchHandToTransform(mainRenderModel.transform);
            if (hoverhighlightRenderModel != null)
                hoverhighlightRenderModel.MatchHandToTransform(hoverhighlightRenderModel.transform);
        }


        //-------------------------------------------------
        // Get the world velocity of the VR IF_VR_Steam_Hand.
        //-------------------------------------------------
        public Vector3 GetTrackedObjectVelocity(float timeOffset = 0)
        {
            if (trackedObject == null)
            {
                Vector3 velocityTarget, angularTarget;
                GetUpdatedAttachedVelocities(currentAttachedObjectInfo.Value, out velocityTarget, out angularTarget);
                return velocityTarget;
            }

            if (isActive)
            {
                if (timeOffset == 0)
                    return IF_VR_Steam_Player.instance.trackingOriginTransform.TransformVector(trackedObject.GetVelocity());
                else
                {
                    Vector3 velocity;
                    Vector3 angularVelocity;

                    trackedObject.GetVelocitiesAtTimeOffset(timeOffset, out velocity, out angularVelocity);
                    return IF_VR_Steam_Player.instance.trackingOriginTransform.TransformVector(velocity);
                }
            }

            return Vector3.zero;
        }


        //-------------------------------------------------
        // Get the world space angular velocity of the VR IF_VR_Steam_Hand.
        //-------------------------------------------------
        public Vector3 GetTrackedObjectAngularVelocity(float timeOffset = 0)
        {
            if (trackedObject == null)
            {
                Vector3 velocityTarget, angularTarget;
                GetUpdatedAttachedVelocities(currentAttachedObjectInfo.Value, out velocityTarget, out angularTarget);
                return angularTarget;
            }

            if (isActive)
            {
                if (timeOffset == 0)
                    return IF_VR_Steam_Player.instance.trackingOriginTransform.TransformDirection(trackedObject.GetAngularVelocity());
                else
                {
                    Vector3 velocity;
                    Vector3 angularVelocity;

                    trackedObject.GetVelocitiesAtTimeOffset(timeOffset, out velocity, out angularVelocity);
                    return IF_VR_Steam_Player.instance.trackingOriginTransform.TransformDirection(angularVelocity);
                }
            }

            return Vector3.zero;
        }

        public void GetEstimatedPeakVelocities(out Vector3 velocity, out Vector3 angularVelocity)
        {
            trackedObject.GetEstimatedPeakVelocities(out velocity, out angularVelocity);
            velocity = IF_VR_Steam_Player.instance.trackingOriginTransform.TransformVector(velocity);
            angularVelocity = IF_VR_Steam_Player.instance.trackingOriginTransform.TransformDirection(angularVelocity);
        }


        //-------------------------------------------------
        private void CleanUpAttachedObjectStack()
        {
            attachedObjects.RemoveAll(l => l.attachedObject == null);
        }


        //-------------------------------------------------
        protected virtual void Awake()
        {
            inputFocusAction = SteamVR_Events.InputFocusAction(OnInputFocus);

            if (hoverSphereTransform == null)
                hoverSphereTransform = this.transform;

            if (objectAttachmentPoint == null)
                objectAttachmentPoint = this.transform;

            applicationLostFocusObject = new GameObject("_application_lost_focus");
            applicationLostFocusObject.transform.parent = transform;
            applicationLostFocusObject.SetActive(false);

            var componentBuilder = EcsRxApplicationBehaviour.Instance.Container.Resolve<IF_VR_Steam_IComponentBuilder>();
            componentBuilder.BuildTracker(this);

            if (trackedObject == null)
            {
                trackedObject = this.gameObject.GetComponent<SteamVR_Behaviour_Pose>();
            }

            if (trackedObject != null)
            {
                trackedObject.onTransformUpdatedEvent += OnTransformUpdated;
                var handPhysics = GetComponent<IF_VR_Steam_HandPhysics>();
                if (handPhysics != null)
                {
                    handPhysics.TrackedObject = trackedObject;
                }
            }
        }

        protected virtual void OnDestroy()
        {
            if (trackedObject != null)
            {
                trackedObject.onTransformUpdatedEvent -= OnTransformUpdated;
            }
        }

        protected virtual void OnTransformUpdated(SteamVR_Behaviour_Pose updatedPose, SteamVR_Input_Sources updatedSource)
        {
            HandFollowUpdate();
        }

        //-------------------------------------------------
        protected virtual IEnumerator Start()
        {
            // save off player instance
            playerInstance = IF_VR_Steam_Player.instance;
            if (!playerInstance)
            {
                Debug.LogError("<b>[SteamVR Interaction]</b> No player instance found in IF_VR_Steam_Hand Start()", this);
            }

            if (this.gameObject.layer == 0)
                Debug.LogWarning("<b>[SteamVR Interaction]</b> IF_VR_Steam_Hand is on default layer. This puts unnecessary strain on hover checks as it is always true for hand colliders (which are then ignored).", this);
            else
                hoverLayerMask &= ~(1 << this.gameObject.layer); //ignore self for hovering

            // allocate array for colliders
            overlappingColliders = new Collider[ColliderArraySize];

            // We are a "no SteamVR fallback hand" if we have this camera set
            // we'll use the right mouse to look around and left mouse to interact
            // - don't need to find the device
            if (noSteamVRFallbackCamera)
            {
                {
                    var componentBuilder = EcsRxApplicationBehaviour.Instance.Container.Resolve<IF_VR_Steam_IComponentBuilder>();
                    componentBuilder.Build(this);
                }
                yield break;
            }

            //Debug.Log( "<b>[SteamVR Interaction]</b> IF_VR_Steam_Hand - initializing connection routine" );

            while (true)
            {
                if (isPoseValid)
                {
                    InitController();
                    break;
                }

                yield return null;
            }

            {
                var componentBuilder = EcsRxApplicationBehaviour.Instance.Container.Resolve<IF_VR_Steam_IComponentBuilder>();
                componentBuilder.Build(this);
            }
        }


        //-------------------------------------------------
        protected virtual void UpdateHovering()
        {
            if ((noSteamVRFallbackCamera == null) && (isActive == false))
            {
                return;
            }

            if (hoverLocked)
                return;

            if (applicationLostFocusObject.activeSelf)
                return;

            float closestDistance = float.MaxValue;
            IF_VR_Steam_Interactable closestInteractable = null;

            if (useHoverSphere)
            {
                float scaledHoverRadius = hoverSphereRadius * Mathf.Abs(SteamVR_Utils.GetLossyScale(hoverSphereTransform));
                CheckHoveringForTransform(hoverSphereTransform.position, scaledHoverRadius, ref closestDistance, ref closestInteractable, Color.green);
            }

            if (useControllerHoverComponent && mainRenderModel != null && mainRenderModel.IsControllerVisibile())
            {
                float scaledHoverRadius = controllerHoverRadius * Mathf.Abs(SteamVR_Utils.GetLossyScale(this.transform));
                CheckHoveringForTransform(mainRenderModel.GetControllerPosition(controllerHoverComponent), scaledHoverRadius / 2f, ref closestDistance, ref closestInteractable, Color.blue);
            }

            if (useFingerJointHover && mainRenderModel != null && mainRenderModel.IsHandVisibile())
            {
                float scaledHoverRadius = fingerJointHoverRadius * Mathf.Abs(SteamVR_Utils.GetLossyScale(this.transform));
                CheckHoveringForTransform(mainRenderModel.GetBonePosition((int)fingerJointHover), scaledHoverRadius / 2f, ref closestDistance, ref closestInteractable, Color.yellow);
            }

            // Hover on this one
            hoveringInteractable = closestInteractable;
        }

        protected virtual bool CheckHoveringForTransform(Vector3 hoverPosition, float hoverRadius, ref float closestDistance, ref IF_VR_Steam_Interactable closestInteractable, Color debugColor)
        {
            bool foundCloser = false;

            // null out old vals
            for (int i = 0; i < overlappingColliders.Length; ++i)
            {
                overlappingColliders[i] = null;
            }

            int numColliding = Physics.OverlapSphereNonAlloc(hoverPosition, hoverRadius, overlappingColliders, hoverLayerMask.value);

            if (numColliding >= ColliderArraySize)
                Debug.LogWarning("<b>[SteamVR Interaction]</b> This hand is overlapping the max number of colliders: " + ColliderArraySize + ". Some collisions may be missed. Increase ColliderArraySize on IF_VR_Steam_Hand.cs");

            // DebugVar
            int iActualColliderCount = 0;

            // Pick the closest hovering
            for (int colliderIndex = 0; colliderIndex < overlappingColliders.Length; colliderIndex++)
            {
                Collider collider = overlappingColliders[colliderIndex];

                if (collider == null)
                    continue;

                IF_VR_Steam_Interactable contacting = collider.GetComponentInParent<IF_VR_Steam_Interactable>();

                // Yeah, it's null, skip
                if (contacting == null)
                    continue;

                // Ignore this collider for hovering
                IF_VR_Steam_IgnoreHovering ignore = collider.GetComponent<IF_VR_Steam_IgnoreHovering>();
                if (ignore != null)
                {
                    if (ignore.onlyIgnoreHand == null || ignore.onlyIgnoreHand == this)
                    {
                        continue;
                    }
                }

                // Can't hover over the object if it's attached
                bool hoveringOverAttached = false;
                for (int attachedIndex = 0; attachedIndex < attachedObjects.Count; attachedIndex++)
                {
                    if (attachedObjects[attachedIndex].attachedObject == contacting.gameObject)
                    {
                        hoveringOverAttached = true;
                        break;
                    }
                }

                if (hoveringOverAttached)
                    continue;

                // Best candidate so far...
                float distance = Vector3.Distance(contacting.transform.position, hoverPosition);
                //float distance = Vector3.Distance(collider.bounds.center, hoverPosition);
                bool lowerPriority = false;
                if (closestInteractable != null)
                { // compare to closest interactable to check priority
                    lowerPriority = contacting.hoverPriority < closestInteractable.hoverPriority;
                }
                bool isCloser = (distance < closestDistance);
                if (isCloser && !lowerPriority)
                {
                    closestDistance = distance;
                    closestInteractable = contacting;
                    foundCloser = true;
                }
                iActualColliderCount++;
            }

            if (showDebugInteractables && foundCloser)
            {
                Debug.DrawLine(hoverPosition, closestInteractable.transform.position, debugColor, .05f, false);
            }

            if (iActualColliderCount > 0 && iActualColliderCount != prevOverlappingColliders)
            {
                prevOverlappingColliders = iActualColliderCount;

                if (spewDebugText)
                    HandDebugLog("Found " + iActualColliderCount + " overlapping colliders.");
            }

            return foundCloser;
        }


        //-------------------------------------------------
        protected virtual void UpdateNoSteamVRFallback()
        {
            if (noSteamVRFallbackCamera)
            {
                Ray ray = noSteamVRFallbackCamera.ScreenPointToRay(Input.mousePosition);

                if (attachedObjects.Count > 0)
                {
                    // Holding down the mouse:
                    // move around a fixed distance from the camera
                    transform.position = ray.origin + noSteamVRFallbackInteractorDistance * ray.direction;
                }
                else
                {
                    // Not holding down the mouse:
                    // cast out a ray to see what we should mouse over

                    // Don't want to hit the hand and anything underneath it
                    // So move it back behind the camera when we do the raycast
                    Vector3 oldPosition = transform.position;
                    transform.position = noSteamVRFallbackCamera.transform.forward * (-1000.0f);

                    RaycastHit raycastHit;
                    if (Physics.Raycast(ray, out raycastHit, noSteamVRFallbackMaxDistanceNoItem))
                    {
                        transform.position = raycastHit.point;

                        // Remember this distance in case we click and drag the mouse
                        noSteamVRFallbackInteractorDistance = Mathf.Min(noSteamVRFallbackMaxDistanceNoItem, raycastHit.distance);
                    }
                    else if (noSteamVRFallbackInteractorDistance > 0.0f)
                    {
                        // Move it around at the distance we last had a hit
                        transform.position = ray.origin + Mathf.Min(noSteamVRFallbackMaxDistanceNoItem, noSteamVRFallbackInteractorDistance) * ray.direction;
                    }
                    else
                    {
                        // Didn't hit, just leave it where it was
                        transform.position = oldPosition;
                    }
                }
            }
        }


        //-------------------------------------------------
        private void UpdateDebugText()
        {
            if (showDebugText)
            {
                if (debugText == null)
                {
                    debugText = new GameObject("_debug_text").AddComponent<TextMesh>();
                    debugText.fontSize = 120;
                    debugText.characterSize = 0.001f;
                    debugText.transform.parent = transform;

                    debugText.transform.localRotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
                }

                if (handType == SteamVR_Input_Sources.RightHand)
                {
                    debugText.transform.localPosition = new Vector3(-0.05f, 0.0f, 0.0f);
                    debugText.alignment = TextAlignment.Right;
                    debugText.anchor = TextAnchor.UpperRight;
                }
                else
                {
                    debugText.transform.localPosition = new Vector3(0.05f, 0.0f, 0.0f);
                    debugText.alignment = TextAlignment.Left;
                    debugText.anchor = TextAnchor.UpperLeft;
                }

                debugText.text = string.Format(
                    "Hovering: {0}\n" +
                    "Hover Lock: {1}\n" +
                    "Attached: {2}\n" +
                    "Total Attached: {3}\n" +
                    "Type: {4}\n",
                    (hoveringInteractable ? hoveringInteractable.gameObject.name : "null"),
                    hoverLocked,
                    (currentAttachedObject ? currentAttachedObject.name : "null"),
                    attachedObjects.Count,
                    handType.ToString());
            }
            else
            {
                if (debugText != null)
                {
                    Destroy(debugText.gameObject);
                }
            }
        }


        //-------------------------------------------------
        protected virtual void OnEnable()
        {
            inputFocusAction.enabled = true;

            // Stagger updates between hands
            float hoverUpdateBegin = ((otherHand != null) && (otherHand.GetInstanceID() < GetInstanceID())) ? (0.5f * hoverUpdateInterval) : (0.0f);
            InvokeRepeating("UpdateHovering", hoverUpdateBegin, hoverUpdateInterval);
            InvokeRepeating("UpdateDebugText", hoverUpdateBegin, hoverUpdateInterval);
        }


        //-------------------------------------------------
        protected virtual void OnDisable()
        {
            inputFocusAction.enabled = false;

            CancelInvoke();
        }


        //-------------------------------------------------
        protected virtual void Update()
        {
            UpdateNoSteamVRFallback();

            GameObject attachedObject = currentAttachedObject;
            if (attachedObject != null)
            {
                attachedObject.SendMessage("HandAttachedUpdate", this, SendMessageOptions.DontRequireReceiver);
                {
                    var sendMessageToEvent = EcsRxApplicationBehaviour.Instance.Container.Resolve<IF_VR_Steam_ISendMessageToEvent>();
                    sendMessageToEvent.HandAttachedUpdate(attachedObject.gameObject, this.gameObject);
                }
            }

            if (hoveringInteractable)
            {
                hoveringInteractable.SendMessage("HandHoverUpdate", this, SendMessageOptions.DontRequireReceiver);
                {
                    var sendMessageToEvent = EcsRxApplicationBehaviour.Instance.Container.Resolve<IF_VR_Steam_ISendMessageToEvent>();
                    sendMessageToEvent.HandHoverUpdate(hoveringInteractable.gameObject, this.gameObject);
                }
            }
        }

        /// <summary>
        /// Returns true when the hand is currently hovering over the interactable passed in
        /// </summary>
        public bool IsStillHovering(IF_VR_Steam_Interactable interactable)
        {
            return hoveringInteractable == interactable;
        }

        protected virtual void HandFollowUpdate()
        {
            GameObject attachedObject = currentAttachedObject;
            if (attachedObject != null)
            {
                if (currentAttachedObjectInfo.Value.interactable != null)
                {
                    SteamVR_Skeleton_PoseSnapshot pose = null;

                    if (currentAttachedObjectInfo.Value.interactable.skeletonPoser != null && HasSkeleton())
                    {
                        pose = currentAttachedObjectInfo.Value.interactable.skeletonPoser.GetBlendedPose(skeleton);
                    }

                    if (currentAttachedObjectInfo.Value.interactable.handFollowTransform)
                    {
                        Quaternion targetHandRotation;
                        Vector3 targetHandPosition;

                        if (pose == null)
                        {
                            Quaternion offset = Quaternion.Inverse(this.transform.rotation) * currentAttachedObjectInfo.Value.handAttachmentPointTransform.rotation;
                            targetHandRotation = currentAttachedObjectInfo.Value.interactable.transform.rotation * Quaternion.Inverse(offset);

                            Vector3 worldOffset = (this.transform.position - currentAttachedObjectInfo.Value.handAttachmentPointTransform.position);
                            Quaternion rotationDiff = mainRenderModel.GetHandRotation() * Quaternion.Inverse(this.transform.rotation);
                            Vector3 localOffset = rotationDiff * worldOffset;
                            targetHandPosition = currentAttachedObjectInfo.Value.interactable.transform.position + localOffset;
                        }
                        else
                        {
                            Transform objectT = currentAttachedObjectInfo.Value.attachedObject.transform;
                            Vector3 oldItemPos = objectT.position;
                            Quaternion oldItemRot = objectT.transform.rotation;
                            objectT.position = TargetItemPosition(currentAttachedObjectInfo.Value);
                            objectT.rotation = TargetItemRotation(currentAttachedObjectInfo.Value);
                            Vector3 localSkelePos = objectT.InverseTransformPoint(transform.position);
                            Quaternion localSkeleRot = Quaternion.Inverse(objectT.rotation) * transform.rotation;
                            objectT.position = oldItemPos;
                            objectT.rotation = oldItemRot;

                            targetHandPosition = objectT.TransformPoint(localSkelePos);
                            targetHandRotation = objectT.rotation * localSkeleRot;
                        }

                        if (mainRenderModel != null)
                            mainRenderModel.SetHandRotation(targetHandRotation);
                        if (hoverhighlightRenderModel != null)
                            hoverhighlightRenderModel.SetHandRotation(targetHandRotation);

                        if (mainRenderModel != null)
                            mainRenderModel.SetHandPosition(targetHandPosition);
                        if (hoverhighlightRenderModel != null)
                            hoverhighlightRenderModel.SetHandPosition(targetHandPosition);
                    }
                }
            }
        }

        protected virtual void FixedUpdate()
        {
            if (currentAttachedObject != null)
            {
                AttachedObject attachedInfo = currentAttachedObjectInfo.Value;
                if (attachedInfo.attachedObject != null)
                {
                    if (attachedInfo.HasAttachFlag(AttachmentFlags.VelocityMovement))
                    {
                        if (attachedInfo.interactable.attachEaseIn == false || attachedInfo.interactable.snapAttachEaseInCompleted)
                            UpdateAttachedVelocity(attachedInfo);

                        /*if (attachedInfo.interactable.handFollowTransformPosition)
                        {
                            skeleton.transform.position = TargetSkeletonPosition(attachedInfo);
                            skeleton.transform.rotation = attachedInfo.attachedObject.transform.rotation * attachedInfo.skeletonLockRotation;
                        }*/
                    }
                    else
                    {
                        if (attachedInfo.HasAttachFlag(AttachmentFlags.ParentToHand))
                        {
                            attachedInfo.attachedObject.transform.position = TargetItemPosition(attachedInfo);
                            attachedInfo.attachedObject.transform.rotation = TargetItemRotation(attachedInfo);
                        }
                    }


                    if (attachedInfo.interactable.attachEaseIn)
                    {
                        float t = IF_VR_Steam_Util.RemapNumberClamped(Time.time, attachedInfo.attachTime, attachedInfo.attachTime + attachedInfo.interactable.snapAttachEaseInTime, 0.0f, 1.0f);
                        if (t < 1.0f)
                        {
                            if (attachedInfo.HasAttachFlag(AttachmentFlags.VelocityMovement))
                            {
                                attachedInfo.attachedRigidbody.velocity = Vector3.zero;
                                attachedInfo.attachedRigidbody.angularVelocity = Vector3.zero;
                            }
                            t = attachedInfo.interactable.snapAttachEaseInCurve.Evaluate(t);
                            attachedInfo.attachedObject.transform.position = Vector3.Lerp(attachedInfo.easeSourcePosition, TargetItemPosition(attachedInfo), t);
                            attachedInfo.attachedObject.transform.rotation = Quaternion.Lerp(attachedInfo.easeSourceRotation, TargetItemRotation(attachedInfo), t);
                        }
                        else if (!attachedInfo.interactable.snapAttachEaseInCompleted)
                        {
                            attachedInfo.interactable.gameObject.SendMessage("OnThrowableAttachEaseInCompleted", this, SendMessageOptions.DontRequireReceiver);
                            {
                                var sendMessageToEvent = EcsRxApplicationBehaviour.Instance.Container.Resolve<IF_VR_Steam_ISendMessageToEvent>();
                                sendMessageToEvent.OnThrowableAttachEaseInCompleted(attachedInfo.interactable.gameObject, this.gameObject);
                            }
                            attachedInfo.interactable.snapAttachEaseInCompleted = true;
                        }
                    }
                }
            }
        }

        protected const float MaxVelocityChange = 10f;
        protected const float VelocityMagic = 6000f;
        protected const float AngularVelocityMagic = 50f;
        protected const float MaxAngularVelocityChange = 20f;

        protected void UpdateAttachedVelocity(AttachedObject attachedObjectInfo)
        {
            Vector3 velocityTarget, angularTarget;
            bool success = GetUpdatedAttachedVelocities(attachedObjectInfo, out velocityTarget, out angularTarget);
            if (success)
            {
                float scale = SteamVR_Utils.GetLossyScale(currentAttachedObjectInfo.Value.handAttachmentPointTransform);
                float maxAngularVelocityChange = MaxAngularVelocityChange * scale;
                float maxVelocityChange = MaxVelocityChange * scale;

                attachedObjectInfo.attachedRigidbody.velocity = Vector3.MoveTowards(attachedObjectInfo.attachedRigidbody.velocity, velocityTarget, maxVelocityChange);
                attachedObjectInfo.attachedRigidbody.angularVelocity = Vector3.MoveTowards(attachedObjectInfo.attachedRigidbody.angularVelocity, angularTarget, maxAngularVelocityChange);
            }
        }

        /// <summary>
        /// Snap an attached object to its target position and rotation. Good for error correction.
        /// </summary>
        public void ResetAttachedTransform(AttachedObject attachedObject)
        {
            attachedObject.attachedObject.transform.position = TargetItemPosition(attachedObject);
            attachedObject.attachedObject.transform.rotation = TargetItemRotation(attachedObject);
        }

        protected Vector3 TargetItemPosition(AttachedObject attachedObject)
        {
            if (attachedObject.interactable != null && attachedObject.interactable.skeletonPoser != null && HasSkeleton())
            {
                Vector3 tp = attachedObject.handAttachmentPointTransform.InverseTransformPoint(transform.TransformPoint(attachedObject.interactable.skeletonPoser.GetBlendedPose(skeleton).position));
                //tp.x *= -1;
                return currentAttachedObjectInfo.Value.handAttachmentPointTransform.TransformPoint(tp);
            }
            else
            {
                return currentAttachedObjectInfo.Value.handAttachmentPointTransform.TransformPoint(attachedObject.initialPositionalOffset);
            }
        }

        protected Quaternion TargetItemRotation(AttachedObject attachedObject)
        {
            if (attachedObject.interactable != null && attachedObject.interactable.skeletonPoser != null && HasSkeleton())
            {
                Quaternion tr = Quaternion.Inverse(attachedObject.handAttachmentPointTransform.rotation) * (transform.rotation * attachedObject.interactable.skeletonPoser.GetBlendedPose(skeleton).rotation);
                return currentAttachedObjectInfo.Value.handAttachmentPointTransform.rotation * tr;
            }
            else
            {
                return currentAttachedObjectInfo.Value.handAttachmentPointTransform.rotation * attachedObject.initialRotationalOffset;
            }
        }

        protected bool GetUpdatedAttachedVelocities(AttachedObject attachedObjectInfo, out Vector3 velocityTarget, out Vector3 angularTarget)
        {
            bool realNumbers = false;


            float velocityMagic = VelocityMagic;
            float angularVelocityMagic = AngularVelocityMagic;

            Vector3 targetItemPosition = TargetItemPosition(attachedObjectInfo);
            Vector3 positionDelta = (targetItemPosition - attachedObjectInfo.attachedRigidbody.position);
            velocityTarget = (positionDelta * velocityMagic * Time.deltaTime);

            if (float.IsNaN(velocityTarget.x) == false && float.IsInfinity(velocityTarget.x) == false)
            {
                if (noSteamVRFallbackCamera)
                    velocityTarget /= 10; //hacky fix for fallback

                realNumbers = true;
            }
            else
                velocityTarget = Vector3.zero;


            Quaternion targetItemRotation = TargetItemRotation(attachedObjectInfo);
            Quaternion rotationDelta = targetItemRotation * Quaternion.Inverse(attachedObjectInfo.attachedObject.transform.rotation);


            float angle;
            Vector3 axis;
            rotationDelta.ToAngleAxis(out angle, out axis);

            if (angle > 180)
                angle -= 360;

            if (angle != 0 && float.IsNaN(axis.x) == false && float.IsInfinity(axis.x) == false)
            {
                angularTarget = angle * axis * angularVelocityMagic * Time.deltaTime;

                if (noSteamVRFallbackCamera)
                    angularTarget /= 10; //hacky fix for fallback

                realNumbers &= true;
            }
            else
                angularTarget = Vector3.zero;

            return realNumbers;
        }

        public void SetFocus(bool hasFocus)
        {
            OnInputFocus(hasFocus);
        }

        //-------------------------------------------------
        protected virtual void OnInputFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                DetachObject(applicationLostFocusObject, true);
                applicationLostFocusObject.SetActive(false);
                UpdateHovering();
                BroadcastMessage("OnParentHandInputFocusAcquired", SendMessageOptions.DontRequireReceiver);
                var broadcastMessageToEvent = EcsRxApplicationBehaviour.Instance.Container.Resolve<IF_VR_Steam_IBroadcastMessageToEvent>();
                broadcastMessageToEvent.OnParentHandInputFocusAcquired(this.gameObject);
            }
            else
            {
                applicationLostFocusObject.SetActive(true);
                AttachObject(applicationLostFocusObject, IF_VR_Steam_GrabTypes.Scripted, AttachmentFlags.ParentToHand);
                BroadcastMessage("OnParentHandInputFocusLost", SendMessageOptions.DontRequireReceiver);
                var broadcastMessageToEvent = EcsRxApplicationBehaviour.Instance.Container.Resolve<IF_VR_Steam_IBroadcastMessageToEvent>();
                broadcastMessageToEvent.OnParentHandInputFocusLost(this.gameObject);
            }
        }

        //-------------------------------------------------
        protected virtual void OnDrawGizmos()
        {
            if (useHoverSphere && hoverSphereTransform != null)
            {
                Gizmos.color = Color.green;
                float scaledHoverRadius = hoverSphereRadius * Mathf.Abs(SteamVR_Utils.GetLossyScale(hoverSphereTransform));
                Gizmos.DrawWireSphere(hoverSphereTransform.position, scaledHoverRadius / 2);
            }

            if (useControllerHoverComponent && mainRenderModel != null && mainRenderModel.IsControllerVisibile())
            {
                Gizmos.color = Color.blue;
                float scaledHoverRadius = controllerHoverRadius * Mathf.Abs(SteamVR_Utils.GetLossyScale(this.transform));
                Gizmos.DrawWireSphere(mainRenderModel.GetControllerPosition(controllerHoverComponent), scaledHoverRadius / 2);
            }

            if (useFingerJointHover && mainRenderModel != null && mainRenderModel.IsHandVisibile())
            {
                Gizmos.color = Color.yellow;
                float scaledHoverRadius = fingerJointHoverRadius * Mathf.Abs(SteamVR_Utils.GetLossyScale(this.transform));
                Gizmos.DrawWireSphere(mainRenderModel.GetBonePosition((int)fingerJointHover), scaledHoverRadius / 2);
            }
        }


        //-------------------------------------------------
        private void HandDebugLog(string msg)
        {
            if (spewDebugText)
            {
                Debug.Log("<b>[SteamVR Interaction]</b> IF_VR_Steam_Hand (" + this.name + "): " + msg);
            }
        }


        //-------------------------------------------------
        // Continue to hover over this object indefinitely, whether or not the IF_VR_Steam_Hand moves out of its interaction trigger volume.
        //
        // interactable - The IF_VR_Steam_Interactable to hover over indefinitely.
        //-------------------------------------------------
        public void HoverLock(IF_VR_Steam_Interactable interactable)
        {
            if (spewDebugText)
                HandDebugLog("HoverLock " + interactable);
            hoverLocked = true;
            hoveringInteractable = interactable;
        }


        //-------------------------------------------------
        // Stop hovering over this object indefinitely.
        //
        // interactable - The hover-locked IF_VR_Steam_Interactable to stop hovering over indefinitely.
        //-------------------------------------------------
        public void HoverUnlock(IF_VR_Steam_Interactable interactable)
        {
            if (spewDebugText)
                HandDebugLog("HoverUnlock " + interactable);

            if (hoveringInteractable == interactable)
            {
                hoverLocked = false;
            }
        }

        public void TriggerHapticPulse(ushort microSecondsDuration)
        {
            if (trackedObject == null)
                return;

            float seconds = (float)microSecondsDuration / 1000000f;
            hapticAction.Execute(0, seconds, 1f / seconds, 1, handType);
        }

        public void TriggerHapticPulse(float duration, float frequency, float amplitude)
        {
            if (trackedObject == null)
                return;

            hapticAction.Execute(0, duration, frequency, amplitude, handType);
        }

        public void ShowGrabHint()
        {
            // ckbang - Hints
            //IF_VR_Steam_ControllerButtonHints.ShowButtonHint(this, grabGripAction); //todo: assess
        }

        public void HideGrabHint()
        {
            // ckbang - Hints
            //IF_VR_Steam_ControllerButtonHints.HideButtonHint(this, grabGripAction); //todo: assess
        }

        public void ShowGrabHint(string text)
        {
            // ckbang - Hints
            //IF_VR_Steam_ControllerButtonHints.ShowTextHint(this, grabGripAction, text);
        }

        public IF_VR_Steam_GrabTypes GetGrabStarting(IF_VR_Steam_GrabTypes explicitType = IF_VR_Steam_GrabTypes.None)
        {
            var grabStatusModule = EcsRxApplicationBehaviour.Instance.Container.Resolve<IF_VR_IGrabStatus>();
            return grabStatusModule.GetGrabStarting(gameObject.GetEntity(), explicitType.ConvertTo()).ConvertTo();
        }

        public IF_VR_Steam_GrabTypes GetGrabEnding(IF_VR_Steam_GrabTypes explicitType = IF_VR_Steam_GrabTypes.None)
        {
            var grabStatusModule = EcsRxApplicationBehaviour.Instance.Container.Resolve<IF_VR_IGrabStatus>();
            return grabStatusModule.GetGrabEnding(gameObject.GetEntity(), explicitType.ConvertTo()).ConvertTo();
        }

        public bool IsGrabEnding(GameObject attachedObject)
        {
            for (int attachedObjectIndex = 0; attachedObjectIndex < attachedObjects.Count; attachedObjectIndex++)
            {
                if (attachedObjects[attachedObjectIndex].attachedObject == attachedObject)
                {
                    return IsGrabbingWithType(attachedObjects[attachedObjectIndex].grabbedWithType) == false;
                }
            }

            return false;
        }

        public bool IsGrabbingWithType(IF_VR_Steam_GrabTypes type)
        {
            var grabStatusModule = EcsRxApplicationBehaviour.Instance.Container.Resolve<IF_VR_IGrabStatus>();
            return grabStatusModule.IsGrabbingWithType(gameObject.GetEntity(), type.ConvertTo());
        }

        public bool IsGrabbingWithOppositeType(IF_VR_Steam_GrabTypes type)
        {
            var grabStatusModule = EcsRxApplicationBehaviour.Instance.Container.Resolve<IF_VR_IGrabStatus>();
            return grabStatusModule.IsGrabbingWithOppositeType(gameObject.GetEntity(), type.ConvertTo());
        }

        public IF_VR_Steam_GrabTypes GetBestGrabbingType()
        {
            return GetBestGrabbingType(IF_VR_Steam_GrabTypes.None);
        }

        public IF_VR_Steam_GrabTypes GetBestGrabbingType(IF_VR_Steam_GrabTypes preferred, bool forcePreference = false)
        {
            var grabStatusModule = EcsRxApplicationBehaviour.Instance.Container.Resolve<IF_VR_IGrabStatus>();
            return grabStatusModule.GetBestGrabbingType(gameObject.GetEntity(), preferred.ConvertTo(), forcePreference).ConvertTo();
        }


        //-------------------------------------------------
        private void InitController()
        {
            if (spewDebugText)
                HandDebugLog("IF_VR_Steam_Hand " + name + " connected with type " + handType.ToString());

            bool hadOldRendermodel = mainRenderModel != null;
            EVRSkeletalMotionRange oldRM_rom = EVRSkeletalMotionRange.WithController;
            if (hadOldRendermodel)
                oldRM_rom = mainRenderModel.GetSkeletonRangeOfMotion;


            foreach (IF_VR_Steam_RenderModel r in renderModels)
            {
                if (r != null)
                    Destroy(r.gameObject);
            }

            renderModels.Clear();

            GameObject renderModelInstance = GameObject.Instantiate(renderModelPrefab);
            renderModelInstance.layer = gameObject.layer;
            renderModelInstance.tag = gameObject.tag;
            renderModelInstance.transform.parent = this.transform;
            renderModelInstance.transform.localPosition = Vector3.zero;
            renderModelInstance.transform.localRotation = Quaternion.identity;
            renderModelInstance.transform.localScale = renderModelPrefab.transform.localScale;

            //TriggerHapticPulse(800);  //pulse on controller init

            int deviceIndex = trackedObject.GetDeviceIndex();

            mainRenderModel = renderModelInstance.GetComponent<IF_VR_Steam_RenderModel>();
            renderModels.Add(mainRenderModel);

            if (hadOldRendermodel)
                mainRenderModel.SetSkeletonRangeOfMotion(oldRM_rom);

            var broadcastMessageToEvent = EcsRxApplicationBehaviour.Instance.Container.Resolve<IF_VR_Steam_IBroadcastMessageToEvent>();
            this.BroadcastMessage("SetInputSource", handType, SendMessageOptions.DontRequireReceiver); // let child objects know we've initialized
            broadcastMessageToEvent.SetInputSource(this.gameObject, handType);

            this.BroadcastMessage("OnHandInitialized", deviceIndex, SendMessageOptions.DontRequireReceiver); // let child objects know we've initialized
            broadcastMessageToEvent.OnHandInitialized(this.gameObject, deviceIndex);
        }

        public void SetRenderModel(GameObject prefab)
        {
            renderModelPrefab = prefab;

            if (mainRenderModel != null && isPoseValid)
                InitController();
        }

        public void SetHoverRenderModel(IF_VR_Steam_RenderModel hoverRenderModel)
        {
            hoverhighlightRenderModel = hoverRenderModel;
            renderModels.Add(hoverRenderModel);
        }

        public int GetDeviceIndex()
        {
            return trackedObject.GetDeviceIndex();
        }
    }


    [System.Serializable]
    public class IF_VR_Steam_HandEvent : UnityEvent<IF_VR_Steam_Hand> { }
}