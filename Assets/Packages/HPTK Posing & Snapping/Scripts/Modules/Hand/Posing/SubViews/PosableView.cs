using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.Input;
using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Modules.Hand.Snapping;
using HandPhysicsToolkit.Modules.Part.Puppet;
using HandPhysicsToolkit.Physics;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HandPhysicsToolkit.Modules.Hand.Posing
{
    [RequireComponent(typeof(Pheasy))]
    public class PosableView : HPTKView
    {
        public static List<PosableView> registry = new List<PosableView>();

        Pheasy _pheasy = null;
        public Pheasy pheasy
        {
            get
            {
                if (_pheasy == null) _pheasy = GetComponent<Pheasy>();
                return _pheasy;
            }
        }

        SnappableView _snappable = null;
        public SnappableView snappable
        {
            get
            {
                if (_snappable == null) _snappable = GetComponent<SnappableView>();
                return _snappable;
            }
        }

        [Header("Refs")]

        [SerializeField]
        Transform _transformRef = null;
        public Transform transformRef { get { return _transformRef; } }

        [SerializeField]
        Transform _transformRefL = null;
        public Transform transformRefL { get { return _transformRefL; } }

        [SerializeField]
        Transform _transformRefR = null;
        public Transform transformRefR { get { return _transformRefR; } }

        [Header("Poses")]

        [SerializeField]
        HandGesture _handGesture = HandGesture.PowerGrip;
        public HandGesture handGesture { get { return _handGesture; } }

        [SerializeField]
        HandPoseAsset _customEndPoseL = null;
        public HandPoseAsset customEndPoseL { get { return _customEndPoseL; } }

        [SerializeField]
        HandPoseAsset _customEndPoseR = null;
        public HandPoseAsset customEndPoseR { get { return _customEndPoseR; } }

        [SerializeField]
        bool _forceEndPose = false;
        public bool forceEndPose { get{ return _forceEndPose; } }

        [Header("Control")]

        [SerializeField]
        PosePointRotationMode _rotationMode = PosePointRotationMode.None;
        public PosePointRotationMode rotationMode { get { return _rotationMode; } }

        [SerializeField]
        PosePointPositionMode _positionMode = PosePointPositionMode.Default;
        public PosePointPositionMode positionMode { get { return _positionMode; } }

        [SerializeField]
        [Range(0.0f, 0.5f)]
        float _minDistance = 0.0f;
        public float minDistance { get { return _minDistance; } }

        [SerializeField]
        [Range(0.0f, 1.0f)]
        float _startAtLerp = 0.0f;
        public float startAtLerp { get { return _startAtLerp; } }

        [SerializeField]
        [Range(0.0f, 1.0f)]
        float _stopAtLerp = 1.0f;
        public float stopAtLerp { get { return _stopAtLerp; } }

        [SerializeField]
        [Range(0.0f, 0.01f)]
        float _boneThickness = 0.008f;
        public float boneThickness { get { return _boneThickness; } }

        [SerializeField]
        bool _collideWithOtherRbs = false;
        public bool collideWithOtherRbs { get { return _collideWithOtherRbs; } }

        [SerializeField]
        bool _collideWithTriggers = false;
        public bool collideWithTriggers { get { return _collideWithTriggers; } }

        public override sealed void Awake()
        {
            base.Awake();
        }

        private void OnEnable()
        {
            registry.Add(this);
        }

        private void OnDisable()
        {
            registry.Remove(this);
        }

        private void Update() { }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!_transformRefL) _transformRefL = transform;
            if (!_transformRefR) _transformRefR = transform;
            if (!_transformRef) _transformRef = transform;
        }
#endif
        public Transform GetTransformRef(Side s)
        {
            switch(s)
            {
                case Side.Left:
                    return transformRefL;
                case Side.Right:
                    return transformRefR;
                default:
                    return transformRef;
            }
        }
    }
}
