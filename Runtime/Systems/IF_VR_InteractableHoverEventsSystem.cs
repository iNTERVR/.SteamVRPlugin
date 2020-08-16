
using InterVR.IF.Components;
using InterVR.IF.Defines;
using EcsRx.Entities;
using EcsRx.Extensions;
using EcsRx.Groups;
using EcsRx.Plugins.ReactiveSystems.Systems;
using EcsRx.Unity.Extensions;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using EcsRx.Collections.Database;
using InterVR.IF.VR.Components;
using Valve.VR.InteractionSystem;

namespace InterVR.IF.VR.Plugin.Steam.Systems
{
    public class IF_VR_InteractableHoverEventsSystem : ISetupSystem, ITeardownSystem
    {
        public IGroup Group => new Group(typeof(IF_VR_Interactable));

        private Dictionary<IEntity, List<IDisposable>> subscriptionsPerEntity = new Dictionary<IEntity, List<IDisposable>>();
        private readonly IEntityDatabase entityDatabase;

        public IF_VR_InteractableHoverEventsSystem(IEntityDatabase entityDatabase)
        {
            this.entityDatabase = entityDatabase;
        }

        public void Setup(IEntity entity)
        {
            var subscriptions = new List<IDisposable>();
            subscriptionsPerEntity.Add(entity, subscriptions);

            var interactableHoverEvents = entity.GetUnityComponent<InteractableHoverEvents>();
            interactableHoverEvents.onHandHoverBegin.AsObservable().Subscribe(x =>
            {

            });
        }

        public void Teardown(IEntity entity)
        {
            if (subscriptionsPerEntity.TryGetValue(entity, out List<IDisposable> subscriptions))
            {
                subscriptions.DisposeAll();
                subscriptions.Clear();
                subscriptionsPerEntity.Remove(entity);
            }
        }
    }
}