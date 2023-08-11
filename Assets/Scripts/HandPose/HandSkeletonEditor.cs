using System.IO;
using Cc83.Utils;
using Sirenix.OdinInspector;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEngine;

namespace Cc83.HandPose
{
    public partial class HandSkeleton
    {
#if UNITY_EDITOR
        private static readonly string DefaultPath = Path.Combine("Assets", "Generated", "HandPose");
        
        [TitleGroup("Interactable Pose Generator")]
        public Transform handTransform;
        
        private InteractableReference interactableReference;

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

            if (interactableReference == null)
            {
                interactableReference = GetComponent<InteractableReference>();
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

                if ((loadPath = PathUtils.SelectAssetsPath(loadPath)) == null)
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

            if ((loadPath = PathUtils.SelectAssetsPath(loadPath)) == null)
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

        [TitleGroup("Interactable Pose Generator")]
        [Button("GenerateInteractablePose", ButtonSizes.Large)]
        public void GenerateInteractablePose()
        {
            if (interactableReference == null)
            {
                EditorUtility.DisplayDialog("Missing Component", "Add InteractableReference component and refer to a Interactable Transform.", "Close");
                return;
            }
            
            var interactableTransform = interactableReference.interactable;
            var data = ScriptableObject.CreateInstance<InteractablePoseData>();
            data.side = handSide;
            data.rotations = GetFingerNodes();
            data.handLocalPosition = handTransform.InverseTransformPoint(interactableTransform.position);

            var interactableParent = interactableTransform.parent;
            interactableTransform.SetParent(handTransform);
            data.handLocalRotation = interactableTransform.localRotation;
            interactableTransform.SetParent(interactableParent);
            
            var savePath = EditorUtility.SaveFilePanelInProject("Save Path", interactableTransform.name + handSide + "InteractablePose", "asset", "Select to Save InteractablePoseData.", DefaultPath);
            if (string.IsNullOrEmpty(savePath))
            {
                Debug.Log("ExportOtherHandPoseData Canceled.");
                return;
            }
            
            AssetDatabase.CreateAsset(data, savePath);
            
            Selection.activeObject = data;
            Debug.Log("GenerateInteractablePose Completed.");
        }
        
        [TitleGroup("Interactable Pose Generator")]
        [Button("LoadInteractablePose", ButtonSizes.Large)]
        public void LoadInteractablePose()
        {
            if (interactableReference == null)
            {
                EditorUtility.DisplayDialog("Missing Component", "Add InteractableReference component and refer to a Interactable Transform.", "Close");
                return;
            }
            
            var loadPath = EditorUtility.OpenFilePanel("Load Path", DefaultPath, "asset");
            if (string.IsNullOrEmpty(loadPath))
            {
                Debug.Log("LoadInteractablePose Canceled.");
                return;
            }

            if ((loadPath = PathUtils.SelectAssetsPath(loadPath)) == null)
            {
                return;
            }

            var data = AssetDatabase.LoadAssetAtPath<InteractablePoseData>(loadPath);
            if (data.side != handSide)
            {
                EditorUtility.DisplayDialog("Detected Error", "The handSide value mismatch for the component and the poseData.", "Close");
                return;
            }
            
            var interactableTransform = interactableReference.interactable;
            // var interactableParent = interactableTransform.parent;
            interactableTransform.SetParent(handTransform);
            interactableTransform.localRotation = data.handLocalRotation;
            interactableTransform.localPosition = data.handLocalPosition;
            // interactableTransform.SetParent(interactableParent);
            
            SetFingerNodes(data);
            
            Debug.Log("LoadInteractablePose Completed.");
        }

        [TitleGroup("Hand Shake Animation Generator")]
        [Button("Calculate Hand Shake Animation", ButtonSizes.Large)]
        public void CalculateHandShakeAnimation()
        {
            if (interactableReference == null)
            {
                EditorUtility.DisplayDialog("Missing Component", "Add InteractableReference component and refer to a Interactable Transform.", "Close");
                return;
            }
            
            var interactable = interactableReference.interactable;
            if (!ReferenceEquals(interactable.parent, handTransform))
            {
                interactable.SetParent(handTransform);
                Debug.Log($"Change the parent of {interactable.name} to {handTransform.name}");
            }
            
            var interactablePose = interactable.GetComponent<InteractablePose>();
            var interactablePoseData = handSide == HandSide.Left
                ? interactablePose.primaryLeftPose
                : interactablePose.primaryRightPose;

            var interactableLocalPr = interactable.GetLocalPose();
            
            var interactableShakeShadow = interactableReference.interactableShakeShadow;
            if (!ReferenceEquals(interactableShakeShadow.parent, interactable))
            {
                interactableShakeShadow.SetParent(interactable);
                Debug.Log($"Change the parent of {interactableShakeShadow.name} to {interactable.name}");
            }
            
            var upLocalPosition = interactableLocalPr.position + interactableLocalPr.rotation * interactableShakeShadow.localPosition;
            var upLocalRotation = interactableLocalPr.rotation * interactableShakeShadow.localRotation;
            
            // var eulerAngles = upLocalRotation.eulerAngles;
            // Debug.Log($"LocalPosition[x:{upLocalPosition.x}, y:{upLocalPosition.y}, z:{upLocalPosition.z}]");
            // Debug.Log($"LocalRotation[x:{eulerAngles.x}, y:{eulerAngles.y}, z:{eulerAngles.z}]");
            
            var handShadowRotation = upLocalRotation * Quaternion.Inverse(interactablePoseData.handLocalRotation);
            var handShadowPosition = upLocalPosition - handShadowRotation * interactablePoseData.handLocalPosition;
            
            // var eulerAngles = handShadowRotation.eulerAngles;
            // Debug.Log($"Inner Position Offset [x: {handShadowPosition.x}, y: {handShadowPosition.y}, z: {handShadowPosition.z}]");
            // Debug.Log($"Inner Rotation Offset [x: {eulerAngles.x}, y: {eulerAngles.y}, z: {eulerAngles.z}], {eulerAngles}");

            // var handShadow = new GameObject("HAND-SHADOW").transform;
            // handShadow.SetParent(handTransform);
            // handShadow.localPosition = handShadowPosition;
            // handShadow.localRotation = handShadowRotation;
            //
            // var interactableShadow = Instantiate(interactableShakeShadow.gameObject).transform;
            // interactableShadow.SetParent(handShadow);
            // interactableShadow.localPosition = interactablePoseData.handLocalPosition;
            // interactableShadow.localRotation = interactablePoseData.handLocalRotation;
            
            var angles = handShadowRotation.eulerAngles;

            #region 约束旋转角度

            if (angles.x > 180)
            {
                angles.x -= 360;
            } 
            else if (angles.x < -180)
            {
                angles.x += 360;
            }
            
            if (angles.y > 180)
            {
                angles.y -= 360;
            }
            else if (angles.y < -180)
            {
                angles.y += 360;
            }
            
            if (angles.z > 180)
            {
                angles.z -= 360;
            }
            else if (angles.z < -180)
            {
                angles.z += 360;
            }

            #endregion
            
            Debug.Log($"Position Offset [x: {handShadowPosition.x}, y: {handShadowPosition.y}, z: {handShadowPosition.z}]");
            Debug.Log($"Rotation Offset [x: {angles.x}, y: {angles.y}, z: {angles.z}], {angles}");
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
#endif
    }
}
