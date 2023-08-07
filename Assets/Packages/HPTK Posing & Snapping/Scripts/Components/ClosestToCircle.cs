using HandPhysicsToolkit;
using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Modules.Part.Puppet;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ClosestToCircle : MonoBehaviour
{
    public bool isActive = true;
    public bool showGizmos = true;

    public Side side;
    public string key = PuppetModel.key;

    [Header("Constraint")]
    public Transform center;
    public float radius = 0.1f;

    [Header("Avatar")]
    public float closerTo = 0.01f;

    bool started = false;

    AvatarView avatar;
    HandView hand;
    Vector3 centerToProjectionDir;

    void FindAvatar()
    {
        avatar = HPTK.core.avatars.Find(a => Vector3.Distance(a.body.replicatedTsf.position, HPTK.core.trackingSpace.position) < closerTo);
        if (avatar.started) Init(avatar);
        else avatar.onStarted.AddListener(() => Init(avatar));
    }

    void Init(AvatarView avatar)
    {
        if (hand != null)
            return;

        if (side == Side.Left)
        {
            hand = avatar.body.leftHand;
        }
        else
        {
            side = Side.Right;
            hand = avatar.body.rightHand;
        }

        started = true;
    }

    private void Update()
    {
        if (!hand) FindAvatar();

        if (isActive) UpdateClosestPoint(hand.palmCenter.reprs[key].transformRef.position, center.position, center.forward, radius);
    }

    void UpdateClosestPoint(Vector3 pnt, Vector3 center, Vector3 normal, float radius)
    {
        if (center == null|| hand == null || hand.palmCenter == null || !hand.palmCenter.reprs.ContainsKey(key))
            return;

        centerToProjectionDir = Vector3.ProjectOnPlane(pnt - center, normal).normalized;
        transform.position = center + centerToProjectionDir * radius;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (showGizmos && center && radius > 0.0f)
        {
            if (hand == null || hand.palmCenter == null || !hand.palmCenter.reprs.ContainsKey(key))
                return;

            Handles.color = Color.green;
            Handles.DrawWireDisc(center.position, center.forward, radius);

            if (hand != null && hand.palmCenter != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(transform.position, 0.01f);
            }

            if (isActive && !started)
            {
                UpdateClosestPoint(hand.palmCenter.reprs[key].transformRef.position, center.position, center.forward, radius);
            }
        }
    }
#endif
}
