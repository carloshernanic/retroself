using UnityEngine;

// Escala suave em senoide pra simular respiração. Aplica em ambos os eixos com pesos diferentes.
public class BreathingScale : MonoBehaviour
{
    public float amplitude = 0.02f;
    public float speed = 1.8f;
    public float phase = 0f;
    [Range(-1f, 1f)] public float xWeight = -0.4f; // negativo = squash horizontal quando estica vertical

    private Vector3 baseScale;

    void Awake()
    {
        baseScale = transform.localScale;
    }

    void Update()
    {
        float t = Mathf.Sin(Time.time * speed + phase) * amplitude;
        transform.localScale = new Vector3(
            baseScale.x * (1f + t * xWeight),
            baseScale.y * (1f + t),
            baseScale.z);
    }
}
