using EcsRx.Entities;
using EcsRx.Unity.Extensions;
using EcsRx.Unity.MonoBehaviours;
using EcsRx.Zenject;
using InterVR.IF.VR.Defines;
using InterVR.IF.VR.Plugin.Steam.InteractionSystem;
using UnityEngine;
using Valve.VR;

namespace InterVR.IF.VR.Plugin.Steam.Extensions
{
    public static class IF_VR_Steam_HandTypeExtensions
    {
        public static SteamVR_Input_Sources ConvertTo(this IF_VR_HandType type)
        {
            if (type == IF_VR_HandType.Left)
            {
                return SteamVR_Input_Sources.LeftHand;
            }
            else if (type == IF_VR_HandType.Right)
            {
                return SteamVR_Input_Sources.RightHand;
            }
            return SteamVR_Input_Sources.Any;
        }

        public static IF_VR_HandType ConvertTo(this SteamVR_Input_Sources type)
        {
            if (type == SteamVR_Input_Sources.LeftHand)
            {
                return IF_VR_HandType.Left;
            }
            else if (type == SteamVR_Input_Sources.RightHand)
            {
                return IF_VR_HandType.Right;
            }
            return IF_VR_HandType.Any;
        }
    }
}