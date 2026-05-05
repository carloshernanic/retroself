using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSwap : MonoBehaviour
{
    public static PlayerSwap Instance { get; private set; }

    public PlayerController young;
    public PlayerController adult;

    public PlayerController Active { get; private set; }
    public GameObject ActivePlayer => Active != null ? Active.gameObject : null;

    public event Action<GameObject> OnActiveChanged;

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        // Os dois Woody coexistem mas não devem se empurrar — são o mesmo personagem
        // em idades diferentes. Ignora colisão entre os colliders.
        if (young != null && adult != null)
        {
            var yc = young.GetComponent<Collider2D>();
            var ac = adult.GetComponent<Collider2D>();
            if (yc != null && ac != null) Physics2D.IgnoreCollision(yc, ac, true);
        }

        SetActive(young != null ? young : adult, fireEvent: true);
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;
        if (kb.tabKey.wasPressedThisFrame) Toggle();
    }

    public void Toggle()
    {
        if (young == null || adult == null) return;
        var next = (Active == young) ? adult : young;
        Debug.Log($"[PlayerSwap] Toggle: {(Active == null ? "null" : Active.name)} -> {next.name}");
        SetActive(next, fireEvent: true);
    }

    // Re-aplica o estado do ativo. Usado pelo IntroDialogue após congelar/descongelar
    // os controles, pra garantir que enabled/alpha de young/adult voltem ao certo.
    public void RefreshActive()
    {
        if (Active != null) SetActive(Active, fireEvent: false);
    }

    void SetActive(PlayerController next, bool fireEvent)
    {
        if (next == null) return;
        Active = next;

        ToggleControl(young, young == next);
        ToggleControl(adult, adult == next);

        if (fireEvent) OnActiveChanged?.Invoke(next.gameObject);
    }

    static void ToggleControl(PlayerController pc, bool on)
    {
        if (pc == null) return;
        pc.enabled = on;
        var atk = pc.GetComponent<PlayerAttack>();
        if (atk != null) atk.enabled = on;
        var anim = pc.GetComponent<PlayerAnimator>();
        if (anim != null) anim.enabled = on;
        // PlayerHealth.Update reseta o alpha pra 1 se < 1 — desligando ele no inativo
        // mantemos o dim do alpha + o inativo também não toma dano.
        var hp = pc.GetComponent<PlayerHealth>();
        if (hp != null) hp.enabled = on;

        // Para o corpo inativo: zera horizontal pra ele não escorregar.
        var rb = pc.GetComponent<Rigidbody2D>();
        if (rb != null && !on)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        // Feedback visual: ativo cor cheia (PlayerAnimator restaura via baseColor),
        // inativo fica semi-transparente. Como PlayerAnimator do inativo está desligado,
        // o alpha persiste até o próximo swap.
        if (pc.body != null)
        {
            var c = pc.body.color;
            c.a = on ? 1f : 0.4f;
            pc.body.color = c;
        }
    }
}
