using UnityEngine;

// Bobbing vertical genérico em senoide. Mexe na localPosition do transform.
public class IdleBob : MonoBehaviour
{
    public float amplitude = 0.08f;
    public float speed = 2.5f;
    public float phase = 0f;

    private Vector3 baseLocal;

    void Awake()
    {
        baseLocal = transform.localPosition;
    }

    void Update()
    {
        float y = Mathf.Sin(Time.time * speed + phase) * amplitude;
        transform.localPosition = new Vector3(baseLocal.x, baseLocal.y + y, baseLocal.z);
    }
}
