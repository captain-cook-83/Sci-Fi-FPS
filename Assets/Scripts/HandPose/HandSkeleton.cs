using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace Cc83.HandPose
{
    public class HandSkeleton : MonoBehaviour
    {
        private static readonly string DefaultPath = Path.Combine("Assets", "Generated", "HandPose");
        
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

        private void CalculateFingerNodes(IReadOnlyCollection<int> indexes, float value)
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

#if UNITY_EDITOR
        #region Editor Only
        private void OnValidate()
        {
            if (defaultPoseData != null && defaultPoseData.side != handSide)
            {
                Debug.LogError($"Mismatch for handSide({handSide}) and defaultPoseData.side({defaultPoseData.side})");
            }
            
            if (fistPoseData != null && fistPoseData.side != handSide)
            {
                Debug.LogError($"Mismatch for handSide({handSide}) and fistPoseData.side({fistPoseData.side})");
            }
        }

        [TitleGroup("Default Pose Operations")]
        [Button("ExportHandPose", ButtonSizes.Large)]
        public void ExportHandPose()
        {
            var data = defaultPoseData;
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<HandPoseData>();
                data.side = handSide;
            }
            else
            {
                if (data.side != handSide)
                {
                    EditorUtility.DisplayDialog("Mismatch Error", "The handSide value mismatch for the component and the poseData.", "Close");
                    return;
                }
            }

            data.rotations = GetFingerNodes();

            if (defaultPoseData == null)
            {
                var savePath = EditorUtility.SaveFilePanelInProject("Save Path", handSide + "HandPose", "asset", "Select to Save HandPoseData.", DefaultPath);
                if (string.IsNullOrEmpty(savePath))
                {
                    Debug.Log("ExportHandPoseData Canceled.");
                    return;
                }
                
                AssetDatabase.CreateAsset(data, savePath);
                defaultPoseData = data;
            }
            else
            {
                EditorUtility.SetDirty(data);
                AssetDatabase.SaveAssetIfDirty(data);
            }

            Selection.activeObject = data;
            Debug.Log("ExportHandPoseData Completed.");
        }

        [TitleGroup("Default Pose Operations")]
        [Button("ResetHandPose", ButtonSizes.Large)]
        public void ResetHandPose()
        {
            if (defaultPoseData == null)
            {
                var loadPath = EditorUtility.OpenFilePanel("Load Path", DefaultPath, "asset");
                if (string.IsNullOrEmpty(loadPath))
                {
                    Debug.Log("ResetHandPose Canceled.");
                    return;
                }

                if ((loadPath = SelectAssetsPath(loadPath)) == null)
                {
                    return;
                }

                defaultPoseData = AssetDatabase.LoadAssetAtPath<HandPoseData>(loadPath);
            }
            
            if (defaultPoseData.side != handSide)
            {
                EditorUtility.DisplayDialog("Detected Error", "The handSide value mismatch for the component and the poseData.", "Close");
                return;
            }
            
            SetFingerNodes(defaultPoseData);
            Debug.Log("ResetHandPose Completed.");
        }

        [TitleGroup("Default Pose Operations")]
        [Button("DeleteHandPose", ButtonSizes.Large)]
        public void DeleteHandPose()
        {
            var path = AssetDatabase.GetAssetPath(defaultPoseData);
            if (path is { Length: > 0 })
            {
                AssetDatabase.DeleteAsset(path);
            }
                
            defaultPoseData = null;
            Debug.Log("DeleteHandPose Deleted.");
        }
        
        [TitleGroup("Other Pose Operations")]
        [Button("ExportOtherHandPose", ButtonSizes.Large)]
        public void ExportOtherHandPose()
        {
            var data = ScriptableObject.CreateInstance<HandPoseData>();
            data.side = handSide;
            data.rotations = GetFingerNodes();

            var savePath = EditorUtility.SaveFilePanelInProject("Save Path", handSide + "OtherHandPose", "asset", "Select to Save HandPoseData.", DefaultPath);
            if (string.IsNullOrEmpty(savePath))
            {
                Debug.Log("ExportOtherHandPoseData Canceled.");
                return;
            }
            
            var fistPoseDataPath = fistPoseData != null ? AssetDatabase.GetAssetPath(fistPoseData) : null;
            AssetDatabase.CreateAsset(data, savePath);
            
            if (fistPoseDataPath != null && fistPoseDataPath.Equals(savePath))
            {
                fistPoseData = AssetDatabase.LoadAssetAtPath<HandPoseData>(fistPoseDataPath);
            }
            
            Selection.activeObject = data;
            Debug.Log("ExportOtherHandPoseData Completed.");
        }

        [TitleGroup("Other Pose Operations")]
        [Button("LoadOtherHandPose", ButtonSizes.Large)]
        public void LoadOtherHandPose()
        {
            var loadPath = EditorUtility.OpenFilePanel("Load Path", DefaultPath, "asset");
            if (string.IsNullOrEmpty(loadPath))
            {
                Debug.Log("LoadOtherHandPose Canceled.");
                return;
            }

            if ((loadPath = SelectAssetsPath(loadPath)) == null)
            {
                return;
            }

            var data = AssetDatabase.LoadAssetAtPath<HandPoseData>(loadPath);
            if (data.side != handSide)
            {
                EditorUtility.DisplayDialog("Detected Error", "The handSide value mismatch for the component and the poseData.", "Close");
                return;
            }

            SetFingerNodes(data);
            Debug.Log("LoadOtherHandPose Completed.");
        }

        private void SetFingerNodes(HandPoseData data)
        {
            for (var i = 0; i < fingerNodes.Length; i++)
            {
                fingerNodes[i].localRotation = data.rotations[i];
            }
        }

        private Quaternion[] GetFingerNodes()
        {
            var data = new Quaternion[fingerNodes.Length];
            for (var i = 0; i < fingerNodes.Length; i++)
            {
                data[i] = fingerNodes[i].localRotation;
            }

            return data;
        }

        private static string SelectAssetsPath(string filePath)
        {
            var index = filePath.IndexOf("Assets", StringComparison.Ordinal);
            if (index == -1)
            {
                EditorUtility.DisplayDialog("Path Error", $"Invalid asset path at {filePath}", "Close");
                return null;
            }

            return filePath[index..];
        }
        #endregion
#endif
    }
}
