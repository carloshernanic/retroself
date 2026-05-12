using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    public float cooldown = 0.4f;
    public float spawnOffsetX = 0.6f;
    public float stoneSpeed = 12f;
    public float stoneLifetime = 1.5f;
    public Color stoneColor = new Color(0.55f, 0.55f, 0.55f, 1f);

    private float cooldownCounter;
    private PlayerController controller;
    private float lastAttackTime = -999f;

    public float LastAttackTime => lastAttackTime;

    void Awake()
    {
        controller = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (cooldownCounter > 0f) cooldownCounter -= Time.deltaTime;

        // Só o jovem arremessa pedras. O adulto resolve via puzzle físico (HeavyBox).
        if (controller != null && controller.kind != PlayerKind.Young) return;

        var kb = Keyboard.current;
        if (kb == null) return;
        if (kb.kKey.wasPressedThisFrame && cooldownCounter <= 0f)
        {
            ThrowStone();
            cooldownCounter = cooldown;
        }
    }

    void ThrowStone()
    {
        lastAttackTime = Time.time;
        float dirX = (controller != null && controller.body != null && controller.body.flipX) ? -1f : 1f;
        Vector3 spawn = transform.position + new Vector3(dirX * spawnOffsetX, 0.1f, 0);

        var go = new GameObject("Stone");
        go.transform.position = spawn;
        go.transform.localScale = new Vector3(0.25f, 0.25f, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = StoneSprite();
        sr.color = stoneColor;
        sr.sortingOrder = 11;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        var stone = go.AddComponent<Stone>();
        stone.speed = stoneSpeed;
        stone.lifetime = stoneLifetime;
        stone.Launch(new Vector2(dirX, 0));
    }

    static Sprite cachedStone;
    static Sprite StoneSprite()
    {
        if (cachedStone != null) return cachedStone;
        var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        var px = new Color[16];
        for (int i = 0; i < 16; i++) px[i] = Color.white;
        tex.SetPixels(px);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        cachedStone = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
        return cachedStone;
    }
}
