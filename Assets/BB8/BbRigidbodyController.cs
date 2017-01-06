using System;
using Assets;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class BbRigidbodyController : MonoBehaviour
{
    public float Speed = 10.0f;

    public bool Oomph = true;

    public bool CanJump = true;

    public float JumpForce = 2.0f;

    public float JumpDelay = 2.0f;

    public Vector3 Gravity = Vector3.down;

    public Transform Camera;

    public Rigidbody Head;

    [Tooltip("Scale torque by this value")]
    public float TorqueScale = 0.3f;

    bool grounded;

    TimeSpan jumpDelay;
    DateTime lastJump = DateTime.MinValue;
    Rigidbody rbody;
    Vector3 up;
    float lateralSpeed;
    float forwardSpeed;
    bool jumping;

    void Awake()
    {
        rbody = GetComponent<Rigidbody>();
        up = -Gravity.normalized;
        jumpDelay = TimeSpan.FromSeconds(JumpDelay);
        Head.rotation = Quaternion.Euler(-90, 0, 132);
    }

    void Start()
    {
        Camera.LookAt(transform.position);
    }

    void LateUpdate()
    {
        Head.transform.position = transform.position;
    }

    void Update()
    {
        lateralSpeed = Input.GetAxis("Horizontal") * Speed;
        forwardSpeed = Input.GetAxis("Vertical") * Speed;
        jumping = Input.GetButton("Jump") && DateTime.Now - lastJump > jumpDelay;
    }

    void FixedUpdate()
    {
        Head.position = transform.position;
        Head.AddTorque(rbody.angularVelocity);
        HeadUp(Head, TorqueScale);

        // Point head in direction of movement
        var angle = Vector3.Dot(Head.transform.right, rbody.velocity.normalized) * (Mathf.Rad2Deg * Time.fixedDeltaTime * 12f);
        var q = Quaternion.AngleAxis(angle, Vector3.forward);
        Head.rotation *= q;
            
        if (grounded)
        {
            var d = rbody.position - Camera.position;
            var forward = new Vector3(d.x, 0, d.z).normalized;
            var right = Vector3.Cross(up, forward).normalized;
            var moveForce = forward * forwardSpeed + right * lateralSpeed;
            moveForce = Vector3.ClampMagnitude(moveForce, Speed);
            if (jumping)
            {
                moveForce += up * JumpForce;
                lastJump = DateTime.Now;
            }
            rbody.AddForce(moveForce);

            if (Oomph)
            {
                var oomphForce = 1f / Mathf.Pow(rbody.velocity.magnitude, 2f);
                oomphForce = Mathf.Clamp(oomphForce, 0, Speed);
                rbody.AddForce(moveForce.normalized * oomphForce);
            }
        }
        grounded = false;
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
        var q = body.rotation * body.inertiaTensorRotation;
        // Transform to local space
        w = Quaternion.Inverse(q) * w;
        // Calculate torque and convert back to world space
        var T = q * Vector3.Scale(body.inertiaTensor, w);
        body.AddTorque(T, ForceMode.Force);
    }

    void OnCollisionStay()
    {
        grounded = true;
    }
}