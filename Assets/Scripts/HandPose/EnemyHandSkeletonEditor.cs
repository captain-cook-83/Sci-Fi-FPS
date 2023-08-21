using System;
using System.IO;
using Cc83.Utils;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Cc83.HandPose
{
    public partial class EnemyHandSkeleton
    {
#if UNITY_EDITOR
        private static readonly string DefaultPath = Path.Combine("Assets", "Generated", "HandPose");
        
        [TitleGroup("Weapon Pose Generator")]
        public Transform handTransform;

        private Transform GetAnchor()
        {
            return handSide switch
            {
                HandSide.Left => weaponRifle.secondaryAnchor,
                HandSide.Right => weaponRifle.primaryAnchor,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        [TitleGroup("Weapon Pose Generator")]
        [Button("GenerateWeaponPose", ButtonSizes.Large)]
        public void GenerateWeaponPose()
        {
            
            var interactableTransform = weaponRifle.transform;
            var data = ScriptableObject.CreateInstance<InteractablePoseData>();
            data.side = handSide;
            data.rotations = GetFingerNodes();
            
            if (Application.isPlaying)
            {
                var anchor = GetAnchor();
                data.handLocalPosition = anchor.localPosition;
                data.handLocalRotation = anchor.localRotation;
            }
            else
            {
                data.handLocalPosition = interactableTransform.InverseTransformPoint(handTransform.position);
                data.handLocalRotation = Quaternion.Inverse(interactableTransform.rotation) * handTransform.rotation;        // TODO 旋转存在问题，需要借助第二次（ Playing模式下的 IK ）生成校准
            }
            
            var savePath = EditorUtility.SaveFilePanelInProject("Save Path", interactableTransform.name + handSide + "WeaponPose", "asset", "Select to Save WeaponPoseData.", DefaultPath);
            if (string.IsNullOrEmpty(savePath))
            {
                Debug.Log("GenerateWeaponPose Canceled.");
                return;
            }
            
            AssetDatabase.CreateAsset(data, savePath);
            
            Selection.activeObject = data;
            Debug.Log("GenerateWeaponPose Completed.");
        }
        
        [TitleGroup("Weapon Pose Generator")]
        [Button("LoadWeaponPose", ButtonSizes.Large)]
        public void LoadWeaponPose()
        {
            var loadPath = EditorUtility.OpenFilePanel("Load Path", DefaultPath, "asset");
            if (string.IsNullOrEmpty(loadPath))
            {
                Debug.Log("LoadWeaponPose Canceled.");
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
            
            var interactableTransform = weaponRifle.transform;
            handTransform.localRotation = Quaternion.Inverse(handTransform.parent.rotation) * (interactableTransform.rotation * data.handLocalRotation);
            handTransform.position = interactableTransform.TransformPoint(data.handLocalPosition);
            
            var anchor = GetAnchor();
            anchor.localPosition = data.handLocalPosition;
            anchor.localRotation = data.handLocalRotation;
            
            SetFingerNodes(data);
            
            Debug.Log("LoadWeaponPose Completed.");
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
