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
    private Transform lastTarget;

    void LateUpdate()
    {
        // Fonte de verdade do alvo: o ativo do PlayerSwap. Polling aqui evita
        // qualquer race entre Awake/OnEnable e a inscrição do evento.
        if (PlayerSwap.Instance != null && PlayerSwap.Instance.ActivePlayer != null)
        {
            target = PlayerSwap.Instance.ActivePlayer.transform;
        }

        if (target == null) return;

        // Quando o alvo muda (Tab), snap pra visão dele sem pan suave.
        if (target != lastTarget)
        {
            Vector3 snap = new Vector3(target.position.x + offset.x, target.position.y + offset.y, transform.position.z);
            if (minX < maxX) snap.x = Mathf.Clamp(snap.x, minX, maxX);
            if (minY < maxY) snap.y = Mathf.Clamp(snap.y, minY, maxY);
            transform.position = snap;
            velocity = Vector3.zero;
            lastTarget = target;
            return;
        }

        Vector3 desired = new Vector3(target.position.x + offset.x, target.position.y + offset.y, transform.position.z);
        if (minX < maxX) desired.x = Mathf.Clamp(desired.x, minX, maxX);
        if (minY < maxY) desired.y = Mathf.Clamp(desired.y, minY, maxY);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
    }
}
