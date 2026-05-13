using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Guitar Hero co-op com 2 lanes (Young à esquerda, Adult à direita). Notas
// caem do topo até a hit zone na base. Player aperta Espaço (lane ativa),
// Z (force lane Young) ou X (force lane Adult) pra acertar. Tab alterna qual
// lane está "ativa" (a outra fica acinzentada — notas nela = miss automático).
//
// BeatMap (ScriptableObject) define quando/onde cada nota nasce. AudioClip
// `song` opcional: se null, toca tick procedural a cada nota como metrônomo.
//
// Win = accuracy >= passThreshold (default 70%).
public class GuitarHeroMinigame : MinigameOverlay
{
    [Header("Beat data")]
    public BeatMap beatMap;
    public AudioClip song;
    [Tooltip("Tempo (s) que a nota leva do topo até a hit zone.")]
    public float noteFallTime = 2.0f;
    [Tooltip("Margem (s) pra hit ser válido — ±janela.")]
    public float hitWindow = 0.25f;
    [Range(0f, 1f)] public float passThreshold = 0.5f;
    [Tooltip("Offset (s) entre songTime=0 e a primeira nota. Compensa pre-roll do beatmap.")]
    public float songStartOffset = 0f;

    [Header("UI Refs")]
    public RectTransform laneYoungArea;
    public RectTransform laneAdultArea;
    public RectTransform hitZoneYoung;
    public RectTransform hitZoneAdult;
    public TMP_Text scoreText;
    public TMP_Text statusText;
    public Image laneYoungOverlay;
    public Image laneAdultOverlay;

    [Header("Cores")]
    public Color youngColor = new Color(1f, 0.85f, 0.25f, 1f);
    public Color adultColor = new Color(0.7f, 0.45f, 0.25f, 1f);
    // Inativa fica só levemente esmaecida pras notas continuarem visíveis (bug
    // anterior: alpha 0.55 escondia as notas, jogador achava que só Espaço/lane
    // ativa registrava). Ambas as lanes recebem hits independentes via Z/X/setas.
    public Color inactiveOverlayColor = new Color(0f, 0f, 0f, 0.08f);
    public Color activeOverlayColor = new Color(0f, 0f, 0f, 0f);

    int activeLane;
    float songTime;
    int nextSpawnIdx;
    int notesScored;
    int hits;
    int totalNotes;
    bool finished;

    class FlyingNote
    {
        public Image img;
        public int lane;
        public float spawnTime;
        public float hitTime;
        public bool resolved;
    }
    readonly List<FlyingNote> live = new List<FlyingNote>();

    AudioSource songSource;

    protected override void Awake()
    {
        base.Awake();
        songSource = GetComponent<AudioSource>();
        if (songSource == null)
        {
            songSource = gameObject.AddComponent<AudioSource>();
            songSource.playOnAwake = false;
            songSource.spatialBlend = 0f;
        }
    }

    protected override void OnStart()
    {
        // Limpa estado.
        foreach (var n in live) if (n.img != null) Destroy(n.img.gameObject);
        live.Clear();
        nextSpawnIdx = 0;
        notesScored = 0;
        hits = 0;
        songTime = 0f;
        finished = false;
        activeLane = 0;
        totalNotes = beatMap != null ? beatMap.notes.Count : 0;
        UpdateOverlay();
        UpdateScoreText();
        if (statusText != null) statusText.text = "Qualquer tecla de hit toca a nota mais perto. Esc desiste.";

        AudioManager.PauseMusic();

        if (song != null)
        {
            songSource.clip = song;
            songSource.time = 0f;
            songSource.Play();
        }
    }

    protected override void OnEnd(bool won)
    {
        if (songSource != null && songSource.isPlaying) songSource.Stop();
        AudioManager.ResumeMusic();
        foreach (var n in live) if (n != null && n.img != null) Destroy(n.img.gameObject);
        live.Clear();
    }

    protected override void TickGame()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.escapeKey.wasPressedThisFrame) { Lose(); return; }

        if (finished) return;

        // Sincroniza songTime ao playback do AudioSource quando há música — evita
        // drift entre relógio do jogo (unscaledDeltaTime) e thread de áudio.
        // Sem música, acumula manualmente (fallback metronômico).
        if (song != null && songSource != null && songSource.isPlaying)
            songTime = songSource.time - songStartOffset;
        else
            songTime += Time.unscaledDeltaTime;

        if (kb.tabKey.wasPressedThisFrame)
        {
            activeLane = 1 - activeLane;
            UpdateOverlay();
        }
        // Auto-sense total: QUALQUER tecla de hit busca a nota mais próxima em
        // qualquer lane. Antes Z/A/← e X/D/→ travavam na lane fixa (0 ou 1),
        // então o jogador apertava perto de uma nota Adulta com Z e nada
        // acontecia — sensação de "só a tecla do meio funciona". Agora todas as
        // 9 teclas têm comportamento idêntico: pega a nota mais próxima dentro
        // da hit window, lane que for.
        bool anyHit = kb.spaceKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame
                   || kb.downArrowKey.wasPressedThisFrame
                   || kb.zKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame
                   || kb.leftArrowKey.wasPressedThisFrame
                   || kb.xKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame
                   || kb.rightArrowKey.wasPressedThisFrame;
        if (anyHit) TryHitAny();

        // Spawn das próximas notas (start mostra antes do hitTime por noteFallTime).
        if (beatMap != null)
        {
            while (nextSpawnIdx < beatMap.notes.Count)
            {
                var n = beatMap.notes[nextSpawnIdx];
                if (songTime + noteFallTime < n.time) break;
                SpawnNote(n);
                nextSpawnIdx++;
            }
        }

        // Atualiza posição das notas + resolve passou da hit zone.
        // t é função de songTime vs hitTime — independente de quando spawnou,
        // pra primeira nota não chegar atrasada quando songTime começa em 0.
        for (int i = live.Count - 1; i >= 0; i--)
        {
            var fn = live[i];
            float tRaw = 1f - (fn.hitTime - songTime) / noteFallTime;
            UpdateNotePosition(fn, Mathf.Clamp01(tRaw));

            if (fn.resolved) { if (tRaw >= 1.05f) { Destroy(fn.img.gameObject); live.RemoveAt(i); } continue; }

            // Miss automático se passou da hit zone sem hit.
            if (songTime > fn.hitTime + hitWindow)
            {
                fn.resolved = true;
                fn.img.color = new Color(0.4f, 0.4f, 0.4f, 0.6f);
                notesScored++;
                SfxBeep.PlayBeatMiss();
                UpdateScoreText();
                CheckFinish();
            }
        }

        // Tick procedural pra notas se sem música (metrônomo no momento da nota).
        // Implementado em SpawnNote schedulando — ver lá.
    }

    void SpawnNote(BeatMap.Note n)
    {
        var area = n.lane == 0 ? laneYoungArea : laneAdultArea;
        if (area == null) return;
        var go = new GameObject($"Note_{n.lane}", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(area, false);
        var img = go.GetComponent<Image>();
        img.color = n.lane == 0 ? youngColor : adultColor;
        img.raycastTarget = false;
        var rt = img.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(80f, 36f);

        var fn = new FlyingNote
        {
            img = img,
            lane = n.lane,
            spawnTime = songTime,
            hitTime = n.time,
            resolved = false
        };
        live.Add(fn);
        UpdateNotePosition(fn, 0f);
    }

    void UpdateNotePosition(FlyingNote fn, float t)
    {
        var area = fn.lane == 0 ? laneYoungArea : laneAdultArea;
        var hit = fn.lane == 0 ? hitZoneYoung : hitZoneAdult;
        if (area == null) return;
        float top = 0f; // anchor top, pivot center → y=0 está no topo da área
        float bottom = -area.rect.height + (hit != null ? hit.rect.height * 0.5f : 60f);
        float y = Mathf.Lerp(top, bottom, t);
        fn.img.rectTransform.anchoredPosition = new Vector2(0f, y);
    }

    void TryHit(int lane)
    {
        FlyingNote best = null;
        float bestDelta = float.MaxValue;
        foreach (var fn in live)
        {
            if (fn.resolved) continue;
            if (fn.lane != lane) continue;
            float delta = Mathf.Abs(songTime - fn.hitTime);
            if (delta < bestDelta) { best = fn; bestDelta = delta; }
        }
        ResolveBest(best, bestDelta);
    }

    void TryHitAny()
    {
        FlyingNote best = null;
        float bestDelta = float.MaxValue;
        foreach (var fn in live)
        {
            if (fn.resolved) continue;
            float delta = Mathf.Abs(songTime - fn.hitTime);
            if (delta < bestDelta) { best = fn; bestDelta = delta; }
        }
        ResolveBest(best, bestDelta);
    }

    void ResolveBest(FlyingNote best, float bestDelta)
    {
        if (best == null) return;
        if (bestDelta > hitWindow) return;
        best.resolved = true;
        bool perfect = bestDelta < hitWindow * 0.4f;
        best.img.color = perfect ? Color.white : new Color(1f, 1f, 0.6f, 1f);
        notesScored++;
        hits++;
        SfxBeep.PlayBeatHit();
        UpdateScoreText();
        CheckFinish();
    }

    void CheckFinish()
    {
        if (notesScored < totalNotes) return;
        if (totalNotes <= 0) return;
        finished = true;
        float acc = (float)hits / totalNotes;
        if (statusText != null) statusText.text = $"Acertos {Mathf.RoundToInt(acc * 100f)}% — " + (acc >= passThreshold ? "VOCÊ MANDOU BEM!" : "tente de novo.");
        if (acc >= passThreshold) Win();
        else Lose();
    }

    void UpdateScoreText()
    {
        if (scoreText != null) scoreText.text = $"{hits} / {totalNotes}";
    }

    void UpdateOverlay()
    {
        if (laneYoungOverlay != null) laneYoungOverlay.color = activeLane == 0 ? activeOverlayColor : inactiveOverlayColor;
        if (laneAdultOverlay != null) laneAdultOverlay.color = activeLane == 1 ? activeOverlayColor : inactiveOverlayColor;
    }
}
