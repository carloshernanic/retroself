using System.Collections.Generic;
using UnityEngine;

// Plataforma/parede móvel que abre quando todas as suas fontes (PressurePlate)
// estão ativas. Move o transform via Rigidbody2D.MovePosition pra que a colisão
// com o player seja suave (não joga ele pelo ar). Pode ser usada como porta
// vertical (openOffset.y > 0) ou como plataforma de impulso.
[RequireComponent(typeof(Rigidbody2D))]
public class GatedDoor : MonoBehaviour
{
    // Aceita qualquer GateSource (PressurePlate, StoneSwitch, etc.) — todos eles
    // expõem bool IsActive, e o gate só abre quando TODOS estão ativos (AND).
    public List<GateSource> sources = new List<GateSource>();

    // Quando true, uma vez aberta nunca fecha. Útil pra última placa do puzzle
    // 3 (adulto pisa, latched, depois jovem corre pra porta livre).
    public bool latched = false;

    // Offset relativo à posição inicial. Y positivo abre pra cima (porta vertical
    // sobe), X positivo abre pra direita. Plataforma de impulso usa Y positivo.
    public Vector2 openOffset = new Vector2(0f, 3f);

    public float moveSpeed = 4f;

    private Rigidbody2D rb;
    private Vector2 closedPos;
    private bool latchedOpen;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.gravityScale = 0f;
        closedPos = rb.position;
    }

    bool ShouldBeOpen()
    {
        if (latchedOpen) return true;
        if (sources == null || sources.Count == 0) return false;
        foreach (var s in sources)
        {
            if (s == null) continue;
            if (!s.IsActive) return false;
        }
        if (latched) latchedOpen = true;
        return true;
    }

    void FixedUpdate()
    {
        Vector2 target = ShouldBeOpen() ? closedPos + openOffset : closedPos;
        Vector2 next = Vector2.MoveTowards(rb.position, target, moveSpeed * Time.fixedDeltaTime);
        if (next != rb.position) rb.MovePosition(next);
    }

    // Destrava em runtime sem precisar de uma GateSource — útil pra portas que
    // começam fechadas e abrem por evento externo (ex: ReturnPad abrindo a
    // câmara da chave no Memory_02).
    public void ForceOpen()
    {
        latchedOpen = true;
    }
}
