using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Modules.Part.Puppet;
using HandPhysicsToolkit.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using HandPhysicsToolkit.Modules.Hand.Posing;
using HandPhysicsToolkit.Helpers;

namespace HandPhysicsToolkit.Utils
{
    [RequireComponent(typeof(HandSearchEngine))]
    public class GhostReprWizard : MonoBehaviour
    {
        public BodyModel body;

        public bool requiresCorrected = true;

        [ReadOnly]
        public HandSearchEngine source;

        [ReadOnly]
        public List<ReprModel> reprModels = new List<ReprModel>();

        public void GenerateMissingReprModels()
        {
            if (!source) source = GetComponent<HandSearchEngine>();

            if (!source.hand.IsValid())
            {
                Debug.LogError("Hand has missing references. ReprModels cannot be generated");
                return;
            }
            else if (requiresCorrected && !source.hand.wristPoint.corrected)
            {
                Debug.LogError("Hand wrist has not been corrected. Fix rotations first");
                return;
            }
            else
            {
                GenerateMissingReprModels(source);
            }
        }

        public void DestroyGenerated()
        {
            reprModels.ForEach(r => DestroyImmediate(r));
            reprModels.Clear();
        }

        List<Point> GetPoints(HandSearchEngine source)
        {
            List<Point> points; //ToList

            points = source.hand.ToList();
            points.AddRange(source.specialPoints.ToList());

            return points;
        }

        void GenerateMissingReprModels(HandSearchEngine source)
        {
            List<Point> points = GetPoints(source);
            points.ForEach(p => GenerateReprModel(p, source.bones.Find(b => b.parent.original == p.original) == null));
        }

        void GenerateReprModel(Point point, bool isLeaf)
        {
            if (point.original == null)
                return;

            // Avoid generating multiple ReprModels for the same point
            point.repr = point.tsf.GetComponent<PoserReprModel>();

            if (!(point.repr is PoserReprModel)) DestroyImmediate(point.repr);

            if (point.repr == null)
            {
                point.repr = point.tsf.gameObject.AddComponent<PoserReprModel>();

                point.repr.originalTsfRef = point.original;
            }

            if (!reprModels.Contains(point.repr)) reprModels.Add(point.repr);
        }

        public void LinkPointReprs()
        {
            List<Point> points = GetPoints(source);

            Point invalidPoint = points.Find(p => p.repr == null);

            if (invalidPoint != null)
            {
                Debug.LogError("There are points, like " + invalidPoint.tsf.name + ", that have no ReprModel. ReprModels may have not been generated.");
                return;
            }

            WizardHelpers.LinkHandPointReprs(source, body, true, requiresCorrected);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(GhostReprWizard))]
public class GhostReprWizardEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GhostReprWizard myScript = (GhostReprWizard)target;

        if (GUILayout.Button("GENERATE MISSING REPRMODELS"))
        {
            myScript.GenerateMissingReprModels();
        }

        if (GUILayout.Button("DESTROY GENERATED REPRMODELS"))
        {
            myScript.DestroyGenerated();
        }

        if (GUILayout.Button("LINK POINTS-REPRS"))
        {
            myScript.LinkPointReprs();
        }
    }
}
#endif

