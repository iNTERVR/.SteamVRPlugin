using EcsRx.Groups;
using EcsRx.Systems;
using EcsRx.Groups.Observable;
using UnityEngine;
using EcsRx.Events;
using InterVR.IF.Events;
using UniRx;
using System.Collections.Generic;
using System;
using EcsRx.Extensions;
using Valve.VR.InteractionSystem;
using EcsRx.Collections.Database;
using EcsRx.Collections.Entity;
using EcsRx.Unity.Extensions;
using InterVR.IF.VR.Components;
using UnityEngine.Assertions;
using InterVR.IF.Extensions;
using Valve.VR;

namespace InterVR.IF.VR.Plugin.Steam.Systems
{
    public class IF_VR_Steam_BuildComponentsSystem : IManualSystem
    {
        public IGroup Group => new EmptyGroup();

        private List<IDisposable> subscriptions = new List<IDisposable>();
        private readonly IEventSystem eventSystem;
        private readonly IEntityDatabase entityDatabase;

        public IF_VR_Steam_BuildComponentsSystem(IEventSystem eventSystem,
            IEntityDatabase entityDatabase)
        {
            this.eventSystem = eventSystem;
            this.entityDatabase = entityDatabase;
        }

        public void StartSystem(IObservableGroup observableGroup)
        {
            eventSystem.Receive<IF_ApplicationStartedEvent>().Subscribe(evt =>
            {
                //startBuild();
            }).AddTo(subscriptions);
        }

        public void StopSystem(IObservableGroup observableGroup)
        {
            subscriptions.DisposeAll();
        }

        void startBuild()
        {
            buildRig();
            buildCamera();
            buildHands();
            buildTrackers();
            buildInteractables();
            buildThrowables();
        }

        void buildRig()
        {
            var player = Player.instance;
            Assert.IsNotNull(player, "Must SteamVR Player has been installed");

            var go = player.gameObject;
            var entity = go.GetEntity(true);
            var rigComponent = new IF_VR_Rig();
            entity.AddComponent(rigComponent);
        }

        void buildCamera()
        {
            var player = Player.instance;
            Assert.IsNotNull(player, "Must SteamVR Player has been installed");

            var go = player.hmdTransform.gameObject;
            var entity = go.GetEntity(true);
            var cameraComponent = new IF_VR_Camera();
            entity.AddComponent(cameraComponent);
        }

        void buildHands()
        {
            GameObject.FindObjectsOfType<Hand>(true).ForEach(hand =>
            {
                var go = hand.gameObject;
                var entity = go.GetEntity(true);
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
            });
        }

        void buildTrackers()
        {
            GameObject.FindObjectsOfType<SteamVR_Behaviour_Pose>(true).ForEach(pose =>
            {
                var go = pose.gameObject;
                var entity = go.GetEntity(true);
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
            });
        }

        void buildInteractables()
        {
            GameObject.FindObjectsOfType<Interactable>(true).ForEach(interactable =>
            {
                var go = interactable.gameObject;
                var entity = go.GetEntity(true);
                var interactableComponent = new IF_VR_Interactable();
                entity.AddComponent(interactableComponent);
            });
        }

        void buildThrowables()
        {
            GameObject.FindObjectsOfType<Throwable>(true).ForEach(throwable =>
            {
                var go = throwable.gameObject;
                var entity = go.GetEntity(true);
                var throwableComponent = new IF_VR_Throwable();
                entity.AddComponent(throwableComponent);
            });
        }
    }
}