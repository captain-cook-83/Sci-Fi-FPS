using HandPhysicsToolkit.Modules.Avatar;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HandPhysicsToolkit.Modules.Hand.Posing
{
    [Serializable]
    public class PosableEvent : UnityEvent<PosableView> { }

    [RequireComponent(typeof(PoserModel))]
    public sealed class PoserView : HPTKView
    {
        PoserModel model;

        PoserController controller { get { return model.controller; } }

        public HandView hand { get { return model.hand.specificView; } }

        public PosableView posable
        {
            get { return model.posable; }
            set { if (model.posable != value) controller.SetPosable(value); }
        }

        // Events
        public PosableEvent onSetPosable = new PosableEvent();

        public override sealed void Awake()
        {
            base.Awake();
            model = GetComponent<PoserModel>();
        }
    }
}

    