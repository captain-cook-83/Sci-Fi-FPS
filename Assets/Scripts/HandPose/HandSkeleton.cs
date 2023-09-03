using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using InputDevice = UnityEngine.XR.InputDevice;

namespace Cc83.HandPose
{
    public partial class HandSkeleton : MonoBehaviour
    {
        private const int FingerNodeCount = 3;

        private static readonly int[] SelectFingers = { 2, 3, 4 };

        private static readonly int[] ActiveFingers = { 1 };

        private static readonly int[] ThumbFingers = { 0 };

        [Range(0.001f, 0.1f)]
        public float animateThreshold = 0.01f;

        [Range(5, 15)]
        public float animateSpeed = 10;

        public HandSide handSide;
        public ActionBasedController controller;
        
        public InputActionReference thumbActionReference;
        
        public InputActionReference thumbTouchedActionReference;
        
        public HandPoseData defaultPoseData;
        public HandPoseData fistPoseData;
        
        public Transform[] fingerNodes;
        
        // private InputDevice inputDevice;

        private HandPoseData selectPoseData;
        private HandPoseData activatePoseData;

        private float actuallyAnimateSpeed;
        
        private float targetSelectValue;
        private float targetActiveValue;
        private float  targetThumbValue;

        private float currentSelectValue;
        private float currentActiveValue;
        private float currentThumbValue;

        public void SetPoseData(HandPoseData sPoseData, HandPoseData aPoseData, float speed)
        {
            selectPoseData = sPoseData;
            activatePoseData = aPoseData;
            actuallyAnimateSpeed = speed;
            
            SetFingerNodes(selectPoseData);
        }

        public void ClearPoseData()
        {
            selectPoseData = defaultPoseData;
            activatePoseData = fistPoseData;
            actuallyAnimateSpeed = animateSpeed;

            currentThumbValue = currentSelectValue = currentActiveValue = -1;        // 导致 Update 检测被强制执行
        }

        private void Start()
        {
            ClearPoseData();
        }

        private void Update()
        {
            // targetThumbValue = 0;
            //
            // if (inputDevice != default)
            // {
            //     if (inputDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out var secondaryButton) && secondaryButton)
            //     {
            //         targetThumbValue = 1;
            //     } 
            //     else if (inputDevice.TryGetFeatureValue(CommonUsages.secondaryTouch, out var secondaryTouch) && secondaryTouch)
            //     {
            //         targetThumbValue = 0.4f;
            //     }
            // }
            //
            
#if UNITY_EDITOR
            if (controller == null) return;
#endif
            
            if (Math.Abs(targetThumbValue - currentThumbValue) > animateThreshold)
            {
                currentThumbValue = Mathf.MoveTowards(currentThumbValue, targetThumbValue, Time.deltaTime * actuallyAnimateSpeed);
                CalculateFingerNodes(ThumbFingers, currentThumbValue);
            }
            
            targetSelectValue = controller.selectActionValue.action.ReadValue<float>();
            if (Math.Abs(targetSelectValue - currentSelectValue) > animateThreshold)
            {
                currentSelectValue = Mathf.MoveTowards(currentSelectValue, targetSelectValue, Time.deltaTime * actuallyAnimateSpeed);
                CalculateFingerNodes(SelectFingers, currentSelectValue);
            }

            targetActiveValue = controller.activateActionValue.action.ReadValue<float>();
            if (Math.Abs(targetActiveValue - currentActiveValue) > animateThreshold)
            {
                currentActiveValue = Mathf.MoveTowards(currentActiveValue, targetActiveValue, Time.deltaTime * actuallyAnimateSpeed);
                CalculateFingerNodes(ActiveFingers, currentActiveValue);
            }
        }

        private void OnEnable()
        {
            // InputDevices.deviceConnected += OnDeviceConnected;
            // InputDevices.deviceDisconnected  += OnDeviceDisconnected;

#if UNITY_EDITOR
            if (thumbActionReference == null || thumbTouchedActionReference == null) return;
#endif

            thumbActionReference.action.performed += OnThumbAction;
            thumbActionReference.action.canceled += OnThumbAction;
            thumbTouchedActionReference.action.performed += OnThumbTouchedAction;
            thumbTouchedActionReference.action.canceled += OnThumbTouchedAction;
        }

        private void OnDisable()
        {
            // InputDevices.deviceConnected -= OnDeviceConnected;
            // InputDevices.deviceDisconnected -= OnDeviceDisconnected;

#if UNITY_EDITOR
            if (thumbActionReference == null || thumbTouchedActionReference == null) return;
#endif
            
            thumbActionReference.action.performed -= OnThumbAction;
            thumbActionReference.action.canceled -= OnThumbAction;
            thumbTouchedActionReference.action.performed -= OnThumbTouchedAction;
            thumbTouchedActionReference.action.canceled -= OnThumbTouchedAction;
        }

        // private void OnDeviceConnected(InputDevice device)
        // {
        //     var inputDevices = new List<InputDevice>();
        //     var deviceSide = handSide == HandSide.Left
        //         ? InputDeviceCharacteristics.Left
        //         : InputDeviceCharacteristics.Right;
        //     
        //     InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.HeldInHand | deviceSide, inputDevices);
        //     inputDevice = inputDevices.Count > 0 ? inputDevices[0] : default;
        // }
        //
        // private void OnDeviceDisconnected(InputDevice device)
        // {
        //     inputDevice = default;
        // }

        private void OnThumbAction(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Performed:
                    targetThumbValue = 1;
                    break;
                case InputActionPhase.Canceled:
                    targetThumbValue = 0.5f;
                    break;
            }
        }
        
        private void OnThumbTouchedAction(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Performed:
                    targetThumbValue = 0.5f;
                    break;
                case InputActionPhase.Canceled:
                    targetThumbValue = 0;
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CalculateFingerNodes(IEnumerable<int> indexes, float value)
        {
            foreach (var index in indexes)
            {
                var baseIndex = index * FingerNodeCount;
                for (var n = 0; n < FingerNodeCount; n++)
                {
                    var nodeIndex = baseIndex + n;
                    var finger = fingerNodes[nodeIndex];
                    var idleValue = selectPoseData.rotations[nodeIndex];
                    var fistValue = activatePoseData.rotations[nodeIndex];

                    finger.localRotation = Quaternion.Lerp(idleValue, fistValue, value);
                }
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetFingerNodes(HandPoseData data)
        {
            for (var i = 0; i < fingerNodes.Length; i++)
            {
                fingerNodes[i].localRotation = data.rotations[i];
            }
        }
    }
}
