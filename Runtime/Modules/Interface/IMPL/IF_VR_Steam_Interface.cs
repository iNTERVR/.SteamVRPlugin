using UnityEngine;
using InterVR.IF.VR.Plugin.Steam.InteractionSystem;
using InterVR.IF.Extensions;
using InterVR.IF.VR.Components;
using EcsRx.Extensions;
using Valve.VR;
using EcsRx.Entities;
using InterVR.IF.Components;
using InterVR.IF.Blueprints;
using EcsRx.Infrastructure;
using EcsRx.Zenject;
using InterVR.IF.VR.Modules;
using UniRx;
using InterVR.IF.VR.Defines;
using System;

namespace InterVR.IF.VR.Plugin.Steam.Modules
{
    public class IF_VR_Steam_Interface : IF_VR_IInterface, IDisposable
    {
        public IF_VR_Steam_Interface()
        {
            HeadsetOnHead = new BoolReactiveProperty();
        }

        public void Dispose()
        {
            HeadsetOnHead.Dispose();
        }

        public BoolReactiveProperty HeadsetOnHead { get; private set; }

        public int HandCount
        {
            get
            {
                return IF_VR_Steam_Player.instance.handCount;
            }
        }

        public GameObject Rig
        {
            get
            {
                return CurrentRigType == IF_VR_RigType.VR ? IF_VR_Steam_Player.instance.rigSteamVR
                                                          : IF_VR_Steam_Player.instance.rig2DFallback;
            }
        }

        public Transform HMDTransform
        {
            get
            {
                return IF_VR_Steam_Player.instance.hmdTransform;
            }
        }

        public float EyeHeight
        {
            get
            {
                return IF_VR_Steam_Player.instance.eyeHeight;
            }
        }

        public Vector3 FeetPosition
        {
            get
            {
                return IF_VR_Steam_Player.instance.feetPositionGuess;
            }
        }

        public Vector3 BodyDirection
        {
            get
            {
                return IF_VR_Steam_Player.instance.bodyDirectionGuess;
            }
        }

        public Collider HeadCollider
        {
            get
            {
                return IF_VR_Steam_Player.instance.headCollider;
            }
        }

        public IF_VR_RigType CurrentRigType
        {
            get
            {
                if (IF_VR_Steam_Player.instance.rigSteamVR.activeSelf)
                    return IF_VR_RigType.VR;
                return IF_VR_RigType.Fallback;
            }
        }

        public IEntity GetCameraEntity()
        {
            return HMDTransform.gameObject.GetEntity();
        }

        public IEntity GetHandEntity(IF_VR_HandType type)
        {
            if (type == IF_VR_HandType.Left)
                return IF_VR_Steam_Player.instance.leftHand.gameObject.GetEntity();
            else if (type == IF_VR_HandType.Right)
                return IF_VR_Steam_Player.instance.rightHand.gameObject.GetEntity();
            return null;
        }

        public IEntity GetHandTrackerEntity(IF_VR_HandType type)
        {
            IF_VR_Steam_Hand hand = null;
            if (type == IF_VR_HandType.Left)
                hand = IF_VR_Steam_Player.instance.leftHand;
            else if (type == IF_VR_HandType.Right)
                hand = IF_VR_Steam_Player.instance.rightHand;

            if (hand == null)
                return null;

            return hand.trackedObject.gameObject.GetEntity();
        }

        public IEntity GetRigEntity()
        {
            return Rig.GetEntity();
        }
    }
}