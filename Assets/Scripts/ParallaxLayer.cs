using UnityEngine;

// Oscilação horizontal lenta em senoide pra dar vida ao BG do MainMenu (câmera fica parada).
// Em cenas com câmera móvel, dá pra evoluir esse script pra ler camera.transform e aplicar factor.
public class ParallaxLayer : MonoBehaviour
{
    public float swayAmplitude = 0.3f;
    public float swaySpeed = 0.4f;
    public float phase = 0f;

    private Vector3 baseLocal;

    void Awake()
    {
        baseLocal = transform.localPosition;
    }

    void Update()
    {
        float x = Mathf.Sin(Time.time * swaySpeed + phase) * swayAmplitude;
        transform.localPosition = new Vector3(baseLocal.x + x, baseLocal.y, baseLocal.z);
    }
}
