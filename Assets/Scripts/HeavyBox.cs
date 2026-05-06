using UnityEngine;

// Caixa pesada que só o adulto consegue empurrar. Quando nenhum adulto está em contato,
// trava o eixo X via constraints — assim o jovem (ou qualquer outra coisa) não empurra
// pela colisão dinâmica padrão. Quando o adulto encosta, libera X e aplica força no
// sentido do MoveX dele.
[RequireComponent(typeof(Rigidbody2D))]
public class HeavyBox : MonoBehaviour
{
    public float pushForce = 18f;
    public float maxSpeed = 2.5f;

    private Rigidbody2D rb;
    private int adultContacts = 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 3.5f;
        if (rb.mass < 6f) rb.mass = 6f;
        if (rb.linearDamping < 4f) rb.linearDamping = 4f;
        ApplyConstraints();
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (IsAdult(other)) { adultContacts++; ApplyConstraints(); }
    }

    void OnCollisionExit2D(Collision2D other)
    {
        if (IsAdult(other))
        {
            adultContacts = Mathf.Max(0, adultContacts - 1);
            ApplyConstraints();
        }
    }

    void OnCollisionStay2D(Collision2D other)
    {
        if (!IsAdult(other)) return;
        var pc = other.gameObject.GetComponent<PlayerController>();
        if (pc == null) return;

        float dx = pc.MoveX;
        if (Mathf.Abs(dx) < 0.05f) return;
        if (Mathf.Abs(rb.linearVelocity.x) >= maxSpeed) return;

        rb.AddForce(new Vector2(Mathf.Sign(dx) * pushForce, 0f), ForceMode2D.Force);
    }

    static bool IsAdult(Collision2D other)
    {
        var pc = other.gameObject.GetComponent<PlayerController>();
        return pc != null && pc.kind == PlayerKind.Adult;
    }

    void ApplyConstraints()
    {
        if (adultContacts > 0)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        else
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
        }
    }
}
