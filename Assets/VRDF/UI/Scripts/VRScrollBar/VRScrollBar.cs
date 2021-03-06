﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRDF.Core.Raycast;
using VRDF.Core.VRInteractions;

namespace VRDF.UI
{
    /// <summary>
    /// Create a new ScrollBar based on the Unity scrollbar, but adapted for VR with the Scriptable Framework
    /// </summary>
    [RequireComponent(typeof(Collider), typeof(Image))]
    public class VRScrollBar : Scrollbar
    {
        #region PUBLIC_VARIABLES
        [Tooltip("If you want to set the collider yourself, set this value to false.")]
        [SerializeField] public bool SetColliderAuto = true;
        #endregion


        #region PRIVATE_VARIABLES
        private Transform _minPosBar;
        private Transform _maxPosBar;

        private ERayOrigin _rayHoldingHandle = ERayOrigin.NONE;

        private VRUIScrollableSetup _scrollableSetup;
        #endregion


        #region MONOBEHAVIOUR_METHODS
        protected override void Awake()
        {
            base.Awake();
            if (Application.isPlaying)
                ScrollbarSetup();
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
            if (OnVRClickerStartClicking.IsCallbackRegistered(CheckClickedObject))
            {
                OnVRClickerStartClicking.Listeners -= CheckClickedObject;
                OnVRClickerStopClicking.Listeners -= CheckUnclickedObject;
            }
        }

        protected override void Update()
        {
            base.Update();
            if (Application.isPlaying && _rayHoldingHandle != ERayOrigin.NONE)
                value = _scrollableSetup.SetComponentNewValue(_minPosBar.position, _maxPosBar.position, InteractionVariableContainer.GetCurrentHitPosition(_rayHoldingHandle));
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            VRRaycastAuthoring.CheckSceneContainsRaycaster();
        }
#endif
        #endregion


        #region PRIVATE_METHODS
        /// <summary>
        /// Event called when the user is clicking on something
        /// </summary>
        /// <param name="startClickingEvent">The event raised when an object was clicked</param>
        private void CheckClickedObject(OnVRClickerStartClicking startClickingEvent)
        {
            if (CanHoldHandle())
            {
                _rayHoldingHandle = startClickingEvent.RaycastOrigin;
                UIHapticGenerator.CreateClickHapticSignal(_rayHoldingHandle);
            }

            bool CanHoldHandle()
            {
                return interactable && startClickingEvent.ClickedObject == gameObject && _rayHoldingHandle == ERayOrigin.NONE;
            }
        }

        private void CheckUnclickedObject(OnVRClickerStopClicking stopClickingEvent)
        {
            if (stopClickingEvent.UnclickedObject == gameObject)
                _rayHoldingHandle = ERayOrigin.NONE;
        }

        /// <summary>
        /// Start a coroutine that wait for the second frame to set the BoxCollider
        /// </summary>
        /// <returns></returns>
        private IEnumerator<WaitForEndOfFrame> SetupBoxCollider()
        {
            yield return new WaitForEndOfFrame();
            VRUISetupHelper.CheckBoxColliderSize(GetComponent<BoxCollider>(), GetComponent<RectTransform>());
        }

        private void ScrollbarSetup()
        {
            GetHandleRectReference();

            OnSetupVRReady.RegisterSetupVRCallback(CheckDevice);

            _scrollableSetup = new VRUIScrollableSetup(UnityUIToVRDFUI.ScrollbarDirectionToUIDirection(direction));
            // Check if the Min and Max object are already created, and set there references
            _scrollableSetup.CheckMinMaxGameObjects(handleRect.parent, UnityUIToVRDFUI.ScrollbarDirectionToUIDirection(direction), ref _minPosBar, ref _maxPosBar);

            value = 1;
        }

        private void CheckDevice(OnSetupVRReady info)
        {
            if (OnSetupVRReady.IsCallbackRegistered(CheckDevice))
                OnSetupVRReady.Listeners -= CheckDevice;

            if (VRDF_Components.DeviceLoaded != Core.SetupVR.EDevice.SIMULATOR || VRUISetupHelper.ShouldRegisterForSimulator(this))
            {
                OnVRClickerStartClicking.Listeners += CheckClickedObject;
                OnVRClickerStopClicking.Listeners += CheckUnclickedObject;
            }
        }

        /// <summary>
        /// Try to get the handleRect rectTransform reference by finding the Handle deepChild of this GameObject
        /// </summary>
        private void GetHandleRectReference()
        {
            try
            {
                if (handleRect == null)
                    handleRect = transform.FindDeepChild("Handle").GetComponent<RectTransform>();
            }
            catch
            {
                Debug.LogError("<b>[VRDF] :</b> Please specify a HandleRect in the inspector as a child of this VR Handle Slider.", gameObject);
            }
        }
        #endregion
    }
}