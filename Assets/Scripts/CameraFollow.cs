// CameraFollow.cs
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 3f;
    [SerializeField] private float fallSpeedMultiplier = 1.8f;  
    [SerializeField] private float upwardOffset = 3f;
    [SerializeField] private float downwardOffset = 1f;
    [SerializeField] private float minY;

    private Vector3 velocity = Vector3.zero;

    private void LateUpdate()
    {
        if (target == null) return;

        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        float verticalOffset = targetRb != null && targetRb.velocity.y > 0 ? upwardOffset : downwardOffset;

        float currentSmoothSpeed = targetRb != null && targetRb.velocity.y < 0 ? smoothSpeed * fallSpeedMultiplier : smoothSpeed;

        float targetY = Mathf.Max(target.position.y + verticalOffset, minY);
        Vector3 targetPosition = new Vector3(transform.position.x, targetY, transform.position.z);

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 1f / currentSmoothSpeed);
    }
}
