﻿using System.Collections.Generic;
using UnityEngine;
using VRSF.Core.Raycast;

namespace VRSF.UI
{
    /// <summary>
    /// Create a new VRToggle based on the Toggle for Unity, but usable in VR with the Scriptable Framework
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class VRToggle : UnityEngine.UI.Toggle
    {
        #region VARIABLES
        [Tooltip("If you want to set the collider yourself, set this value to false.")]
        [SerializeField] public bool SetColliderAuto = true;

        [Tooltip("If this button can be click using a Raycast and the trigger of your controller.")]
        [SerializeField] public bool LaserClickable = true;

        [Tooltip("If this button can be click using the meshcollider of your controller.")]
        [SerializeField] public bool ControllerClickable = true;

        [Tooltip("Event raised when hovering the button with your gaze or one of the controller's laser.")]
        [SerializeField] public UnityEngine.Events.UnityEvent OnHover = new UnityEngine.Events.UnityEvent();

        [Tooltip("Event raised when you stop hovering the button with your gaze or one of the controller's laser.")]
        [SerializeField] public UnityEngine.Events.UnityEvent OnStopHovering = new UnityEngine.Events.UnityEvent();
        #endregion VARIABLES


        #region MONOBEHAVIOUR_METHODS
        protected override void Awake()
        {
            base.Awake();
            if (Application.isPlaying)
            {
                if (!VRRaycastAuthoring.SceneContainsRaycaster())
                    OnVRRaycasterIsSetup.Listeners += SetupToggle;
                else
                    SetupToggle(null);
            }     
        }

        protected override void Start()
        {
            base.Start();
            // We setup the BoxCollider size and center
            if (Application.isPlaying && SetColliderAuto)
                StartCoroutine(SetupBoxCollider());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (OnStartHoveringObject.IsMethodAlreadyRegistered(CheckHoveredObject))
            {
                OnStartHoveringObject.Listeners -= CheckHoveredObject;
                OnStopHoveringObject.Listeners -= CheckUnhoveredObject;
                OnVRClickerStartClicking.Listeners -= CheckClickedObject;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!ControllerClickable || !interactable)
                return;

            if (other.gameObject.CompareTag("ControllerBody") || other.gameObject.CompareTag("UIClicker"))
            {
                isOn = !isOn;
                UIHapticGenerator.CreateClickHapticSignal(other.name.ToLower().Contains("left") ? Core.Raycast.ERayOrigin.LEFT_HAND : Core.Raycast.ERayOrigin.RIGHT_HAND);
            }
        }
        #endregion MONOBEHAVIOUR_METHODS


        #region PRIVATE_METHODS
        /// <summary>
        /// Event called when the user is clicking on something
        /// </summary>
        /// <param name="startClickingEvent">The event raised when an object was clicked</param>
        private void CheckClickedObject(OnVRClickerStartClicking startClickingEvent)
        {
            if (CheckGameObject(startClickingEvent.ClickedObject))
            {
                isOn = !isOn;
                UIHapticGenerator.CreateClickHapticSignal(startClickingEvent.RaycastOrigin);
            }
        }

        private void CheckHoveredObject(OnStartHoveringObject startHoveringEvent)
        {
            if (CheckGameObject(startHoveringEvent.HoveredObject))
            {
                OnHover.Invoke();
                UIHapticGenerator.CreateClickHapticSignal(startHoveringEvent.RaycastOrigin);
            }
        }

        private void CheckUnhoveredObject(OnStopHoveringObject info)
        {
            if (CheckGameObject(info.UnhoveredObject))
                OnStopHovering.Invoke();
        }

        private bool CheckGameObject(GameObject toCheck)
        {
            return interactable && toCheck == gameObject;
        }

        /// <summary>
        /// Setup the BoxCOllider size and center by colling the NotScrollableSetup method CheckBoxColliderSize.
        /// We use a coroutine and wait for the end of the first frame as the element cannot be correctly setup on the first frame
        /// </summary>
        /// <returns></returns>
        private IEnumerator<WaitForEndOfFrame> SetupBoxCollider()
        {
            yield return new WaitForEndOfFrame();
            VRUIBoxColliderSetup.CheckBoxColliderSize(GetComponent<BoxCollider>(), GetComponent<RectTransform>());
        }

        private void SetupToggle(OnVRRaycasterIsSetup _)
        {
            if (OnVRRaycasterIsSetup.IsMethodAlreadyRegistered(SetupToggle))
                OnVRRaycasterIsSetup.Listeners -= SetupToggle;

            if (LaserClickable)
            {
                OnStartHoveringObject.Listeners += CheckHoveredObject;
                OnStopHoveringObject.Listeners += CheckUnhoveredObject;
                OnVRClickerStartClicking.Listeners += CheckClickedObject;
            }

            if (ControllerClickable)
                GetComponent<BoxCollider>().isTrigger = true;
        }
        #endregion PRIVATE_METHODS
    }
}