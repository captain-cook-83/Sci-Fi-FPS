using HandPhysicsToolkit;
using HandPhysicsToolkit.Modules.Avatar;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandPhysicsToolkit.Modules.Part.ContactDetection;
using System.Linq;
using HandPhysicsToolkit.Physics;
using HandPhysicsToolkit.Modules.Part.Puppet;
using HandPhysicsToolkit.Modules.Hand.Posing;
using HandPhysicsToolkit.Modules.Hand.GestureDetection;
using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.Utils;

namespace HandPhysicsToolkit.Modules.Hand.Snapping
{
    [RequireComponent(typeof(SnapperModel))]
    public class SnapperController : HPTKController
    {
        [ReadOnly]
        public SnapperModel model;

        CustomJointDrive originalMotionDrive;
        CustomJointDrive originalAngularDrive;

        List<SnappableView> _snappables = new List<SnappableView>();
        List<SnappableView> _candidates = new List<SnappableView>();

        ContactableView contactable;
        List<ContactView> _touched = new List<ContactView>();

        List<Transform> _candidateTsfs = new List<Transform>();

        List<BoneModel> _bones = new List<BoneModel>();

        public override void Awake()
        {
            base.Awake();
            model = GetComponent<SnapperModel>();
            SetGeneric(model.view, model);
        }

        private void OnEnable()
        {
            model.hand.registry.Add(this);
        }

        private void OnDisable()
        {
            if (model.snapped != null) Unsnap();
            model.hand.registry.Remove(this);
        }


        public override void ControllerStart()
        {
            base.ControllerStart();

            // Set candidate to null
            model.candidate = null;

            // Find modules
            if (!model.poser) model.poser = FindPoser();

            if (!model.gestureDetector) model.gestureDetector = FindGestureDetector();

            if (!model.searchGesture && model.gestureDetector) model.searchGesture = model.gestureDetector.model.grasp;

            if (!model.contactDetector) model.contactDetector = BasicHelpers.FindFirst<HPTKController, ContactDetectionController>(model.hand.registry);
            if (!model.contactDetector) model.hand.registry.onRegistry.AddListener(v => { if (v is ContactDetectionView) model.contactDetector = FindContactDetector(); });

            // Hide ghosts
            if (!model.candidate && model.hand.wrist.reprs.ContainsKey(PoserModel.key) && model.hand.wrist.reprs[PoserModel.key].skinnedMeshRenderer)
            {
                model.hand.wrist.reprs[PoserModel.key].skinnedMeshRenderer.enabled = false;
            }
        }

        public override void ControllerUpdate()
        {
            base.ControllerUpdate();

            if (!gameObject.activeSelf)
                return;

            if (!model.searchGesture)
            {
                Debug.LogWarning("SnapperModel " + model.name + " has not gesture referenced as search gesture. Snapper may not work");
            }

            // If snapped object was destroyed
            if (model.step != SnappingStep.None && model.snapped == null) Unsnap();

            // If candidate object was destroyed
            if (model.candidate == null && !ReferenceEquals(model.candidate, null)) model.candidate = null;

            // If nothing is snapped nor in snapping process
            if (!model.snapped)
            {
                model.searching = IsAvailableForSearching() && model.searchGesture.lerp > model.minGestureLerpToSearch;

                if (model.searching)
                {
                    // Get palmCenterPos and palmNormalDir
                    ReprModel palmCenter, palmNormal;

                    palmCenter = model.hand.palmCenter.reprs[model.searchCandidatesCloseTo];
                    palmNormal = model.hand.palmNormal.reprs[model.searchCandidatesCloseTo];

                    if (!palmCenter || !palmCenter.transformRef)
                    {
                        Debug.LogError("Palm center does not have a " + model.searchCandidatesCloseTo.ToString() + " representation with transformRef reference. Search for snappables cannot continue");
                        return;
                    }

                    if (!palmNormal || !palmNormal.transformRef)
                    {
                        Debug.LogError("Palm normal does not have a " + model.searchCandidatesCloseTo.ToString() + " representation with transformRef reference. Search for snappables cannot continue");
                        return;
                    }

                    SnappableView candidate = FindCandidate(model.maxCandidateDistance, palmCenter.transformRef.position, palmNormal.transformRef.forward);

                    if (model.candidate != candidate || candidate == null) model.timeSelecting = 0.0f;

                    // Update candidate even if it's null
                    model.candidate = candidate;

                    if (candidate)
                    {
                        // Move indicator to candidate
                        model.indicator.position = candidate.GetTransformRef(model.hand.side).position;

                        // Pose preview
                        if (model.poser && candidate.posable && candidate.previewPose)
                        {
                            model.poser.Pose(candidate.posable, AvatarModel.key);
                        }

                        // Selection detection
                        if (model.selectionGesture.lerp > candidate.minGestureLerpToSelect) model.timeSelecting += Time.deltaTime;

                        if (model.timeSelecting > model.timeSelectingToAttract)
                        {
                            model.searching = false;
                            model.timeSelecting = 0.0f;

                            Attract(candidate);
                        }
                    }
                }
                else
                {
                    if (model.candidate) model.candidate = null;
                }

                if (model.timeSnapped > 0.0f) model.timeSnapped = 0.0f;
            }
            // Check that attraction was completed
            else if (model.snapped && model.step == SnappingStep.Attracting)
            {
                // We can collide with the snapped object again if it's very close to destination position
                // Move indicator to candidate
                model.indicator.position = model.snapped.transformRef.position;

                // Update drives according to scale
                if (model.drivesAreRelativeToSize)
                {
                    ScaleDrives(model.constraint.settings, model.hand.totalScale);
                }

                // Count time under postion theresold
                if (Vector3.Distance(model.constraint.anchor.position, model.constraint.connectedAnchor.position) < model.minDistanceToCompleteSnap)
                {
                    model.timeSnapped += Time.deltaTime;
                }
                else
                {
                    model.timeSnapped = 0.0f;
                }

                // Snap if it's under threshold for a certain time
                if (model.timeSnapped > model.timeInMinDistanceToSnap) Snap(model.snapped);

                // Detect unsnap
                if (model.stopGestureToUnsnap && model.selectionGesture.lerp < model.snapped.minGestureLerpToSelect) Unsnap();
            }
            else if (model.snapped && model.step == SnappingStep.Snapped)
            {
                // Update drives according to scale
                if (model.drivesAreRelativeToSize)
                {
                    ScaleDrives(model.constraint.settings, model.hand.totalScale);
                }

                // Update pose if required (partial)
                if (model.refreshPoseLock && model.poser && model.snapped.posable)
                {
                    // From slave representation as it's already snapped
                    model.poser.Pose(model.snapped.posable, AvatarModel.key);
                    SetPuppetHandLimits(model.snapped);
                }

                // Detect unsnap
                if (model.stopGestureToUnsnap && model.selectionGesture.lerp < model.snapped.minGestureLerpToSelect) Unsnap();
            }

            // Gravity disabling is done every frame to avoid other snappers to restore gravity while that rigidbody is still snapped to this snapper
            if (model.snapped) // snapped or not
            {
                if (model.snapped.useGravity)
                {
                    model.snapped.pheasy.rb.useGravity = false;
                }
            }
        }

        ContactDetectionController FindContactDetector()
        {
            ContactDetectionController contactDetector = BasicHelpers.FindFirst<HPTKController, ContactDetectionController>(model.hand.registry);
            if (!contactDetector) Debug.LogWarning("Hand " + model.hand.name + " does not have a ContactDetection module registered");
            return contactDetector;
        }

        PoserController FindPoser()
        {
            PoserController poser = BasicHelpers.FindFirst<HPTKController, PoserController>(model.hand.registry);
            if (!poser) Debug.LogWarning("Hand " + model.hand.name + " does not have a Poser module registered");
            return poser;
        }

        GestureDetectionController FindGestureDetector()
        {
            GestureDetectionController gestureDetector = BasicHelpers.FindFirst<HPTKController, GestureDetectionController>(model.hand.registry);
            if (!gestureDetector) Debug.LogWarning("Hand " + model.hand.name + " does not have a GestureDetection module registered");
            return gestureDetector;
        }

        bool IsAvailableForSearching()
        {
            return (model.availability == SnapperAvailability.Always || !model.contactDetector || (model.contactDetector && model.contactDetector.model.contacts.Find(c => c.type >= ContactType.Touched) == null));
        }

        SnappableView FindCandidate(float maxDistance, Vector3 palmCenterPos, Vector3 palmNormalDir)
        {
            // Get all snappables in the Scene
            SnappableView.registry.ToList(_snappables);

            // Filter snappables
            _candidates.Clear();
            for (int s = 0; s < _snappables.Count; s++)
            {
                Transform snapTsf = _snappables[s].GetTransformRef(model.hand.side);

                // Filter snappables that are active in hierarchy
                if (!_snappables[s].gameObject.activeInHierarchy) continue;

                // Filter snappables which transformRef is closer thatn a given distance
                if (Vector3.Distance(palmCenterPos, snapTsf.position) > maxDistance) continue;

                // Filter snappables that can't be attached to this hand side
                if (_snappables[s].side == Side.None) continue;
                if (_snappables[s].side != Side.Both && _snappables[s].side != model.hand.side) continue;

                // Filter snappables which transformRef is opposite to palmNormal
                if (Vector3.Dot(palmNormalDir, (snapTsf.position - palmCenterPos).normalized) < 0.6f) continue;     

                // Filter unavailable candidates
                contactable = _snappables[s].GetComponent<ContactableView>();
                if ((!model.contactDetector || !contactable) && (_snappables[s].availability == SnapPointAvailability.IfUntouchedByAnySnapper || _snappables[s].availability == SnapPointAvailability.IfUntouchedByCurrentSnapper))
                {
                    Debug.LogWarning("Snappable " + _snappables[s].name + " does not have a ContactableView component attached or ContactDetection module was not found. Assuming that it's untouched");
                }
                else
                {
                    switch (_snappables[s].availability)
                    {
                        case SnapPointAvailability.Always:

                            // available

                            break;
                        case SnapPointAvailability.IfUntouchedByAnySnapper:

                            contactable.contacts.FindAll(c => c.type >= ContactType.Touched, _touched);
                            if (_touched.Count > 0) continue;

                            break;
                        case SnapPointAvailability.IfUntouchedByCurrentSnapper:

                            contactable.contacts.FindAll(c => c.detector == model.contactDetector && c.type >= ContactType.Touched, _touched);
                            if (_touched.Count > 0) continue;

                            break;
                        case SnapPointAvailability.None:

                            continue;
                    }
                }
                
                _candidates.Add(_snappables[s]);
            }

            // Get the closest transform from all transformRefs
            _candidates.ConvertAll(x => x.GetTransformRef(model.hand.side), _candidateTsfs);

            float d;
            float minDistance = -1.0f;
            SnappableView closestSnappable = null;

            for (int s = 0; s < _candidateTsfs.Count; s++)
            {
                d = Vector3.Distance(_candidateTsfs[s].position, palmCenterPos);
                if (minDistance < 0.0f || d < minDistance)
                {
                    minDistance = d;
                    closestSnappable = _candidates[s];
                }
            }

            return closestSnappable;
        }

        public void Unsnap()
        {
            if (model.step == SnappingStep.None && !model.snapped)
            {
                Debug.LogWarning("Nothing to unsnap");
                return;
            }

            if (model.constraint != null && model.constraint.pheasy != null) model.constraint.Remove();

            // Unlock pose
            SetPuppetHandLimits(false, false);

            if (model.snapped)
            {
                // Restore gravity
                model.snapped.pheasy.rb.useGravity = model.snapped.snappedBy.Count <= 1 && model.snapped.recoverGravity;

                // Resume collisions
                IgnoreCollisions(model.snapped, false, true, false);
            }

            // Invoke events from property before removing model.snapped
            model.step = SnappingStep.None;

            // Unsnap
            model.snapped = null; 
        }

        public void Attract(SnappableView snappable)
        {
            if (!snappable)
            {
                Debug.LogWarning("Nothing to snap");
                return;
            }

            if (snappable == model.snapped)
            {
                Debug.LogWarning("The object " + snappable.name + " is already snapped");
                return;
            }

            if (model.snapped) Unsnap();

            model.snapped = snappable;

            model.candidate = null;

            // If still snapped
            if (model.constraint != null) model.constraint.Remove();

            // Ignore collisions (forced)
            IgnoreCollisions(snappable, true, true, model.showIgnored);

            // Update pose locking (complete)
            if (model.poser && snappable.posable)
            {
                model.poser.Pose(snappable.posable, AvatarModel.key);
                SetPuppetHandLimits(true, true);
            }

            // Gravity
            snappable.pheasy.rb.useGravity = snappable.useGravity;

            // Prepare attraction

            Transform slavePalmCenter = null;
            if (model.hand.palmCenter.reprs.ContainsKey(PuppetModel.key))
            {
                slavePalmCenter = model.hand.palmCenter.reprs[PuppetModel.key].transformRef;
            }

            Transform ghostPalmCenter = null;
            if (model.hand.palmCenter.reprs.ContainsKey(PoserModel.key))
            {
                ghostPalmCenter = model.hand.palmCenter.reprs[PoserModel.key].transformRef;
            }

            PuppetReprModel slaveWrist = null;
            if (model.hand.wrist.reprs.ContainsKey(PuppetModel.key))
            {
                slaveWrist = model.hand.wrist.reprs[PuppetModel.key] as PuppetReprModel;
            }

            Transform start;
            if (snappable.canBeAttracted && snappable.posable) start = ghostPalmCenter;
            else start = snappable.transformRef;

            Transform end;
            if (snappable.canBeAttracted && slavePalmCenter) end = slavePalmCenter;
            else end = snappable.transformRef;

            // Perform attraction

            if (slaveWrist && slaveWrist.pheasy)
            {
                TargetConstraint constraint = snappable.pheasy.AddTargetConstraint("SnappedBy" + model.hand.name, slaveWrist.pheasy.rb, false, null);

                Vector3 relPos = start.InverseTransformPoint(snappable.transformRef.position);
                Quaternion relRot = Quaternion.Inverse(start.rotation) * snappable.transformRef.rotation;

                constraint.keepAxisRelativeToObject = true;

                constraint.anchor.position = snappable.transformRef.position;
                constraint.anchor.rotation = snappable.transformRef.rotation;
                constraint.connectedAnchor.position = end.TransformPoint(relPos);
                constraint.connectedAnchor.rotation = end.rotation * relRot;

                constraint.settings.collideWithConnectedRb = true;

                // Update drives
                SetAttractionDrives(snappable, constraint);

                // Update scale
                constraint.connectedAnchor.parent = slaveWrist.transformRef;

                model.constraint = constraint;
            }

            // Invoke events
            model.step = SnappingStep.Attracting;
        }

        public void Snap(SnappableView snappable)
        {
            if (snappable != model.snapped) Attract(snappable);

            // Update bone collisions (resume forced collision avoidance, repeat collision avoidance but unforced)
            IgnoreCollisions(snappable, true, false, model.showIgnored);

            // Update pose locking (partial)
            if (model.poser && snappable.posable) SetPuppetHandLimits(snappable);

            // Update drives
            SetSnapDrives(model.snapped, model.constraint);

            // Invoke events
            model.step = SnappingStep.Snapped;
        }

        void SetAttractionDrives(SnappableView snappable, TargetConstraint constraint)
        {
            CustomJointDrive motionDrive, angularDrive;

            if (snappable.useTheseAttractionDrives)
            {
                motionDrive = snappable.attractionMotionDrive;
                angularDrive = snappable.attractionAngularDrive;
            }
            else
            {
                motionDrive = model.attractionMotionDrive;
                angularDrive = model.attractionAngularDrive;
            }

            constraint.settings.motionDrive = motionDrive;
            constraint.settings.angularDrive = angularDrive;

            originalMotionDrive = new CustomJointDrive(motionDrive);
            originalAngularDrive = new CustomJointDrive(angularDrive);
        }

        void SetSnapDrives(SnappableView snappable, TargetConstraint constraint)
        {
            CustomJointDrive motionDrive, angularDrive;

            if (snappable.useTheseSnapDrives)
            {
                motionDrive = snappable.snapMotionDrive;
                angularDrive = snappable.snapAngularDrive;
            }
            else
            {
                motionDrive = model.snapMotionDrive;
                angularDrive = model.snapAngularDrive;
            }

            constraint.settings.motionDrive = motionDrive;
            constraint.settings.angularDrive = angularDrive;

            originalMotionDrive = new CustomJointDrive(motionDrive);
            originalAngularDrive = new CustomJointDrive(angularDrive);
        }

        void ScaleDrives(JointSettings settings, float scale)
        {
            settings.motionDrive.maxForce = originalMotionDrive.maxForce * scale;
            settings.angularDrive.maxForce = originalAngularDrive.maxForce * scale;
        }

        public Gesture FindSelectionGesture(SnappableView snappable)
        {
            if (!model.gestureDetector)
            {
                Debug.LogError("Missing gesture detector reference in " + model.name + ". Selection gesture for " + snappable.name + " cannot be provided");
                return null;
            }

            switch (snappable.selectionGesture)
            {
                case SelectionGesture.IndexPinch:
                    return model.gestureDetector.model.index.pinch;
                case SelectionGesture.Custom:
                    return model.customSelectionGesture;
                case SelectionGesture.Grasp:
                default:
                    return model.gestureDetector.model.grasp;
            }
        }

        void IgnoreCollisions(SnappableView snappable, bool ignore, bool force, bool showIgnored)
        {
            if (model.ignoringCollisionsWith.Contains(snappable) && !ignore) model.ignoringCollisionsWith.Remove(snappable);
            else if (!model.ignoringCollisionsWith.Contains(snappable) && ignore) model.ignoringCollisionsWith.Add(snappable);

            if (model.hand.wrist && model.hand.wrist.reprs.ContainsKey(PuppetModel.key))
            {
                PuppetReprModel wristPuppet = model.hand.wrist.reprs[PuppetModel.key] as PuppetReprModel;
                wristPuppet.pheasy.IgnoreCollisions(snappable.pheasy, (ignore && (force || !snappable.collidesWithWrist)), true);
            }
            else
            {
                Debug.LogWarning("Hand " + model.hand.name + " does not have a valid " + PuppetModel.key + " representation for wrist bone");
            }

            IgnoreCollisions(model.hand.thumb, snappable.thumbConf, snappable, ignore, force, showIgnored);
            IgnoreCollisions(model.hand.index, snappable.indexConf, snappable, ignore, force, showIgnored);
            IgnoreCollisions(model.hand.middle, snappable.middleConf, snappable, ignore, force, showIgnored);
            IgnoreCollisions(model.hand.ring, snappable.ringConf, snappable, ignore, force, showIgnored);
            IgnoreCollisions(model.hand.pinky, snappable.pinkyConf, snappable, ignore, force, showIgnored);
        }

        void IgnoreCollisions(FingerModel finger, FingerSpecificConfiguration fingerConf, SnappableView snappable, bool ignore, bool force, bool showIgnored)
        {
            if (!finger || !snappable)
            {
                Debug.LogError("It's not possible to ignore collisions between NULL finger or NULL snappable");
                return;
            }

            finger.bonesFromRootToTip.ToList(_bones);
            _bones.Reverse();

            PuppetReprModel slave;
            bool ignoreForBone;
            for (int b = 0; b < _bones.Count; b++)
            {
                if (!_bones[b].reprs.ContainsKey(PuppetModel.key))
                    continue;

                slave = _bones[b].reprs[PuppetModel.key] as PuppetReprModel;

                ignoreForBone = ignore && (b < fingerConf.ignoreCollisionsUntilDepth || force);

                slave.pheasy.IgnoreCollisions(snappable.pheasy, ignoreForBone, true);

                slave.constraint.showAnchor = ignoreForBone && showIgnored;
            }
        }

        public void SetPuppetHandLimits(bool limitOpening, bool limitClosing)
        {
            for (int f = 0; f < model.hand.fingers.Count; f++)
            {
                for (int b = 0; b < model.hand.fingers[f].bones.Count; b++)
                {
                    BoneModel bone = model.hand.fingers[f].bones[b];

                    if (!bone.reprs.ContainsKey(PoserModel.key))
                    {
                        Debug.LogWarning("Bone " + bone.name + " does not have a " + PoserModel.key + " representation. It won't be locked/unlocked");
                        continue;
                    }

                    if (!bone.reprs.ContainsKey(PuppetModel.key))
                    {
                        Debug.LogWarning("Bone " + bone.name + " does not have a " + PuppetModel.key + " representation. It won't be locked/unlocked");
                        continue;
                    }

                    PoserReprModel poserBone = bone.reprs[PoserModel.key] as PoserReprModel;
                    PuppetReprModel puppetBone = bone.reprs[PuppetModel.key] as PuppetReprModel;

                    SetPuppetBoneLimits(poserBone, puppetBone, limitOpening, limitClosing);
                }
            }
        }

        void SetPuppetHandLimits(SnappableView snappable)
        {
            SetPuppetFingerLimits(model.hand.thumb, snappable.thumbConf);
            SetPuppetFingerLimits(model.hand.index, snappable.indexConf);
            SetPuppetFingerLimits(model.hand.middle, snappable.middleConf);
            SetPuppetFingerLimits(model.hand.ring, snappable.ringConf);
            SetPuppetFingerLimits(model.hand.pinky, snappable.pinkyConf);
        }

        void SetPuppetFingerLimits(FingerModel finger, FingerSpecificConfiguration fingerConf)
        {
            if (!finger || fingerConf == null)
            {
                Debug.LogError("It's not possible to set limits for puppet fingers for a NULL finger or with NULL configuration");
                return;
            }

            finger.bonesFromRootToTip.ToList(_bones);
            _bones.Reverse();

            bool limitOver, limitUnder;
            PoserReprModel poserBone;
            PuppetReprModel puppetBone;
            for (int b = 0; b < _bones.Count; b++)
            {
                if (!_bones[b].reprs.ContainsKey(PoserModel.key))
                {
                    Debug.LogWarning("Bone " + _bones[b].name + " does not have a " + PoserModel.key + " representation. Lock map cannot be applied for this bone");
                    continue;
                }

                if (!_bones[b].reprs.ContainsKey(PuppetModel.key))
                {
                    Debug.LogWarning("Bone " + _bones[b].name + " does not have a " + PuppetModel.key + " representation. Lock map cannot be applied for this bone");
                    continue;
                }

                poserBone = _bones[b].reprs[PoserModel.key] as PoserReprModel;
                puppetBone = _bones[b].reprs[PuppetModel.key] as PuppetReprModel;

                limitOver = b >= fingerConf.limitOpeningDeeperThan && fingerConf.limitOpeningDeeperThan >= 0;
                limitUnder = b >= fingerConf.limitClosingDeeperThan && fingerConf.limitClosingDeeperThan >= 0;

                SetPuppetBoneLimits(poserBone, puppetBone, limitOver, limitUnder);
            }
        }

        void SetPuppetBoneLimits(PoserReprModel poserBone, PuppetReprModel puppetBone, bool limitOver, bool limitUnder)
        {
            if (!limitOver && !limitUnder) puppetBone.fixedLocalRot = Quaternion.identity;
            else puppetBone.fixedLocalRot = poserBone.transformRef.localRotation;

            if (limitOver) puppetBone.maxLocalRotZ = poserBone.localRotZ;
            else puppetBone.maxLocalRotZ = PuppetModel.maxLocalRotZ;

            if (limitUnder) puppetBone.minLocalRotZ = poserBone.localRotZ;
            else puppetBone.minLocalRotZ = PuppetModel.minLocalRotZ;
        }
    }
}