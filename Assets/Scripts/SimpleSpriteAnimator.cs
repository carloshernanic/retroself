using UnityEngine;

// Animator simples sem Unity Animator/AnimatorController. Recebe listas de
// Sprite (Idle/Walk/Jump) já fatiadas no import (Multiple mode) e cicla os
// frames com base no estado do PlayerController/Rigidbody2D.
[RequireComponent(typeof(SpriteRenderer))]
public class SimpleSpriteAnimator : MonoBehaviour
{
    public Sprite[] idleSprites;
    public Sprite[] walkSprites;
    public Sprite[] jumpSprites; // opcional: se vazio, usa idle no estado Jump

    public float idleFps = 8f;
    public float walkFps = 12f;
    public float jumpFps = 8f;

    // Fonte do estado: um dos dois é setado pelo builder.
    public PlayerController playerController;
    public Rigidbody2D rb;

    private SpriteRenderer sr;

    enum AnimState { Idle, Walk, Jump }
    private AnimState state;
    private float t;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (jumpSprites == null || jumpSprites.Length == 0) jumpSprites = idleSprites;
        Debug.Log($"[SimpleSpriteAnimator] {name}: idle={Count(idleSprites)}, walk={Count(walkSprites)}, jump={Count(jumpSprites)}");
        ApplyFrame(idleSprites, 0);
        AlignFeetToTransform();
    }

    // Alinha a borda inferior do sprite com a base do BoxCollider2D do parent,
    // independente do pivot que veio do Sprite Editor (Center, Bottom, qualquer).
    // O builder posiciona o body em -colliderSize.y/2, mas se o pivot do asset
    // não bater com isso o personagem afunda no chão. Aqui recalculamos baseado
    // nos bounds reais do sprite + offset+size do collider do parent.
    void AlignFeetToTransform()
    {
        if (sr == null || sr.sprite == null) return;
        var col = GetComponentInParent<BoxCollider2D>();
        if (col == null)
        {
            Debug.LogWarning($"[SimpleSpriteAnimator] {name}: sem BoxCollider2D no parent — pulando alinhamento");
            return;
        }

        float colBottomLocal = col.offset.y - col.size.y * 0.5f;
        float minY = sr.sprite.bounds.min.y;
        float maxY = sr.sprite.bounds.max.y;
        var lp = transform.localPosition;
        float newY = colBottomLocal - minY;
        Debug.Log($"[SimpleSpriteAnimator] {name}: align colBottom={colBottomLocal:F2} sprite.bounds=[{minY:F2}..{maxY:F2}] body.localY {lp.y:F2}→{newY:F2} pivot={sr.sprite.pivot} rect={sr.sprite.rect}");
        transform.localPosition = new Vector3(lp.x, newY, lp.z);
    }

    static int Count(Sprite[] arr) => arr == null ? 0 : arr.Length;

    void Update()
    {
        var newState = PickState();
        if (newState != state)
        {
            state = newState;
            t = 0f;
        }
        else
        {
            t += Time.deltaTime;
        }

        var (sprites, fps) = state switch
        {
            AnimState.Walk => (walkSprites, walkFps),
            AnimState.Jump => (jumpSprites, jumpFps),
            _ => (idleSprites, idleFps),
        };
        if (sprites == null || sprites.Length == 0) return;
        int idx = Mathf.FloorToInt(t * fps) % sprites.Length;
        ApplyFrame(sprites, idx);
    }

    AnimState PickState()
    {
        if (playerController != null)
        {
            if (!playerController.IsGrounded) return AnimState.Jump;
            if (Mathf.Abs(playerController.MoveX) > 0.1f) return AnimState.Walk;
            return AnimState.Idle;
        }
        if (rb != null)
        {
            // Sem ground check explícito — bully não pula no gameplay; só Idle/Walk.
            if (Mathf.Abs(rb.linearVelocity.x) > 0.1f) return AnimState.Walk;
            return AnimState.Idle;
        }
        return AnimState.Idle;
    }

    void ApplyFrame(Sprite[] sprites, int idx)
    {
        if (sprites == null || sprites.Length == 0) return;
        sr.sprite = sprites[Mathf.Clamp(idx, 0, sprites.Length - 1)];
    }
}
