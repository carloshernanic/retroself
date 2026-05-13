using System.Collections;
using UnityEngine;

// Cutaway narrativo: quando KeyPickup.Collected vira true, faz a câmera pan
// pra um focusTarget (a porta de fim), congela os players + PlayerSwap pelo
// tempo do pan e devolve o controle. Roda uma única vez por cena.
public class KeyCollectCameraPan : MonoBehaviour
{
    public CameraFollow2D cam;
    public Transform focusTarget;
    public PlayerController young;
    public PlayerController adult;
    public PlayerSwap playerSwap;

    [Tooltip("Tempo total que a câmera fica focada no alvo, em segundos.")]
    public float holdDuration = 2.2f;

    private bool fired;

    void Update()
    {
        if (fired) return;
        if (!KeyPickup.Collected) return;
        if (cam == null || focusTarget == null) return;
        fired = true;
        StartCoroutine(Cutaway());
    }

    IEnumerator Cutaway()
    {
        SetFrozen(true);
        cam.overrideTarget = focusTarget;
        yield return new WaitForSeconds(holdDuration);
        cam.overrideTarget = null;
        SetFrozen(false);
    }

    void SetFrozen(bool frozen)
    {
        if (young != null)
        {
            young.enabled = !frozen;
            var atk = young.GetComponent<PlayerAttack>();
            if (atk != null) atk.enabled = !frozen;
            var rb = young.GetComponent<Rigidbody2D>();
            if (rb != null && frozen) rb.linearVelocity = Vector2.zero;
        }
        if (adult != null)
        {
            adult.enabled = !frozen;
            var atk = adult.GetComponent<PlayerAttack>();
            if (atk != null) atk.enabled = !frozen;
            var rb = adult.GetComponent<Rigidbody2D>();
            if (rb != null && frozen) rb.linearVelocity = Vector2.zero;
        }
        if (playerSwap != null) playerSwap.enabled = !frozen;
        if (!frozen && playerSwap != null) playerSwap.RefreshActive();
    }
}
