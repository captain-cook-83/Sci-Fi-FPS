using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Modules.Hand.GestureDetection;
using HandPhysicsToolkit.Modules.Hand.Posing;
using HandPhysicsToolkit.Modules.Part.ContactDetection;
using HandPhysicsToolkit.Modules.Part.Puppet;
using HandPhysicsToolkit.Physics;
using HandPhysicsToolkit.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandPhysicsToolkit.Modules.Hand.Snapping
{
    public enum SnapperAvailability
    {
        Always,
        WhenUntouchedByAny
    }

    public enum SnappingStep
    {
        None = 0,
        Attracting = 1,
        Snapped = 2
    }

    public enum SelectionGesture
    {
        Grasp,
        IndexPinch,
        Custom
    }

    public class SnapperModel : HPTKModel
    {
        public HandModel hand;

        [Header("Searching")]
        public Transform indicator;
        public Gesture searchGesture;
        public float minGestureLerpToSearch = 0.05f;
        public float maxCandidateDistance = 0.25f;
        public float timeSelectingToAttract = 0.2f;
        public string searchCandidatesCloseTo = "slave";
        public SnapperAvailability availability = SnapperAvailability.WhenUntouchedByAny;

        [Header("Attraction")]
        public float minDistanceToCompleteSnap = 0.005f;
        public float timeInMinDistanceToSnap = 0.1f;
        public Gesture customSelectionGesture;
        public CustomJointDrive attractionMotionDrive = new CustomJointDrive(1000000.0f, 2.0f, 20.0f);
        public CustomJointDrive attractionAngularDrive = new CustomJointDrive(1000000.0f, 2.0f, 20.0f);

        [Header("Snapping")]
        public bool refreshPoseLock = true;
        public bool drivesAreRelativeToSize = true;
        public CustomJointDrive snapMotionDrive = new CustomJointDrive(1000000.0f, 2.0f, 20.0f);
        public CustomJointDrive snapAngularDrive = new CustomJointDrive(1000000.0f, 2.0f, 20.0f);

        [Header("Debug")]
        public bool showIgnored = false;
        public bool stopGestureToUnsnap = true;

        [Header("Read Only")]
        [ReadOnly]
        public List<SnappableView> ignoringCollisionsWith = new List<SnappableView>();
        [ReadOnly]
        public GestureDetectionController gestureDetector;
        [ReadOnly]
        public ContactDetectionController contactDetector;
        [ReadOnly]
        public float timeSelecting = 0.0f;
        [ReadOnly]
        public float timeSnapped = 0.0f;
        [ReadOnly]
        public PoserController poser;
        [ReadOnly]
        public Gesture selectionGesture;
        [ReadOnly]
        public bool searching = false;

        [NonSerialized]
        public TargetConstraint constraint;

        [Header("Properties")]

        [SerializeField]
        SnappableView _candidate;
        public SnappableView candidate
        {
            get { return _candidate; }
            set
            {
                if (_candidate == value && ReferenceEquals(_candidate, value))
                    return;

                _candidate = value;

                if (indicator) indicator.gameObject.SetActive(value != null);

                if (poser) poser.SetVisuals(value != null && value.previewPose);

                if (_candidate != null)
                {
                    if (gestureDetector) selectionGesture = controller.FindSelectionGesture(_candidate);

                    view.onNewCandidate.Invoke(_candidate);
                }
            }
        }

        [SerializeField]
        SnappableView _snapped;
        public SnappableView snapped
        {
            get { return _snapped; }
            set
            {
                if (_snapped == value)
                    return;

                if (value != null)
                {
                    _snapped = value;
                    _snapped.AddSnapper(this);
                    view.onSnap.Invoke(_snapped);
                }
                else
                {
                    view.onUnsnap.Invoke(_snapped);
                    _snapped.RemoveSnapper(this);
                    _snapped = value;
                }
            }
        }

        [SerializeField]
        SnappingStep _step;
        public SnappingStep step
        {
            get { return _step; }
            set
            {
                if (_step == value)
                    return;

                _step = value;

                switch(_step)
                {
                    case SnappingStep.Snapped:
                        view.onSnap.Invoke(snapped);
                        snapped.onSnap.Invoke(view);
                        break;
                    case SnappingStep.Attracting:
                        view.onAttract.Invoke(snapped);
                        snapped.onAttract.Invoke(view);
                        break;
                    case SnappingStep.None:
                    default:
                        view.onUnsnap.Invoke(snapped);
                        snapped.onUnsnap.Invoke(view);
                        break;
                }
            }
        }

        SnapperController _controller;
        public SnapperController controller
        {
            get
            {
                if (!_controller)
                {
                    _controller = GetComponent<SnapperController>();
                    if (!_controller) _controller = gameObject.AddComponent<SnapperController>();
                }

                return _controller;
            }
        }

        SnapperView _view;
        public SnapperView view
        {
            get
            {
                if (!_view)
                {
                    _view = GetComponent<SnapperView>();
                    if (!_view) _view = gameObject.AddComponent<SnapperView>();
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
