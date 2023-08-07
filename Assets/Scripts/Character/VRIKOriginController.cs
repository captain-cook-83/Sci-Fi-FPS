using System;
using RootMotion.FinalIK;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Assertions;

namespace Cc83.Character
{
    public enum OriginScaleMode
    {
        Origin,
        Character
    }
    
     /// <summary>
        /// 启动时（Start 回调阶段）检测 XR 玩家头部高度与 Avatar 角色高度比例，以便调整 XR 玩家比例来适应角色
        /// </summary>
        [RequireComponent(typeof(VRIK))]

    public class VRIKOriginController : MonoBehaviour
    {
        public XROrigin xrOrigin;

        public OriginScaleMode scaleMode;

        [Tooltip("If the value of scaleMode is 'Origin', control the scaling of Camera View.")]
        public bool scaleCameraView;

        private VRIK ik;

        private float initHeadPosition;
        private float initHeadTargetPosition;

        private Transform xrOriginCamera;

        private void Awake()
        {
            Assert.AreEqual(xrOrigin.RequestedTrackingOriginMode, XROrigin.TrackingOriginMode.Floor,
                "Tracking Origin Mode of XR Origin Component must be Floor.");

            ik = GetComponent<VRIK>();
            initHeadPosition = ik.references.head.position.y;
            xrOriginCamera = xrOrigin.GetComponentInChildren<Camera>().transform;

            Debug.Log($"initHeadPosition: {initHeadPosition}");
        }

        private void Update() // 第一帧 Update 时，HeadTarget 高度值为：实际高度 + 1，之后则恢复正常（通常在第二帧 Update 时）
        {
            var headTargetPosition = ik.solver.spine.headTarget.position.y;
            if (Mathf.Abs(initHeadTargetPosition - headTargetPosition) > 0.01f) // 为了找到稳定高度，比实际情况延后了一帧
            {
                Debug.Log($"Update -> headTargetPosition({headTargetPosition})");
                return;
            }

            var rootPosition = ik.references.root.position;
            var scale = (initHeadPosition - rootPosition.y) / (headTargetPosition - rootPosition.y);

            switch (scaleMode)
            {
                case OriginScaleMode.Origin:
                    xrOrigin.transform.localScale *= scale;
                    ik.solver.scale = scale;

                    if (!scaleCameraView)
                    {
                        xrOriginCamera.localScale /= scale;
                    }
                    
                    Debug.Log($"Scale XR Origin: ({scale}, as headTargetPosition({headTargetPosition})))");
                    break;
                case OriginScaleMode.Character:
                    scale = 1 / scale;
                    ik.references.root.localScale *= scale;
                    ik.solver.scale = scale;
                    Debug.Log($"Scale Character: ({scale}, as headTargetPosition({headTargetPosition})))");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            enabled = false;
            Destroy(this);
        }

        private void LateUpdate()
        {
            initHeadTargetPosition = ik.solver.spine.headTarget.position.y;
        }
    }
}