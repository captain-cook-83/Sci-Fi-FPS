using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Modules.Hand.GestureDetection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HandPhysicsToolkit.Modules.Hand.Snapping
{
    [Serializable]
    public class SnappableEvent : UnityEvent<SnappableView> { }

    [RequireComponent(typeof(SnapperModel))]
    public sealed class SnapperView : HPTKView
    {
        SnapperModel model;

        public HandView hand { get { return model.hand.specificView; } }

        public Gesture searchGesture { get { return model.searchGesture; } }
        public Gesture selectionGesture { get { return model.selectionGesture; } }

        public bool stopGestureToUnsnap { get { return model.stopGestureToUnsnap; } set { model.stopGestureToUnsnap = value; } }

        public SnappableView snapped { get { return model.snapped; } }

        public SnappingStep step { get { return model.step; } }

        // Events
        [Header("Selection")]
        public SnappableEvent onNewCandidate = new SnappableEvent();

        [Header("Activation")]
        public SnappableEvent onAttract = new SnappableEvent();
        public SnappableEvent onSnap = new SnappableEvent();
        public SnappableEvent onUnsnap = new SnappableEvent();

        public override sealed void Awake()
        {
            base.Awake();
            model = GetComponent<SnapperModel>();
        }

        public void ForceUnsnap()
        {
            if (model.step == SnappingStep.Snapped) model.controller.Unsnap();
        }
        
        public void ForceSnap(SnappableView snappable)
        {
            if (model.step == SnappingStep.None) model.controller.Snap(snappable);
        }
    }
}