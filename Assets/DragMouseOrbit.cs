using UnityEngine;

[AddComponentMenu("Camera-Control/Mouse drag Orbit with zoom")]
public class DragMouseOrbit : MonoBehaviour
{
    public Transform Target;
    public float Distance = 5.0f;
    public float XSpeed = 10.0f;
    public float YSpeed = 10.0f;
    public float YMinLimit = -20f;
    public float YMaxLimit = 80f;
    public float DistanceMin = 1f;
    public float DistanceMax = 15f;
    public float SmoothTime = 2f;
    float rotationYAxis;
    float rotationXAxis;
    float velocityX;
    float velocityY;

    void Start()
    {
        rotationXAxis = transform.eulerAngles.x;
        rotationYAxis = transform.eulerAngles.y;
    }

    void LateUpdate()
    {
        if (!Target) { return; }

        velocityX += XSpeed * Input.GetAxis("Mouse X") * 0.02f;
        velocityY += YSpeed * Input.GetAxis("Mouse Y") * 0.02f;
        rotationYAxis += velocityX;
        rotationXAxis -= velocityY;
        rotationXAxis = ClampAngle(rotationXAxis, YMinLimit, YMaxLimit);
        var rotation = Quaternion.Euler(rotationXAxis, rotationYAxis, 0);

        Distance = Mathf.Clamp(Distance - Input.GetAxis("Mouse ScrollWheel") * 5f, DistanceMin, DistanceMax);
        var negDistance = new Vector3(0.0f, 0.0f, -Distance);
        var position = rotation * negDistance + Target.position;

        transform.rotation = rotation;
        transform.position = position;
        velocityX = Mathf.Lerp(velocityX, 0, Time.deltaTime * SmoothTime);
        velocityY = Mathf.Lerp(velocityY, 0, Time.deltaTime * SmoothTime);
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
        {
            angle += 360F;
        }
        if (angle > 360F)
        {
            angle -= 360F;
        }

        return Mathf.Clamp(angle, min, max);
    }
}