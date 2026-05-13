using UnityEngine;
using UnityEngine.SceneManagement;

// Singleton de áudio auto-inicializado (sem precisar adicionar nas cenas):
// - Música de fundo (loop) carregada de Resources/Audio/retroself-v1.wav
// - SFX procedurais (sintetizados em runtime, sem assets) pra jump/walk/shoot/dialog
//
// Spawna antes da primeira cena via RuntimeInitializeOnLoadMethod + DontDestroyOnLoad
// → atravessa MainMenu → Prologue → Memory_01/02 sem reload.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    const string MusicResourcePath = "Audio/retroself-v1";
    const float MusicTargetVolume = 0.45f;
    const float MusicFadeIn = 2f;
    const float SfxVolume = 0.55f;

    AudioSource musicSource;
    AudioSource sfxSource;

    // Clips procedurais cacheados — gerados 1× no Awake.
    AudioClip clipJump;
    AudioClip clipShoot;
    AudioClip clipStep;
    AudioClip clipDialog;
    AudioClip clipKey;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoInit()
    {
        if (Instance != null) return;
        var go = new GameObject("AudioManager");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<AudioManager>();
    }

    void Awake()
    {
        // Music
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = 0f;
        var clip = Resources.Load<AudioClip>(MusicResourcePath);
        if (clip != null)
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning($"[AudioManager] Música não encontrada em Resources/{MusicResourcePath}");
        }

        // SFX
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = SfxVolume;

        // Pre-gera clips procedurais.
        clipJump   = MakeChirp(durationSec: 0.12f, startHz: 380f, endHz: 720f, square: true);
        clipShoot  = MakeChirp(durationSec: 0.10f, startHz: 900f, endHz: 300f, square: true);
        clipStep   = MakeStep(durationSec: 0.05f);
        clipDialog = MakeBlip(durationSec: 0.04f, hz: 660f, square: true);
        clipKey    = MakeChime(durationSec: 0.5f);

        DisableLegacyMenuMusic();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene s, LoadSceneMode mode) { DisableLegacyMenuMusic(); }

    // Desliga AudioSources de instâncias antigas de MenuMusic pra evitar overlap.
    // (MenuMusic.cs original toca no MainMenu via component-in-scene.)
    void DisableLegacyMenuMusic()
    {
        foreach (var m in FindObjectsByType<MenuMusic>(FindObjectsSortMode.None))
        {
            var src = m.GetComponent<AudioSource>();
            if (src != null) { src.Stop(); src.enabled = false; }
            m.enabled = false;
        }
    }

    void Update()
    {
        if (musicSource != null && musicSource.isPlaying && musicSource.volume < MusicTargetVolume)
        {
            musicSource.volume = Mathf.MoveTowards(musicSource.volume, MusicTargetVolume,
                (MusicTargetVolume / MusicFadeIn) * Time.deltaTime);
        }
    }

    // ----- Public SFX API -----
    public static void PlayJump()   { Instance?.PlayClipPitched(Instance.clipJump,   Random.Range(0.95f, 1.05f)); }
    public static void PlayShoot()  { Instance?.PlayClipPitched(Instance.clipShoot,  Random.Range(0.97f, 1.04f)); }
    public static void PlayStep()   { Instance?.PlayClipPitched(Instance.clipStep,   Random.Range(0.90f, 1.10f), volScale: 0.6f); }
    public static void PlayDialog() { Instance?.PlayClipPitched(Instance.clipDialog, Random.Range(0.95f, 1.05f), volScale: 0.7f); }
    public static void PlayKeyCollect() { Instance?.PlayClipPitched(Instance.clipKey, 1f, volScale: 1.1f); }

    // ----- Music control -----
    // Usado pelo GuitarHeroMinigame: pausa a trilha de fundo enquanto o minigame
    // toca, e retoma quando o jogador fecha o overlay.
    public static void PauseMusic()
    {
        if (Instance == null || Instance.musicSource == null) return;
        if (Instance.musicSource.isPlaying) Instance.musicSource.Pause();
    }

    public static void ResumeMusic()
    {
        if (Instance == null || Instance.musicSource == null) return;
        if (Instance.musicSource.clip != null && !Instance.musicSource.isPlaying) Instance.musicSource.UnPause();
    }

    void PlayClipPitched(AudioClip clip, float pitch, float volScale = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(clip, SfxVolume * volScale);
    }

    // ----- Procedural waveform generators -----
    const int SR = 22050;

    // Chirp (frequência variando linearmente) com fade-out exponencial.
    static AudioClip MakeChirp(float durationSec, float startHz, float endHz, bool square)
    {
        int n = Mathf.Max(1, Mathf.RoundToInt(durationSec * SR));
        var samples = new float[n];
        double phase = 0;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;
            float hz = Mathf.Lerp(startHz, endHz, t);
            phase += (2.0 * System.Math.PI * hz) / SR;
            float wave = square ? (System.Math.Sin(phase) > 0 ? 1f : -1f) : Mathf.Sin((float)phase);
            float env = Mathf.Exp(-3f * t); // decay
            samples[i] = wave * env * 0.5f;
        }
        var clip = AudioClip.Create("Chirp", n, 1, SR, false);
        clip.SetData(samples, 0);
        return clip;
    }

    // Blip curto monotônico.
    static AudioClip MakeBlip(float durationSec, float hz, bool square)
    {
        int n = Mathf.Max(1, Mathf.RoundToInt(durationSec * SR));
        var samples = new float[n];
        double phase = 0;
        double step = (2.0 * System.Math.PI * hz) / SR;
        for (int i = 0; i < n; i++)
        {
            phase += step;
            float wave = square ? (System.Math.Sin(phase) > 0 ? 1f : -1f) : Mathf.Sin((float)phase);
            float env = 1f - ((float)i / n);
            samples[i] = wave * env * 0.5f;
        }
        var clip = AudioClip.Create("Blip", n, 1, SR, false);
        clip.SetData(samples, 0);
        return clip;
    }

    // Chime de coleta: 3 notas (C5→E5→G5) tipo Zelda mini, sine puro com decay.
    static AudioClip MakeChime(float durationSec)
    {
        int n = Mathf.Max(1, Mathf.RoundToInt(durationSec * SR));
        var samples = new float[n];
        float[] notes = { 523f, 659f, 784f };
        int noteLen = n / notes.Length;
        for (int k = 0; k < notes.Length; k++)
        {
            double phase = 0;
            double step = (2.0 * System.Math.PI * notes[k]) / SR;
            int start = k * noteLen;
            int end = Mathf.Min(start + noteLen, n);
            for (int i = start; i < end; i++)
            {
                phase += step;
                float local = (float)(i - start) / noteLen;
                float env = Mathf.Exp(-4f * local);
                samples[i] = Mathf.Sin((float)phase) * env * 0.45f;
            }
        }
        var clip = AudioClip.Create("Chime", n, 1, SR, false);
        clip.SetData(samples, 0);
        return clip;
    }

    // Passo: ruído branco filtrado com envelope curto (thump de pé).
    static AudioClip MakeStep(float durationSec)
    {
        int n = Mathf.Max(1, Mathf.RoundToInt(durationSec * SR));
        var samples = new float[n];
        float lowpass = 0f;
        var rng = new System.Random(0xBEEF);
        for (int i = 0; i < n; i++)
        {
            float noise = ((float)rng.NextDouble() * 2f - 1f);
            lowpass = Mathf.Lerp(lowpass, noise, 0.3f); // low-pass simples
            float env = Mathf.Exp(-12f * ((float)i / n));
            samples[i] = lowpass * env * 0.7f;
        }
        var clip = AudioClip.Create("Step", n, 1, SR, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
