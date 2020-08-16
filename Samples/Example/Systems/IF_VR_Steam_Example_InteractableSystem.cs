using EcsRx.Groups;
using EcsRx.Events;
using System.Collections.Generic;
using System;
using EcsRx.Collections.Database;
using InterVR.IF.VR.Components;

namespace InterVR.IF.VR.Plugin.Steam.Example.System
{
    public class IF_VR_Steam_Example_InteractableSystem
    {
        public IGroup Group => new Group(typeof(IF_VR_Interactable));

        private List<IDisposable> subscriptions = new List<IDisposable>();
        private readonly IEventSystem eventSystem;
        private readonly IEntityDatabase entityDatabase;

        public IF_VR_Steam_Example_InteractableSystem(IEventSystem eventSystem,
            IEntityDatabase entityDatabase)
        {
            this.eventSystem = eventSystem;
            this.entityDatabase = entityDatabase;
        }
   }
}