using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

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
        
        public Transform[] fingerNodes;

        public HandPoseData defaultPoseData;
        public HandPoseData fistPoseData;
        
        private InputDevice inputDevice;
        
        private float targetSelectValue;
        private float targetActiveValue;
        private float  targetThumbValue;

        private float currentSelectValue;
        private float currentActiveValue;
        private float currentThumbValue;

        public void SetPoseData(HandPoseData data)
        {
            SetFingerNodes(data);
        }

        public void ClearPoseData()
        {
            SetFingerNodes(defaultPoseData);
        }
        
        private void Update()
        {
            targetThumbValue = 0;
            
            if (inputDevice != default)
            {
                if (inputDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out var secondaryButton) && secondaryButton)
                {
                    targetThumbValue = 1;
                } 
                else if (inputDevice.TryGetFeatureValue(CommonUsages.secondaryTouch, out var secondaryTouch) && secondaryTouch)
                {
                    targetThumbValue = 0.4f;
                }
            }
            
            if (Math.Abs(targetThumbValue - currentThumbValue) > animateThreshold)
            {
                currentThumbValue = Mathf.MoveTowards(currentThumbValue, targetThumbValue, Time.deltaTime * animateSpeed);
                CalculateFingerNodes(ThumbFingers, currentThumbValue);
            }
            
            targetSelectValue = controller.selectAction.action.ReadValue<float>();
            if (Math.Abs(targetSelectValue - currentSelectValue) > animateThreshold)
            {
                currentSelectValue = Mathf.MoveTowards(currentSelectValue, targetSelectValue, Time.deltaTime * animateSpeed);
                CalculateFingerNodes(SelectFingers, currentSelectValue);
            }

            targetActiveValue = controller.activateAction.action.ReadValue<float>();
            if (Math.Abs(targetActiveValue - currentActiveValue) > animateThreshold)
            {
                currentActiveValue = Mathf.MoveTowards(currentActiveValue, targetActiveValue, Time.deltaTime * animateSpeed);
                CalculateFingerNodes(ActiveFingers, currentActiveValue);
            }
        }

        private void OnEnable()
        {
            InputDevices.deviceConnected += OnDeviceConnected;
            InputDevices.deviceDisconnected  += OnDeviceDisconnected;
        }

        private void OnDisable()
        {
            InputDevices.deviceConnected -= OnDeviceConnected;
            InputDevices.deviceDisconnected -= OnDeviceDisconnected;
        }

        private void OnDeviceConnected(InputDevice device)
        {
            var inputDevices = new List<InputDevice>();
            var deviceSide = handSide == HandSide.Left
                ? InputDeviceCharacteristics.Left
                : InputDeviceCharacteristics.Right;
            
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.HeldInHand | deviceSide, inputDevices);
            inputDevice = inputDevices.Count > 0 ? inputDevices[0] : default;
        }

        private void OnDeviceDisconnected(InputDevice device)
        {
            inputDevice = default;
        }

        private void CalculateFingerNodes(IEnumerable<int> indexes, float value)
        {
            foreach (var index in indexes)
            {
                var baseIndex = index * FingerNodeCount;
                for (var n = 0; n < FingerNodeCount; n++)
                {
                    var nodeIndex = baseIndex + n;
                    var finger = fingerNodes[nodeIndex];
                    var idleValue = defaultPoseData.rotations[nodeIndex];
                    var fistValue = fistPoseData.rotations[nodeIndex];

                    finger.localRotation = Quaternion.Lerp(idleValue, fistValue, value);
                }
            }
        }
    }
}
