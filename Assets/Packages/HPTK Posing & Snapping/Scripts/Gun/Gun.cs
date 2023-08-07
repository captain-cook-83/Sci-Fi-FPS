using HandPhysicsToolkit.Helpers;
using HandPhysicsToolkit.Physics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Gun : MonoBehaviour
{
    public Pheasy slide;
    public AngularConstraint trigger;
    public int constraintIndex = 0;
    public float recoilVelocity = 1.0f;
    public float recoilDuration = 0.1f;

    public UnityEvent onShoot = new UnityEvent();

    float recoildRemainingTime = 0.0f;

    private void FixedUpdate()
    {
        if (recoildRemainingTime > 0.0f)
        {
            slide.rb.AddForce(slide.targets[constraintIndex].axis.forward * recoilVelocity, ForceMode.VelocityChange);

            recoildRemainingTime -= Time.fixedDeltaTime;
        }
    }

    public void Shoot()
    {
        recoildRemainingTime = recoilDuration;

        onShoot.Invoke();
    }
}
