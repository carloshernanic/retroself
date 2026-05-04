using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public Vector2 offset = new Vector2(0f, 1f);
    public float smoothTime = 0.15f;

    [Header("Bounds (mundo). Deixe iguais pra desativar.)")]
    public float minX = 0f;
    public float maxX = 0f;
    public float minY = 0f;
    public float maxY = 0f;

    private Vector3 velocity;

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 desired = new Vector3(target.position.x + offset.x, target.position.y + offset.y, transform.position.z);
        if (minX < maxX) desired.x = Mathf.Clamp(desired.x, minX, maxX);
        if (minY < maxY) desired.y = Mathf.Clamp(desired.y, minY, maxY);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
    }
}
