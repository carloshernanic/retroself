using UnityEngine;

// SFX procedural pra puzzle feedback (pressure plate click + target hit). Gera
// AudioClips no primeiro acesso via síntese aditiva — sem dependência de WAVs
// nos Assets. Mantém o "feel" 8-bit do projeto.
//
// Player único 2D (spatialBlend=0) parented em DontDestroyOnLoad — sobrevive
// trocas de cena, então o clique não corta no meio se algo carrega cena.
public static class SfxBeep
{
    static AudioClip plateOnClip;
    static AudioClip plateOffClip;
    static AudioClip targetHitClip;
    static AudioClip breakClip;
    static AudioSource source;

    // Criamos o source no boot (antes da 1ª cena) em vez de lazy. Se criássemos
    // sob demanda durante o teardown da cena (ex: OnTriggerExit das placas ao
    // unload), o Unity warna "Some objects were not cleaned up when closing the
    // scene" porque um GameObject novo apareceu já no ciclo de unload.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void EnsureSource()
    {
        if (source != null) return;
        var go = new GameObject("[SfxBeep]");
        Object.DontDestroyOnLoad(go);
        source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
    }

    static AudioSource Source
    {
        get
        {
            // Defensivo: se algo chamar antes do BeforeSceneLoad (não deveria),
            // ou se o GO foi destruído manualmente, recria.
            if (source == null) EnsureSource();
            return source;
        }
    }

    public static void PlayPlateOn()
    {
        if (plateOnClip == null) plateOnClip = BuildBlip(660f, 0.09f, square: true, sweep: 140f);
        Source.PlayOneShot(plateOnClip, 0.35f);
    }

    public static void PlayPlateOff()
    {
        if (plateOffClip == null) plateOffClip = BuildBlip(440f, 0.09f, square: true, sweep: -140f);
        Source.PlayOneShot(plateOffClip, 0.25f);
    }

    public static void PlayTargetHit()
    {
        if (targetHitClip == null) targetHitClip = BuildBlip(1320f, 0.15f, square: true, sweep: -600f);
        Source.PlayOneShot(targetHitClip, 0.55f);
    }

    public static void PlayBreak()
    {
        if (breakClip == null) breakClip = BuildWoodCrack(0.32f);
        Source.PlayOneShot(breakClip, 0.7f);
    }

    // Madeira quebrando: 3 camadas somadas
    //   (a) Estalo: ruído filtrado curto, ataque imediato (3ms), decay rápido.
    //   (b) Thump: square wave grave (~120Hz→60Hz sweep) pro "corpo" do impacto.
    //   (c) Tail: ruído mais grave, decay longo, simulando lascas no chão.
    static AudioClip BuildWoodCrack(float duration)
    {
        const int sampleRate = 44100;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        var data = new float[samples];
        var rand = new System.Random(7331);
        // 1-pole lowpass pro ruído ficar menos "siseado" e mais "leve madeira"
        float lp = 0f;
        float lpA = 0.35f;
        float phase = 0f;

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;

            // (a) Estalo curto — ruído com decay 50ms
            float crackEnv = Mathf.Clamp01(t / 0.002f) * Mathf.Exp(-t / 0.05f);
            float noise = (float)(rand.NextDouble() * 2.0 - 1.0);
            lp = Mathf.Lerp(lp, noise, lpA);
            float crack = lp * crackEnv * 0.7f;

            // (b) Thump grave — square 120Hz descendo pra 60Hz, decay 80ms
            float thumpFreq = 120f - 60f * Mathf.Clamp01(t / 0.08f);
            phase += 2f * Mathf.PI * thumpFreq / sampleRate;
            float thumpEnv = Mathf.Clamp01(t / 0.003f) * Mathf.Exp(-t / 0.08f);
            float thump = (Mathf.Sin(phase) >= 0f ? 1f : -1f) * thumpEnv * 0.45f;

            // (c) Tail — ruído escuro com decay quadrático longo
            float tailNoise = (float)(rand.NextDouble() * 2.0 - 1.0) * 0.35f;
            float tailEnv = Mathf.Exp(-t / 0.18f);
            float tail = tailNoise * tailEnv * 0.25f;

            data[i] = Mathf.Clamp(crack + thump + tail, -0.95f, 0.95f);
        }

        var clip = AudioClip.Create("WoodCrack", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    static AudioClip BuildBlip(float freq, float duration, bool square, float sweep = 0f)
    {
        const int sampleRate = 44100;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        var data = new float[samples];
        float phase = 0f;
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float f = freq + sweep * (t / duration);
            phase += 2f * Mathf.PI * f / sampleRate;
            float w = square ? (Mathf.Sin(phase) >= 0f ? 1f : -1f) : Mathf.Sin(phase);
            // ADSR simples: attack 5ms, decay até o fim.
            float attack = Mathf.Clamp01(t / 0.005f);
            float decay  = Mathf.Clamp01(1f - (t - 0.005f) / Mathf.Max(0.001f, duration - 0.005f));
            data[i] = w * attack * decay * 0.3f;
        }
        var clip = AudioClip.Create("Blip", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
