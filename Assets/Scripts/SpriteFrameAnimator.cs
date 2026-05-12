using UnityEngine;

// Anim simples por frames pra props decorativos (porta, portal). Sem dependência
// de PlayerController/Rigidbody — alvo: SpriteRenderer puro. Modo loop (portal
// idle) ou one-shot (porta abrindo até frame final).
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteFrameAnimator : MonoBehaviour
{
    public Sprite[] frames;
    public float fps = 8f;
    public bool loop = true;
    public bool autoPlay = true;

    private SpriteRenderer sr;
    private bool playing;
    private float t;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (frames != null && frames.Length > 0) sr.sprite = frames[0];
        if (autoPlay) Play();
    }

    public void Play()
    {
        if (frames == null || frames.Length == 0) return;
        playing = true;
        t = 0f;
    }

    void Update()
    {
        if (!playing || frames == null || frames.Length == 0) return;
        t += Time.deltaTime;
        int total = frames.Length;
        int idx = Mathf.FloorToInt(t * fps);
        if (loop)
        {
            sr.sprite = frames[idx % total];
        }
        else
        {
            if (idx >= total - 1)
            {
                sr.sprite = frames[total - 1];
                playing = false;
            }
            else
            {
                sr.sprite = frames[idx];
            }
        }
    }
}
