using HandPhysicsToolkit.Input;
using HandPhysicsToolkit.Modules.Avatar;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Hand.Posing
{
    public enum PosePointPositionMode
    {
        Default,
        ClosestPoint,
        AlwaysMatch
    }

    public enum PosePointRotationMode
    {
        None,
        MatchXY,
        MatchNormal
    }

    public enum DefaultPosingBehaviour
    {
        SetOpenedHandPose,
        SetIndexPinchPose,
        SetFullPinchPose,
        SetPrecisionGripPose,
        SetPowerGripPose,
        SetCustomPose,
        MatchMaster,
        KeepLastPose
    }

    public class PoserModel : HPTKModel
    {
        public static string key = "ghost";

        public HandModel hand;

        public PosableView posable;

        [Header("Poses")]
        public HandPoseAsset openedHandPose;
        public HandPoseAsset indexPinchPose;
        public HandPoseAsset fullPinchPose;
        public HandPoseAsset precisionGripPose;
        public HandPoseAsset powerGripPose;
        public HandPoseAsset customEndPose; // If PosePoint use custom gesture but custom gesture is not referenced by PosePoint, then use custom gesture from posable hand

        [Header("Control")]
        public float maxDistance = 1.0f;
        public bool ghostMatchesMastersWrist = false;
        public DefaultPosingBehaviour whenPosePointIsNull = DefaultPosingBehaviour.SetOpenedHandPose;

        public bool useLayerMask = false;
        public List<string> layerNames = new List<string>(new string[]{"Default"});

        [Header("Performance")]
        [Range(10, 100)]
        public int maxIterations = 60;
        public bool onlyFingerTips = false;

        [Header("Debug")]
        public bool drawLines = false;

        PoserController _controller;
        public PoserController controller
        {
            get
            {
                if (!_controller)
                {
                    _controller = GetComponent<PoserController>();
                    if (!_controller) _controller = gameObject.AddComponent<PoserController>();
                }

                return _controller;
            }
        }

        PoserView _view;
        public PoserView view
        {
            get
            {
                if (!_view)
                {
                    _view = GetComponent<PoserView>();
                    if (!_view) _view = gameObject.AddComponent<PoserView>();
                }

                return _view;
            }
        }

        public override void Awake()
        {
            base.Awake();
        }
    }
}
