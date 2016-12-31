using System;
using UnityEngine;

namespace Assets
{
    public static class Extensions
    {
        public static void MoveRotationTorque(this Rigidbody body, Quaternion targetRotation)
        {
            body.maxAngularVelocity = 1000;

            Quaternion r = targetRotation * Quaternion.Inverse(body.rotation);
            body.AddTorque(r.x / Time.fixedDeltaTime, r.y / Time.fixedDeltaTime, r.z / Time.fixedDeltaTime, ForceMode.VelocityChange);
            body.angularVelocity = Vector3.zero;
        }
    }
}
