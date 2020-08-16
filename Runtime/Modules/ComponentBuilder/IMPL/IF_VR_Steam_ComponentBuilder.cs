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

namespace InterVR.IF.VR.Plugin.Steam.Modules
{
    public class IF_VR_Steam_ComponentBuilder : IF_VR_Steam_IComponentBuilder
    {
        public void Build<T>(T obj) where T : MonoBehaviour
        {
            if (typeof(T) == typeof(IF_VR_Steam_Player))
            {
                buildRig(obj.gameObject);
            }

            if (typeof(T) == typeof(IF_VR_Steam_Hand))
            {
                buildHand(obj.gameObject);
            }

            if (typeof(T) == typeof(IF_VR_Steam_Interactable))
            {
                buildInteractable(obj.gameObject);
            }
        }

        void buildRig(GameObject go)
        {
            var entity = go.GetEntity(true);
            var rigComponent = new IF_VR_Rig();
            entity.AddComponent(rigComponent);

            var player = go.GetComponent<IF_VR_Steam_Player>();
            buildCamera(player);
        }

        void buildCamera(IF_VR_Steam_Player player)
        {
            var go = player.hmdTransform.gameObject;
            var entity = go.GetEntity(true);
            var cameraComponent = new IF_VR_Camera();
            entity.AddComponent(cameraComponent);
        }

        void buildHand(GameObject go)
        {
            var entity = go.GetEntity(true);
            var hand = go.GetComponent<IF_VR_Steam_Hand>();
            var handComponent = new IF_VR_Hand();
            if (hand.handType == Valve.VR.SteamVR_Input_Sources.LeftHand)
            {
                handComponent.Type = Defines.IF_VR_HandType.Left;
            }
            else if (hand.handType == Valve.VR.SteamVR_Input_Sources.RightHand)
            {
                handComponent.Type = Defines.IF_VR_HandType.Right;
            }
            else
            {
                handComponent.Type = Defines.IF_VR_HandType.Any;
            }
            entity.AddComponent(handComponent);

            var pool = EcsRxApplicationBehaviour.Instance.EntityDatabase.GetCollection();
            pool.CreateEntity(new IF_FollowEntityBlueprint(IF.Defines.IF_UpdateMomentType.Update,
                hand.trackedObject.gameObject.GetEntity(),
                entity,
                true,
                true,
                Vector3.zero,
                Vector3.zero));
        }

        void buildInteractable(GameObject go)
        {
            var entity = go.GetEntity(true);
            var interactableComponent = new IF_VR_Interactable();
            entity.AddComponent(interactableComponent);
        }

        public void BuildTracker(IF_VR_Steam_Hand hand)
        {
            if (hand.handType == SteamVR_Input_Sources.Any)
                return;

            var poses = GameObject.FindObjectsOfType<SteamVR_Behaviour_Pose>(true);
            foreach (var pose in poses)
            {
                if (pose.inputSource != hand.handType)
                    continue;

                var go = pose.gameObject;
                var entity = go.GetEntity(true);
                if (entity.HasComponent<IF_VR_HandTracker>())
                    continue;

                var poseComponent = new IF_VR_HandTracker();
                if (pose.inputSource == SteamVR_Input_Sources.LeftHand)
                {
                    poseComponent.Type = Defines.IF_VR_HandType.Left;
                }
                else if (pose.inputSource == SteamVR_Input_Sources.RightHand)
                {
                    poseComponent.Type = Defines.IF_VR_HandType.Right;
                }
                else
                {
                    poseComponent.Type = Defines.IF_VR_HandType.Any;
                }
                entity.AddComponent(poseComponent);
                hand.trackedObject = pose;
                break;
            }
        }
    }
}