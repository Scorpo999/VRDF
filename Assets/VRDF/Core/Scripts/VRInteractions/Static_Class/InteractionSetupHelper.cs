﻿using Unity.Entities;
using UnityEngine;
using VRDF.Core.Controllers;
using VRDF.Core.Inputs;
using VRDF.Core.SetupVR;

namespace VRDF.Core.VRInteractions
{
    public static class InteractionSetupHelper
    {
        public static bool SetupInteractions(ref EntityManager entityManager, ref Entity entity, VRInteractionAuthoring interactionParameters)
        {
            // Add the corresponding input component for the selected button. If the button wasn't chose correctly, we destroy this entity and return.
            if (!AddInputCaptureComponent(ref entityManager, ref entity, interactionParameters))
            {
                Debug.Log("<Color=yellow><b>[VRDF] :</b> An error has occur while setting up the Input Capture Component. Please check that the right device is loaded, and your VRInteractionAuthoring parameters.</Color>", interactionParameters.gameObject);
                return false;
            }

            // If the Hand wasn't chose correctly, we destroy this entity and return.
            if (!AddButtonHand(ref entityManager, ref entity, interactionParameters.ButtonHand))
                return false;

            // Add the corresponding interaction type component for the selected button. If the interaction type wasn't chose correctly, we destroy this entity and return.
            if (!AddInteractionType(ref entityManager, ref entity, interactionParameters.InteractionType, interactionParameters.ButtonToUse))
                return false;

            return true;
        }

        /// <summary>
        /// Add the corresponding Input component for the selected button. 
        /// </summary>
        public static bool AddInputCaptureComponent(ref EntityManager entityManager, ref Entity entity, VRInteractionAuthoring interactionSet)
        {
            // Add the BaseInputCapture component to the entity
            if (entityManager.HasComponent<BaseInputCapture>(entity))
                entityManager.SetComponentData(entity, new BaseInputCapture());
            else
                entityManager.AddComponentData(entity, new BaseInputCapture());

            // Add the specific inputCapture component for our button
            switch (interactionSet.ButtonToUse)
            {
                case EControllersButton.A_BUTTON:
                    entityManager.AddComponentData(entity, new AButtonInputCapture());
                    return IsTwoHandOculusDevice() && interactionSet.ButtonHand == EHand.RIGHT;
                case EControllersButton.B_BUTTON:
                    entityManager.AddComponentData(entity, new BButtonInputCapture());
                    return IsTwoHandOculusDevice() && interactionSet.ButtonHand == EHand.RIGHT;
                case EControllersButton.X_BUTTON:
                    entityManager.AddComponentData(entity, new XButtonInputCapture());
                    return IsTwoHandOculusDevice() && interactionSet.ButtonHand == EHand.LEFT;
                case EControllersButton.Y_BUTTON:
                    entityManager.AddComponentData(entity, new YButtonInputCapture());
                    return IsTwoHandOculusDevice() && interactionSet.ButtonHand == EHand.LEFT;
                case EControllersButton.THUMBREST:
                    entityManager.AddComponentData(entity, new ThumbrestInputCapture(interactionSet.ButtonHand));
                    return IsTwoHandOculusDevice();

                case EControllersButton.BACK_BUTTON:
                    entityManager.AddComponentData(entity, new GoAndGearVRInputCapture());
                    return IsOneHandPortableDevice();

                case EControllersButton.TRIGGER:
                    entityManager.AddComponentData(entity, new TriggerInputCapture(interactionSet.ButtonHand));
                    return true;
                case EControllersButton.GRIP:
                    entityManager.AddComponentData(entity, new GripInputCapture(interactionSet.ButtonHand));
                    return true;
                case EControllersButton.MENU:
                    if (IsTwoHandOculusDevice() && interactionSet.ButtonHand == EHand.RIGHT)
                    {
                        Debug.LogError("<b>[VRDF] :</b> Menu button aren't supported on the Right Hand for Two Handed Oculus Devices.");
                        return false;
                    }

                    entityManager.AddComponentData(entity, new MenuInputCapture(interactionSet.ButtonHand));
                    return true;

                case EControllersButton.TOUCHPAD:
                    // We check that the thumbposition give in the inspector is not set as none
                    if (((interactionSet.InteractionType & EControllerInteractionType.TOUCH) == EControllerInteractionType.TOUCH && interactionSet.TouchThumbPosition != EThumbPosition.NONE) || 
                        ((interactionSet.InteractionType & EControllerInteractionType.CLICK) == EControllerInteractionType.CLICK && interactionSet.ClickThumbPosition != EThumbPosition.NONE)) 
                    {
                        entityManager.AddComponentData(entity, new TouchpadInputCapture(interactionSet.UseThumbPositionForTouch));
                        entityManager.AddComponentData(entity, new InteractionThumbPosition 
                        { 
                            TouchThumbPosition = interactionSet.TouchThumbPosition, 
                            IsTouchingThreshold = interactionSet.IsTouchingThreshold, 
                            ClickThumbPosition = interactionSet.ClickThumbPosition, 
                            IsClickingThreshold = interactionSet.IsClickingThreshold
                        });
                        return true;
                    }

                    Debug.LogError("<b>[VRDF] :</b> Please Specify valid Thumb Positions to use for your VR Interaction Authoring component.", interactionSet.gameObject);
                    return false;

                default:
                    Debug.LogError("<b>[VRDF] :</b> Please Specify valid buttons to use for your VR Interaction Authoring component.", interactionSet.gameObject);
                    return false;
            }
        }

        /// <summary>
        /// Add the corresponding hand component for the selected button. 
        /// </summary>
        public static bool AddButtonHand(ref EntityManager entityManager, ref Entity entity, EHand hand)
        {
            // Add the CBRA Hand component to the entity
            switch (hand)
            {
                case EHand.LEFT:
                    entityManager.AddComponentData(entity, new LeftHand());
                    return true;

                case EHand.RIGHT:
                    entityManager.AddComponentData(entity, new RightHand());
                    return true;

                // If the button hand wasn't set in editor, we destroy this entity and return.
                default:
                    Debug.LogError("<b>[VRDF] :</b> Please Specify valid Hands to use for your ControllersButtonResponseAssigners.");
                    return false;
            }
        }

        /// <summary>
        /// Add the corresponding InteractionType component for the selected button. 
        /// </summary>
        public static bool AddInteractionType(ref EntityManager entityManager, ref Entity entity, EControllerInteractionType interactionType, EControllersButton button)
        {
            // If the button hand wasn't set in editor, we destroy this entity and return.
            if (interactionType == EControllerInteractionType.NONE || !InteractionIsCompatibleWithButton(interactionType, button))
            {
                Debug.LogError("<b>[VRDF] :</b> Please Specify valid Interaction Types in your ControllersButtonResponseAssigners.");
                return false;
            }
            else
            {
                // set the CBRA Interaction Type component to the entity
                entityManager.AddComponentData(entity, new ControllersInteractionType
                {
                    InteractionType = interactionType,
                    HasTouchInteraction = (interactionType & EControllerInteractionType.TOUCH) == EControllerInteractionType.TOUCH,
                    HasClickInteraction = (interactionType & EControllerInteractionType.CLICK) == EControllerInteractionType.CLICK
                });

                return true;
            }
        }

        private static bool InteractionIsCompatibleWithButton(EControllerInteractionType interactionType, EControllersButton button)
        {
            switch (button)
            {
                case EControllersButton.THUMBREST: 
                    // Only work with Touch Feature and Two Hand Oculus devices
                    return IsTwoHandOculusDevice() && (interactionType & EControllerInteractionType.TOUCH) == EControllerInteractionType.TOUCH;

                case EControllersButton.GRIP:
                    switch (VRDF_Components.DeviceLoaded)
                    {
                        // No grip button
                        case EDevice.GEAR_VR:
                        case EDevice.OCULUS_GO:
                            return false;
                        // Touch and Click supported
                        case EDevice.OCULUS_QUEST:
                        case EDevice.OCULUS_RIFT:
                        case EDevice.OCULUS_RIFT_S:
                            return true;
                        default:
                            // If the user wanna use the touch feature on a non-Oculus or GearVR device, we display a warning
                            if ((interactionType & EControllerInteractionType.TOUCH) == EControllerInteractionType.TOUCH)
                                Debug.LogWarning("<b>[VRDF] :</b> Grip Button has only a touch callback on non-Oculus devices.");

                            return (interactionType & EControllerInteractionType.CLICK) == EControllerInteractionType.CLICK;
                    }

                case EControllersButton.MENU:
                    switch(VRDF_Components.DeviceLoaded)
                    {
                        // No menu button
                        case EDevice.GEAR_VR:
                        case EDevice.OCULUS_GO:
                            return false;
                        default:
                            // If the user wanna use the touch feature on a Menu button, we display a warning
                            if ((interactionType & EControllerInteractionType.TOUCH) == EControllerInteractionType.TOUCH)
                                Debug.LogWarning("<b>[VRDF] :</b> Menu Button has only a Click Callback.");

                            return (interactionType & EControllerInteractionType.CLICK) == EControllerInteractionType.CLICK;
                    }

                case EControllersButton.BACK_BUTTON:
                    // Only work with Click Feature and Portable VR
                    return (interactionType & EControllerInteractionType.CLICK) == EControllerInteractionType.CLICK;

                default:
                    return true;
            }
        }

        private static bool IsTwoHandOculusDevice()
        {
            return VRDF_Components.DeviceLoaded == EDevice.OCULUS_QUEST || VRDF_Components.DeviceLoaded == EDevice.OCULUS_RIFT || VRDF_Components.DeviceLoaded == EDevice.OCULUS_RIFT_S || VRDF_Components.DeviceLoaded == EDevice.SIMULATOR;
        }

        private static bool IsOneHandPortableDevice()
        {
            return VRDF_Components.DeviceLoaded == EDevice.GEAR_VR || VRDF_Components.DeviceLoaded == EDevice.OCULUS_GO || VRDF_Components.DeviceLoaded == EDevice.SIMULATOR;
        }

        public static bool FlagHasOculusDevice(EDevice flagToTest)
        {
            return flagToTest.HasFlag(EDevice.OCULUS_GO) || flagToTest.HasFlag(EDevice.OCULUS_QUEST) || flagToTest.HasFlag(EDevice.OCULUS_RIFT) || flagToTest.HasFlag(EDevice.OCULUS_RIFT_S) || flagToTest.HasFlag(EDevice.SIMULATOR);
        }
    }
}
