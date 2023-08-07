using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Physics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HandPhysicsToolkit.Modules.Hand.Posing
{
    public class PoserReprModel : ReprModel
    {
        public PoserReprView specificView { get { return view as PoserReprView; } }

        protected sealed override string FindKey()
        {
            return PoserModel.key;
        }

        protected sealed override ReprView GetView()
        {
            ReprView view = GetComponent<PoserReprView>();
            if (!view) view = gameObject.AddComponent<PoserReprView>();

            return view;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(PoserReprModel)), CanEditMultipleObjects]
    public class PoserReprModelEditor : ReprModelEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
#endif
}