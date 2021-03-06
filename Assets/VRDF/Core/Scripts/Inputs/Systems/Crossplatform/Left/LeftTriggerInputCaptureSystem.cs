﻿using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using VRDF.Core.SetupVR;

namespace VRDF.Core.Inputs
{
    /// <summary>
    /// System common for the VR Headsets, capture the trigger inputs for the left controller
    /// </summary>
    public class LeftTriggerInputCaptureSystem : JobComponentSystem
    {
        private EndSimulationEntityCommandBufferSystem _endSimEcbSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            // Cache the EndSimulationEntityCommandBufferSystem in a field, so we don't have to get it every frame
            _endSimEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            OnSetupVRReady.Listeners += Init;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            OnSetupVRReady.Listeners -= Init;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var handle = new LeftTriggerInputCaptureJob
            {
                LeftTriggerSqueezeValue = Input.GetAxis("LeftTriggerSqueeze"),
                Commands = _endSimEcbSystem.CreateCommandBuffer().ToConcurrent()
            }.Schedule(this, inputDeps);

            handle.Complete();
            return handle;
        }

        [RequireComponentTag(typeof(LeftHand))]
        struct LeftTriggerInputCaptureJob : IJobForEachWithEntity<TriggerInputCapture, BaseInputCapture>
        {
            [ReadOnly] public float LeftTriggerSqueezeValue;

            public EntityCommandBuffer.Concurrent Commands;

            public void Execute(Entity entity, int index, ref TriggerInputCapture triggerInput, ref BaseInputCapture baseInput)
            {
                if (!baseInput.IsClicking && LeftTriggerSqueezeValue > triggerInput.SqueezeClickThreshold)
                {
                    Commands.AddComponent(index, entity, new StartClickingEventComp { HasWaitedOneFrameBeforeRemoval = false, ButtonInteracting = EControllersButton.TRIGGER });
                    baseInput.IsTouching = false;
                    baseInput.IsClicking = true;
                }
                else if (baseInput.IsClicking && LeftTriggerSqueezeValue < triggerInput.SqueezeClickThreshold)
                {
                    Commands.AddComponent(index, entity, new StopClickingEventComp { HasWaitedOneFrameBeforeRemoval = false, ButtonInteracting = EControllersButton.TRIGGER });
                    baseInput.IsClicking = false;
                    baseInput.IsTouching = true;
                }
                else if (!baseInput.IsClicking && !baseInput.IsTouching && LeftTriggerSqueezeValue > 0.0f)
                {
                    baseInput.IsTouching = true;
                    Commands.AddComponent(index, entity, new StartTouchingEventComp { HasWaitedOneFrameBeforeRemoval = false, ButtonInteracting = EControllersButton.TRIGGER });
                }
                else if (baseInput.IsTouching && LeftTriggerSqueezeValue == 0.0f)
                {
                    baseInput.IsTouching = false;
                    Commands.AddComponent(index, entity, new StopTouchingEventComp { HasWaitedOneFrameBeforeRemoval = false, ButtonInteracting = EControllersButton.TRIGGER });
                }
            }
        }

        private void Init(OnSetupVRReady info)
        {
            this.Enabled = VRDF_Components.DeviceLoaded != EDevice.SIMULATOR;
        }
    }
}