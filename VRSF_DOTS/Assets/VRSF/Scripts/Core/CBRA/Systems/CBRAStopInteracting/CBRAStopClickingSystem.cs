﻿using Unity.Entities;
using VRSF.Core.Inputs;

namespace VRSF.Core.CBRA
{
    /// <summary>
    /// TODO
    /// </summary>
    public class CBRAStopClickingSystem : ComponentSystem
    {
        private EntityManager _entityManager;

        protected override void OnCreate()
        {
            // Cache the EntityManager in a field, so we don't have to get it every frame
            _entityManager = World.Active.EntityManager;
        }

        protected override void OnUpdate()
        {
            Entities.ForEach((Entity entity, ref CBRAInteractionType cbraInteractionType, ref StopClickingEventComp stopClickingEvent) =>
            {
                if (_entityManager.HasComponent(entity, CBRAInputTypeGetter.GetTypeFor(stopClickingEvent.ButtonInteracting)))
                    CBRADelegatesHolder.StopClickingEvents[entity]?.Invoke();

                PostUpdateCommands.RemoveComponent(entity, typeof(StopClickingEventComp));
            });
        }
    }
}