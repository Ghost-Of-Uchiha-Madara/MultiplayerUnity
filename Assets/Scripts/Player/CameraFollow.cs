using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    [Header("Third-Person Follow")]
    public float distance = 6f;
    public float height = 2.5f;
    public float followSmoothTime = 0.12f;
    public float rotationSmoothTime = 0.08f;

    [Header("Look")]
    public float lookHeight = 1.5f;

    [Header("Desktop Orbit (Optional)")]
    public bool allowMouseOrbit = true;
    public float mouseSensitivity = 120f;

    private Transform target;
    private Vector3 currentVelocity;
    private float yaw;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            yaw = target.eulerAngles.y;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        if (allowMouseOrbit && Mouse.current.rightButton.isPressed)
        {
            float mouseX = Mouse.current.delta.ReadValue().x;
            yaw += mouseX * mouseSensitivity * Time.deltaTime;
        }
        else
        {
            yaw = Mathf.LerpAngle(yaw, target.eulerAngles.y, rotationSmoothTime * 60f * Time.deltaTime);
        }
#else
        yaw = Mathf.LerpAngle(yaw, target.eulerAngles.y, rotationSmoothTime * 60f * Time.deltaTime);
#endif

        Vector3 rotation = new Vector3(0f, yaw, 0f);
        Vector3 desiredOffset = Quaternion.Euler(rotation) * new Vector3(0f, height, -distance);
        Vector3 desiredPosition = target.position + desiredOffset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref currentVelocity,
            followSmoothTime
        );

        transform.LookAt(target.position + Vector3.up * lookHeight);
    }
}