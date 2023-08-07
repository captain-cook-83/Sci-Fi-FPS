using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Modules.Hand.Posing;
using HandPhysicsToolkit.Modules.Part.Puppet;
using HandPhysicsToolkit.Physics;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HandPhysicsToolkit.Modules.Hand.Snapping
{
    public enum SnapPointAvailability
    {
        None,
        IfUntouchedByAnySnapper,
        IfUntouchedByCurrentSnapper,
        Always
    }

    [Serializable]
    public class SnapperEvent : UnityEvent<SnapperView> { }

    [Serializable]
    public class FingerSpecificConfiguration
    {
        [Header("Lock pose")]

        [Tooltip("Pose defines how much it can be opened. Set 1 to unlock only distal bones. Set -1 to let the finger be opened without restrictions")]
        public int limitOpeningDeeperThan = 0;
        [Tooltip("Pose defines how much it can be closed. Set 1 to unlock only distal bones. Set -1 to let the finger be closed without restrictions")]
        public int limitClosingDeeperThan = 0;

        [Header("Ignore collisions")]

        [Tooltip("Set 0 to ignore only distal bone")]
        public int ignoreCollisionsUntilDepth = 0;
    }

    [RequireComponent(typeof(Pheasy))]
    public class SnappableView : HPTKView
    {
        public static List<SnappableView> registry = new List<SnappableView>();

        Pheasy _pheasy = null;
        public Pheasy pheasy
        {
            get
            {
                if (_pheasy == null) _pheasy = GetComponent<Pheasy>();
                return _pheasy;
            }
        }

        PosableView _posable = null;
        public PosableView posable
        {
            get
            {
                if (_posable == null) _posable = GetComponent<PosableView>();
                return _posable;
            }
        }

        List<SnapperModel> _snappedBy = new List<SnapperModel>();
        List<SnapperView> snappedBy_views = new List<SnapperView>();
        public List<SnapperView> snappedBy { get { _snappedBy.ConvertAll(s => s.view, snappedBy_views); return snappedBy_views; } }

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

        [Header("Selection")]

        [SerializeField]
        Side _side = Side.Both;
        public Side side { get { return _side; } }

        [SerializeField]
        SnapPointAvailability _availability = SnapPointAvailability.IfUntouchedByCurrentSnapper;
        public SnapPointAvailability availability { get { return _availability; } }

        [SerializeField]
        SelectionGesture _selectionGesture = SelectionGesture.Grasp;
        public SelectionGesture selectionGesture { get { return _selectionGesture; } }

        [SerializeField]
        float _minGestureLerpToSelect = 0.5f;
        public float minGestureLerpToSelect { get { return _minGestureLerpToSelect; } }

        [SerializeField]
        bool _previewPose = true;
        public bool previewPose { get { return _previewPose; } }

        [SerializeField]
        bool _canBeAttracted = true;
        public bool canBeAttracted { get { return _canBeAttracted; } }

        [Header("While Attracted")]

        [SerializeField]
        bool _useGravity = false;
        public bool useGravity { get { return _useGravity; } }

        [SerializeField]
        bool _useTheseAttractionDrives = false;
        public bool useTheseAttractionDrives { get { return _useTheseAttractionDrives; } }

        [SerializeField]
        CustomJointDrive _attractionMotionDrive = new CustomJointDrive(1000000.0f, 2.0f, 20.0f);
        public CustomJointDrive attractionMotionDrive { get { return _attractionMotionDrive; } }

        [SerializeField]
        CustomJointDrive _attractionAngularDrive = new CustomJointDrive(1000000.0f, 2.0f, 20.0f);
        public CustomJointDrive attractionAngularDrive { get { return _attractionAngularDrive; } }

        [Header("While Snapped")]

        [SerializeField]
        bool _useTheseSnapDrives = false;
        public bool useTheseSnapDrives { get { return _useTheseSnapDrives; } }

        [SerializeField]
        CustomJointDrive _snapMotionDrive = new CustomJointDrive(1000000.0f, 2.0f, 20.0f);
        public CustomJointDrive snapMotionDrive { get { return _snapMotionDrive; } }

        [SerializeField]
        CustomJointDrive _snapAngularDrive = new CustomJointDrive(1000000.0f, 2.0f, 20.0f);
        public CustomJointDrive snapAngularDrive { get { return _snapAngularDrive; } }

        [Header("On Release")]

        [SerializeField]
        bool _recoverGravity = true;
        public bool recoverGravity { get { return _recoverGravity; } }

        [Header("Ignore collisions")]

        [SerializeField]
        bool _collidesWithWrist = true;
        public bool collidesWithWrist { get { return _collidesWithWrist; } }

        [SerializeField]
        FingerSpecificConfiguration _thumbConf = null;
        public FingerSpecificConfiguration thumbConf { get { return _thumbConf; } }

        [SerializeField]
        FingerSpecificConfiguration _indexConf = null;
        public FingerSpecificConfiguration indexConf { get { return _indexConf; } }

        [SerializeField]
        FingerSpecificConfiguration _middleConf = null;
        public FingerSpecificConfiguration middleConf { get { return _middleConf; } }

        [SerializeField]
        FingerSpecificConfiguration _ringConf = null;
        public FingerSpecificConfiguration ringConf { get { return _ringConf; } }

        [SerializeField]
        FingerSpecificConfiguration _pinkyConf = null;
        public FingerSpecificConfiguration pinkyConf { get { return _pinkyConf; } }

        [Header("Events")]

        public SnapperEvent onAttract = new SnapperEvent();
        public SnapperEvent onSnap = new SnapperEvent();
        public SnapperEvent onUnsnap = new SnapperEvent();

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
        public void AddSnapper(SnapperModel snapper)
        {
            if (!_snappedBy.Contains(snapper))
                _snappedBy.Add(snapper);
        }

        public void RemoveSnapper(SnapperModel snapper)
        {
            if (_snappedBy.Contains(snapper))
                _snappedBy.Remove(snapper);
        }

        public Transform GetTransformRef(Side s)
        {
            switch (s)
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
