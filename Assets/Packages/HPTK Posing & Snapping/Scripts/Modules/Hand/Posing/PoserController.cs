using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.Input;
using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Modules.Part.Puppet;
using HandPhysicsToolkit.Utils;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HandPhysicsToolkit.Modules.Hand.Posing
{
    [RequireComponent(typeof(PoserModel))]
    public class PoserController : HPTKController
    {
        [ReadOnly]
        public PoserModel model;

        LayerMask layerMask;

        RaycastHit normalHit = new RaycastHit();
        RaycastHit tempHit = new RaycastHit();
        RaycastHit upHit = new RaycastHit();
        RaycastHit[] hits = new RaycastHit[0];

        public override void Awake()
        {
            base.Awake();
            model = GetComponent<PoserModel>();
            SetGeneric(model.view, model);
        }

        private void OnEnable()
        {
            model.hand.registry.Add(this);
        }

        private void OnDisable()
        {
            model.hand.registry.Remove(this);
        }


        public override void ControllerStart()
        {
            base.ControllerStart();

            // Get layer mask
            if (model.layerNames.Count == 0 || !model.useLayerMask)
                layerMask = ~0;
            else
                layerMask = LayerMask.GetMask(model.layerNames.ToArray());
        }

        public override void ControllerUpdate()
        {
            base.ControllerUpdate();

            if (model.posable) Pose(model.posable, AvatarModel.key);
        }

        void MovePalm(PosableView posable)
        {
            if (!posable) return;

            Transform ghostWrist = model.hand.wrist.reprs[PoserModel.key].transformRef;
            Transform ghostPalmNormal = model.hand.palmNormal.reprs[PoserModel.key].transformRef;
            Transform ghostPalmCenter = model.hand.palmCenter.reprs[PoserModel.key].transformRef;
            Transform masterPalmCenter = model.hand.palmCenter.reprs[AvatarModel.key].transformRef;

            Vector3 palmInterior = ghostPalmCenter.position + masterPalmCenter.up * 0.05f;

            Rigidbody desiredRb = posable.pheasy.rb;

            // Find the hit
            Ray fromPalmToPosable = new Ray(ghostPalmNormal.position, posable.GetTransformRef(model.hand.side).position - ghostPalmNormal.position);
            PhysHelpers.ClosestHitFromPoint(
                ref normalHit,
                hits,
                fromPalmToPosable,
                model.maxDistance,
                desiredRb,
                posable.collideWithOtherRbs,
                posable.collideWithTriggers,
                layerMask);

            Vector3 palmDestination;
            switch (posable.positionMode)
            {
                case PosePointPositionMode.Default:
                    
                    if (normalHit.rigidbody != null)
                    {
                        palmDestination = normalHit.point;
                    }
                    else
                    {
                        Debug.LogWarning("Poser didn't hit any rigidbody while posing " + posable.name + ". Position mode Default cannot be used");
                        palmDestination = posable.GetTransformRef(model.hand.side).position;
                    }

                    break;

                case PosePointPositionMode.ClosestPoint:

                    if (normalHit.collider != null)
                    {
                        // Get closest point to that collider
                        palmDestination = normalHit.collider.ClosestPoint(ghostPalmNormal.position);

                        // Raycast to that point and store hit (including normal)
                        if (palmDestination != ghostPalmNormal.position)
                        {
                            fromPalmToPosable = new Ray(ghostPalmNormal.position, (palmDestination - ghostPalmNormal.position).normalized);
                            normalHit.collider.Raycast(fromPalmToPosable, out normalHit, model.maxDistance);
                        }
                        else
                        {
                            Debug.LogWarning("Ghost hand was already in destination position");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Poser didn't hit any collider while posing " + posable.name + ". Position mode ClosestPoint cannot be used");
                        palmDestination = posable.GetTransformRef(model.hand.side).position;
                    }

                    break;

                case PosePointPositionMode.AlwaysMatch:
                default:

                    palmDestination = posable.GetTransformRef(model.hand.side).position;

                    break;
            }

            // Rotate hand so its normal will be the opposite to hit normal
            Vector3 forwardDir = normalHit.normal * -1.0f;

            Quaternion desiredRot;
            Quaternion relRot;

            switch (posable.rotationMode)
            {
                case PosePointRotationMode.MatchXY:

                    desiredRot = posable.GetTransformRef(model.hand.side).rotation;
                    relRot = Quaternion.Inverse(ghostWrist.rotation) * ghostPalmCenter.rotation;
                    ghostWrist.rotation = desiredRot * Quaternion.Inverse(relRot);

                    break;

                case PosePointRotationMode.MatchNormal:

                    desiredRot = Quaternion.LookRotation(posable.GetTransformRef(model.hand.side).forward, ghostPalmCenter.up);
                    relRot = Quaternion.Inverse(ghostWrist.rotation) * ghostPalmNormal.rotation;
                    ghostWrist.rotation = desiredRot * Quaternion.Inverse(relRot);

                    break;

                default:

                    Vector3 rayDir = fromPalmToPosable.direction;
                    PhysHelpers.ClosestHitFromLine(
                        ref upHit,
                        ref tempHit,
                        hits,
                        rayDir,
                        model.maxDistance,
                        ghostPalmNormal.position,
                        palmInterior,
                        4,
                        desiredRb,
                        posable.collideWithOtherRbs,
                        posable.collideWithTriggers,
                        layerMask);

                    Vector3 interiorDestination;
                    if (upHit.rigidbody != null)
                        interiorDestination = upHit.point;
                    else if (normalHit.collider != null)
                        interiorDestination = normalHit.collider.ClosestPoint(palmInterior);
                    else
                        interiorDestination = posable.GetTransformRef(model.hand.side).up;

                    Vector3 upDir = (interiorDestination - palmDestination).normalized;

                    if (forwardDir != Vector3.zero)
                    {
                        desiredRot = Quaternion.LookRotation(forwardDir, upDir);
                        relRot = Quaternion.Inverse(ghostWrist.rotation) * ghostPalmNormal.rotation;
                        ghostWrist.rotation = desiredRot * Quaternion.Inverse(relRot);
                    }

                    break;
            }

            // Move hand to match hit point
            Vector3 posDiff = palmDestination - ghostPalmNormal.position;

            Debug.DrawLine(palmDestination, ghostPalmNormal.position, Color.yellow);

            ghostWrist.position += posDiff;

            posDiff = ghostPalmNormal.forward * posable.minDistance;

            ghostWrist.position -= posDiff;
        }

        FingerPose GenerateFingerPose(FingerModel finger, FingerPose startFingerPose, FingerPose endFingerPose, int initialIteration, int lastIteration, PosableView posable)
        {
            List<BoneModel> bones = finger.bonesFromRootToTip;

            float[] lerpMap = new float[bones.Count];

            for (int m = 0; m < lerpMap.Length; m++)
            {
                lerpMap[m] = 1.0f;
            }

            float lerp;
            Vector3 boneBase;
            Vector3 boneTip;
            Vector3 boneVector;

            bool updateMap;
            for (int i = initialIteration; i <= lastIteration; i++)
            {
                lerp = (float)i / (float)model.maxIterations;

                for (int b = 0; b < bones.Count; b++)
                {
                    if (lerp > lerpMap[b])
                        continue;

                    Quaternion startLocalRot, endLocalRot;

                    // If thumb0 or pinky0 are missing
                    if (bones.Count < 4 && startFingerPose.bones.Count >= 4)
                    {
                        if (b == 0) startLocalRot = startFingerPose.bones[0].rotation * startFingerPose.bones[1].rotation;
                        else startLocalRot = startFingerPose.bones[b + 1].rotation;
                    }
                    else
                    {
                        startLocalRot = startFingerPose.bones[b].rotation;
                    }

                    // If thumb0 or pinky0 are missing
                    if (bones.Count < 4 && endFingerPose.bones.Count >= 4)
                    {
                        if (b == 0) endLocalRot = endFingerPose.bones[0].rotation * endFingerPose.bones[1].rotation;
                        else endLocalRot = endFingerPose.bones[b + 1].rotation;
                    }
                    else
                    {
                        endLocalRot = endFingerPose.bones[b].rotation;
                    }

                    // Rotate
                    Quaternion localRot = Quaternion.Lerp(startLocalRot, endLocalRot, lerp);
                    bones[b].reprs[PoserModel.key].transformRef.localRotation = localRot;

                    if (!model.onlyFingerTips || (model.onlyFingerTips && b == bones.Count - 1))
                    {
                        // Search for collisions
                        boneBase = bones[b].reprs[PoserModel.key].transformRef.position;

                        if (b == bones.Count - 1)
                        {
                            boneTip = finger.tip.reprs[PoserModel.key].transformRef.position;
                        }
                        else
                        {
                            boneTip = bones[b + 1].reprs[PoserModel.key].transformRef.position;
                        }

                        boneVector = boneTip - boneBase;

                        Ray ray = new Ray(boneBase, boneVector.normalized);

                        RaycastHit[] hits;
                        if (posable.boneThickness > 0)
                        {
                            hits = UnityEngine.Physics.SphereCastAll(ray, posable.boneThickness, boneVector.magnitude, layerMask);
                        }
                        else
                        {
                            hits = UnityEngine.Physics.RaycastAll(ray, boneVector.magnitude, layerMask);
                        }

                        updateMap = false;
                        for (int h = 0; h < hits.Length; h++)
                        {
                            // If we didn't touch anything
                            if (hits[h].rigidbody == null)
                                continue;
                            // If we touch a trigger and we can ignore triggers
                            if (hits[h].collider.isTrigger && !posable.collideWithTriggers)
                                continue;

                            // If we touch the desired rigidbody
                            if (hits[h].rigidbody == posable.pheasy.rb)
                            {
                                updateMap = true;
                            }
                            // If we touched something and we can collide with any rigidbody (except ourselves)
                            else if (posable.pheasy.rb == null && hits[h].rigidbody && !hits[h].rigidbody.transform.IsChildOf(finger.hand.wrist.reprs[PoserModel.key].transformRef))
                            {
                                updateMap = true;
                            }
                        }

                        // If there are, update lerp map
                        if (updateMap)
                        {
                            for (int m = 0; m <= b; m++)
                            {
                                lerpMap[m] = lerp;
                            }
                        }
                    }
                }

                // If any bone has a lerp lower than its limit then we can keep iterating
                bool exit = true;
                for (int b = 0; b < bones.Count; b++)
                {
                    if (lerp < lerpMap[b])
                        exit = false;
                }

                if (exit)
                    break;
            }

            return new FingerPose(finger, PoserModel.key);
        }

        HandPoseAsset FindEndPose(PosableView posable)
        {
            HandPoseAsset endPose;

            switch (posable.handGesture)
            {
                case HandGesture.Custom:
                    switch (model.hand.side)
                    {
                        case Side.Left:
                            endPose = posable.customEndPoseL;
                            if (!endPose) endPose = model.customEndPose;
                            break;
                        case Side.Right:
                        default:
                            endPose = posable.customEndPoseR;
                            if (!endPose) endPose = model.customEndPose;
                            break;
                    }
                    break;
                case HandGesture.IndexPinch:
                    endPose = model.indexPinchPose;
                    break;
                case HandGesture.FullPinch:
                    endPose = model.fullPinchPose;
                    break;
                case HandGesture.PrecisionGrip:
                    endPose = model.precisionGripPose;
                    break;
                case HandGesture.PowerGrip:
                default:
                    endPose = model.powerGripPose;
                    break;
            }

            return endPose;
        }

        void PoseEmpty()
        {
            switch (model.whenPosePointIsNull)
            {
                case DefaultPosingBehaviour.MatchMaster:
                    PoseHelpers.ApplyHandPose(model.hand, AvatarModel.key, model.hand, PoserModel.key, false, true, true, false);
                    break;
                case DefaultPosingBehaviour.SetOpenedHandPose:
                    ApplyHandPose(model.openedHandPose);
                    break;
                case DefaultPosingBehaviour.SetIndexPinchPose:
                    ApplyHandPose(model.indexPinchPose);
                    break;
                case DefaultPosingBehaviour.SetFullPinchPose:
                    ApplyHandPose(model.fullPinchPose);
                    break;
                case DefaultPosingBehaviour.SetPrecisionGripPose:
                    ApplyHandPose(model.precisionGripPose);
                    break;
                case DefaultPosingBehaviour.SetPowerGripPose:
                    ApplyHandPose(model.powerGripPose);
                    break;
                case DefaultPosingBehaviour.SetCustomPose:
                    ApplyHandPose(model.customEndPose);
                    break;
                case DefaultPosingBehaviour.KeepLastPose:
                    // Do nothing
                    break;
            }
        }

        void ApplyHandPose(HandPoseAsset pose)
        {
            PoseHelpers.ApplyHandPose(pose, model.hand, PoserModel.key, false, true, false, false);
        }

        public void Pose(PosableView posable, string sourceKey)
        {
            if (!model.hand.wrist.reprs.ContainsKey(sourceKey))
            {
                Debug.LogError("Wrist bone of hand " + model.hand.name + " does not have a representation with key " + sourceKey + ". The hand cannot be posed");
                return;
            }

            // Match wrist
            ReprModel originalWrist = model.hand.wrist.reprs[sourceKey];
            ReprModel ghostWrist = model.hand.wrist.reprs[PoserModel.key];

            ghostWrist.transformRef.position = originalWrist.transformRef.position;
            ghostWrist.transformRef.rotation = originalWrist.transformRef.rotation;
            ghostWrist.transformRef.localScale = originalWrist.transformRef.localScale;

            if (posable == null)
            {
                PoseEmpty();
                Debug.Log("Hey!");
                return;
            }

            // Move wrist if needed
            if (!model.ghostMatchesMastersWrist) MovePalm(posable);

            // Set iterations
            int lastIteration = (int)(model.maxIterations * posable.stopAtLerp);
            int initialIteration;
            if (posable.forceEndPose) initialIteration = lastIteration; // Process only last iteration if needed
            else initialIteration = (int)(model.maxIterations * posable.startAtLerp);

            // Find the end pose
            HandPoseAsset startPose = model.openedHandPose;
            HandPoseAsset endPose = FindEndPose(posable);

            // Pose each finger
            FingerPose startFingerPose, endFingerPose, generatedFingerPose;
            for (int f = 0; f < model.hand.fingers.Count; f++)
            {
                startFingerPose = startPose.fingers.Find(fng => fng.finger == model.hand.fingers[f].finger);
                endFingerPose = endPose.fingers.Find(fng => fng.finger == model.hand.fingers[f].finger);

                if (startFingerPose == null || endFingerPose == null) continue;

                generatedFingerPose = GenerateFingerPose(model.hand.fingers[f], startFingerPose, endFingerPose, initialIteration, lastIteration, posable);
            }
        }

        public void SetPosable(PosableView posable)
        {
            model.posable = posable;

            model.view.onSetPosable.Invoke(posable);
        }

        public void SetVisuals(bool enabled)
        {
            model.hand.body.avatar.controller.SetPartVisuals(model.hand, PoserModel.key, enabled);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!model) Awake();

            if (model.hand && model.drawLines)
            {
                Gizmos.color = Color.blue;
                if (model.posable && model.posable.pheasy)
                    Gizmos.DrawLine(model.hand.palmCenter.reprs[PoserModel.key].transformRef.position, model.posable.pheasy.rb.position);
            }
        }
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(PoserController)), CanEditMultipleObjects]
    public class PoserControllerEditor : HPTKControllerEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            PoserController myScript = (PoserController)target;

            if (GUILayout.Button("POSE"))
            {
                myScript.Pose(myScript.model.posable, AvatarModel.key);
            }
        }
    }
#endif

}