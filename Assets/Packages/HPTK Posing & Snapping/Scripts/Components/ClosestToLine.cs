using HandPhysicsToolkit;
using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.Modules.Avatar;
using HandPhysicsToolkit.Modules.Part.Puppet;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosestToLine : MonoBehaviour
{
    public bool isActive = true;
    public bool showGizmos = true;

    public Side side;
    public string key = PuppetModel.key;

    [Header("Constraint")]
    public Transform a;
    public Transform b;

    [Header("Avatar")]
    public float closerTo = 0.01f;

    bool started = false;

    AvatarView avatar;
    HandView hand;
    Vector3 closestPoint;

    void FindAvatar()
    {
        avatar = HPTK.core.avatars.Find(a => Vector3.Distance(a.body.replicatedTsf.position, HPTK.core.trackingSpace.position) < closerTo);

        if (!avatar) return;

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

        if (isActive) UpdateClosestPoint();
    }

    void UpdateClosestPoint()
    {
        if (!hand) return;

        closestPoint = GetClosestPointToInfiniteLine(hand.palmCenter.reprs[key].transformRef.position, a.position, b.position);

        if (Vector3.Distance(closestPoint, a.position) > Vector3.Distance(a.position, b.position))
            transform.position = b.position;
        else if (Vector3.Distance(closestPoint, b.position) > Vector3.Distance(a.position, b.position))
            transform.position = a.position;
        else
            transform.position = closestPoint;
    }

    Vector3 GetClosestPointToInfiniteLine(Vector3 pnt, Vector3 a, Vector3 b)
    {
        return a + Vector3.Project(pnt - a, b - a);
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (showGizmos && a && b)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(a.position, b.position);

            if (hand != null && hand.palmCenter != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(transform.position, 0.01f);
            }

            if (isActive && started) UpdateClosestPoint();
        }
    }
#endif
}
