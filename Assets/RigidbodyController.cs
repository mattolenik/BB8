using System;
using Assets;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class RigidbodyController : MonoBehaviour
{
    public float Speed = 10.0f;

    public bool Oomph = true;

    public bool CanJump = true;

    public float JumpForce = 2.0f;

    public float JumpDelay = 2.0f;

    public Vector3 Gravity = Vector3.down;

    public Transform Camera;

    public Rigidbody HeadRoot;

    public Transform Head;

    [Tooltip("Scale torque by this value")]
    public float TorqueScale = 0.3f;

    bool grounded;

    TimeSpan jumpDelay;
    DateTime lastJump = DateTime.MinValue;
    Rigidbody rbody;
    Vector3 up;

    void Awake()
    {
        rbody = GetComponent<Rigidbody>();
        up = -Gravity.normalized;
        jumpDelay = TimeSpan.FromSeconds(JumpDelay);
    }

    void LateUpdate()
    {
        HeadRoot.transform.position = transform.position;
    }

    void Update()
    {
        Camera.position = transform.position;
        var input = CrossPlatformInputManager.GetAxis("Mouse X") * 2f;
        Camera.rotation *= Quaternion.AngleAxis(input, Vector3.up);
    }

    void FixedUpdate()
    {
        HeadRoot.transform.position = transform.position;

        // When falling, allow the head to separate from the body to give the illusion of a separate part
        var y = rbody.velocity.y;
        if (y < 0)
        {
            var o = Vector3.ClampMagnitude(new Vector3(0f, Mathf.Pow(-y / 10f, 2f), 0f), 0.2f);
            var x = Vector3.MoveTowards(Head.transform.position, transform.position + o, Time.fixedDeltaTime);
            Head.transform.position = x;
        }
        else
        {
            // Resync position on upward motion to prevent any position drift
            Head.transform.position = transform.position;
        }

        HeadRoot.AddTorque(rbody.angularVelocity);
        HeadUp(HeadRoot, TorqueScale);

        var angle = Vector3.Dot(HeadRoot.transform.right, rbody.velocity.normalized) * (Mathf.Rad2Deg * Time.fixedDeltaTime * 8f);
        var q = Quaternion.AngleAxis(angle, Vector3.forward);
        HeadRoot.rotation *= q;
            
        if (grounded)
        {
            var right = Input.GetAxis("Horizontal");
            var forward = Input.GetAxis("Vertical");
            var jumping = Input.GetButton("Jump") && DateTime.Now - lastJump > jumpDelay;

            var moveForce = (Camera.forward * forward + Camera.right * right) * Speed;
            moveForce = Vector3.ClampMagnitude(moveForce, Speed);
            if (jumping)
            {
                moveForce += up * JumpForce;
                lastJump = DateTime.Now;
            }
            rbody.AddForce(moveForce, ForceMode.Force);

            if (Oomph)
            {
                var oomphForce = 1f / Mathf.Pow(rbody.velocity.magnitude, 2f);
                oomphForce = Mathf.Clamp(oomphForce, 0, Speed);
                rbody.AddForce(moveForce.normalized * oomphForce, ForceMode.Force);
            }
        }
        grounded = false;

        //Debug.DrawRay(HeadRoot.position, HeadRoot.transform.up, Color.blue);
        //Debug.DrawRay(HeadRoot.position, HeadRoot.transform.forward, Color.green);
        //Debug.DrawRay(HeadRoot.position, HeadRoot.transform.right, Color.red);
        //Debug.DrawRay(HeadRoot.position, rbody.velocity.normalized);
    }

    void HeadUp(Rigidbody body, float scale)
    {
        var target = Vector3.up;
        var current = body.transform.forward;
        // Axis of rotation
        var x = Vector3.Cross(current, target);
        var theta = Mathf.Asin(x.magnitude);
        // Change in angular velocity
        var w = x.normalized * (theta / Time.fixedDeltaTime * scale);
        // Current rotation in world space
        var q = body.transform.rotation * body.inertiaTensorRotation;
        // Transform to local space
        w = Quaternion.Inverse(q) * w;
        // Calculate torque and convert back to world space
        var T = q * Vector3.Scale(body.inertiaTensor, w);
        body.AddTorque(T, ForceMode.Force);

        //var p = body.position;
        //Debug.DrawRay(p, target, Color.white);
        //Debug.DrawRay(p, x, Color.cyan);
        //Debug.DrawRay(p, w, Color.yellow);
        //Debug.DrawRay(p, T, Color.magenta);
        //Debug.DrawRay(p, current, Color.red);
    }

    void OnCollisionStay()
    {
        grounded = true;
    }
}