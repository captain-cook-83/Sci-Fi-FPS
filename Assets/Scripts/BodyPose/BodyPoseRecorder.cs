using System.IO;
using Cc83.Utils;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Cc83.BodyPose
{
    public class BodyPoseRecorder : MonoBehaviour
    {
#if UNITY_EDITOR
        private static readonly string DefaultPath = Path.Combine("Assets", "Generated", "BodyPose");
        
        public Transform[] boneNodes;

        [Button("ExportBodyPose", ButtonSizes.Large)]
        public void ExportBodyPose()
        {
            var bodyPoseData = ScriptableObject.CreateInstance<BodyPoseData>();
            var savePath = EditorUtility.SaveFilePanelInProject("Save Path", "BodyPose", "asset", "Select to Save BodyPoseData.", DefaultPath);
            if (string.IsNullOrEmpty(savePath))
            {
                Debug.Log("ExportBodyPose Canceled.");
                return;
            }

            bodyPoseData.rotations = new Quaternion[boneNodes.Length];
            for (var i = 0; i < boneNodes.Length; i++)
            {
                bodyPoseData.rotations[i] = boneNodes[i].localRotation;
            }
                
            AssetDatabase.CreateAsset(bodyPoseData, savePath);
            Selection.activeObject = bodyPoseData;
            Debug.Log("ExportBodyPose Completed.");
        }

        [Button("LoadBodyPose", ButtonSizes.Large)]
        public void LoadBodyPose()
        {
            var loadPath = EditorUtility.OpenFilePanel("Load Path", DefaultPath, "asset");
            if (string.IsNullOrEmpty(loadPath))
            {
                Debug.Log("LoadBodyPose Canceled.");
                return;
            }

            if ((loadPath = PathUtils.SelectAssetsPath(loadPath)) == null)
            {
                return;
            }

            var bodyPoseData = AssetDatabase.LoadAssetAtPath<BodyPoseData>(loadPath);
            for (var i = 0; i < boneNodes.Length; i++)
            {
                boneNodes[i].localRotation = bodyPoseData.rotations[i];
            }
            Debug.Log("LoadBodyPose Completed.");
        }
#endif
    }
}
