using UnityEngine;
using System.Collections;

public class KeepAwake : MonoBehaviour
{
    Rigidbody body;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    void Update()
    {
    }

    void FixedUpdate()
    {
        if (body.IsSleeping())
        {
            body.WakeUp();
        }
    }
}
