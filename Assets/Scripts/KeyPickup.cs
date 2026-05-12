using UnityEngine;

// Chave coletável pelo player (qualquer Woody serve). Usa flag estática reseta
// no boot da subsystem pra que SchoolDoor possa checar requisito sem manter
// referência direta ao GameObject (que é destruído ao coletar).
[RequireComponent(typeof(Collider2D))]
public class KeyPickup : MonoBehaviour
{
    public static bool Collected { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetState() { Collected = false; }

    // Reset cross-cena: SubsystemRegistration roda só no boot do app, então sair
    // de Memory_02 com chave coletada e voltar pra Memory_01 deixaria a porta da
    // escola "pré-aberta". SceneStartReset chama isto no Awake da cena.
    public static void Reset() { Collected = false; }

    void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (Collected) return;
        if (other.GetComponent<PlayerController>() == null) return;
        Collected = true;
        AudioManager.PlayKeyCollect();
        Destroy(gameObject);
    }

    // Cria uma chave em runtime (usado pelo KeyDropper quando o bully cai). Mantém
    // o mesmo visual/setup que o builder usa em first-spawn — mas executável fora
    // do editor.
    public static GameObject Spawn(Vector3 worldPos)
    {
        var root = new GameObject("Key");
        root.transform.position = worldPos;

        var visual = new GameObject("Visual");
        visual.transform.SetParent(root.transform, false);
        visual.transform.localPosition = Vector3.zero;
        var sprite = LoadKeySprite();
        var sr = visual.AddComponent<SpriteRenderer>();
        if (sprite != null)
        {
            sr.sprite = sprite;
            visual.transform.localScale = Vector3.one;
            sr.color = Color.white;
        }
        else
        {
            sr.sprite = GoldSquareSprite();
            sr.color = new Color(1f, 0.85f, 0.25f, 1f);
            visual.transform.localScale = new Vector3(0.35f, 0.35f, 1f);
        }
        sr.sortingOrder = 9;

        var col = root.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.6f, 0.6f);

        root.AddComponent<KeyPickup>();

        var bob = root.AddComponent<IdleBob>();
        bob.amplitude = 0.12f;
        bob.speed = 2.2f;
        return root;
    }

    static Sprite cachedKeySprite;
    static bool keySpriteLoadAttempted;
    static Sprite LoadKeySprite()
    {
        if (cachedKeySprite != null) return cachedKeySprite;
        if (keySpriteLoadAttempted) return null;
        keySpriteLoadAttempted = true;
        cachedKeySprite = Resources.Load<Sprite>("Sprites/key");
        return cachedKeySprite;
    }

    static Sprite cachedSprite;
    static Sprite GoldSquareSprite()
    {
        if (cachedSprite != null) return cachedSprite;
        var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        var px = new Color[16];
        for (int i = 0; i < 16; i++) px[i] = Color.white;
        tex.SetPixels(px);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        cachedSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
        return cachedSprite;
    }
}
