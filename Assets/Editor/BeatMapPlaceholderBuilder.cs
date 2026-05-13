#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// Gera Assets/Settings/BeatMap_M04.asset alinhado ao retroself-gh.wav (130 BPM).
//
// Estrutura musical (1 ciclo de 48 tempos):
//   [4 vazio][8 Jovem][4 vazio][8 Adulto][4 vazio][4 Jovem][4 Jovem][4 Adulto][8 Jovem]
//
// Densidade: 1 nota a cada N tempos (`BeatsPerNote`). Estende em ciclos até cobrir
// a duração total do WAV.
//
// Onset detection (piano-retro_Master.wav): roda RMS-envelope no WAV do piano
// (versão de análise — não tocada no jogo) pra encontrar o downbeat 1 real do
// arquivo, armazena como `BeatMap.songStartOffset`. O GuitarHero usa esse offset
// pra sincar songTime ao primeiro tempo da música, eliminando erro de fase.
// Piano é melhor pra análise que o master final por ter ataques limpos.
public static class BeatMapPlaceholderBuilder
{
    const string AssetPath = "Assets/Settings/BeatMap_M04.asset";
    const string SongPath = "Assets/Audio/retroself-gh.wav";
    const string AnalysisWavName = "piano-retro_Master.wav";   // raiz do projeto, fora de Assets/
    const float Bpm = 130f;
    const int BeatsPerNote = 2;

    static readonly (int? lane, int beats)[] Sections = new (int?, int)[]
    {
        (null, 4),
        (0,    8),
        (null, 4),
        (1,    8),
        (null, 4),
        (0,    4),
        (0,    4),
        (1,    4),
        (0,    8),
    };

    [MenuItem("Retroself/Build BeatMap Placeholder")]
    public static void BuildPlaceholder()
    {
        EnsureFolder("Assets/Settings");

        var existing = AssetDatabase.LoadAssetAtPath<BeatMap>(AssetPath);
        var bm = existing != null ? existing : ScriptableObject.CreateInstance<BeatMap>();
        bm.notes.Clear();

        // 1) Detectar songStartOffset analisando o piano WAV.
        string analysisPath = Path.Combine(Directory.GetCurrentDirectory(), AnalysisWavName);
        float startOffset = 0f;
        int onsetCount = 0;
        if (File.Exists(analysisPath))
        {
            var onsets = DetectOnsets(analysisPath);
            onsetCount = onsets.Count;
            if (onsets.Count > 0) startOffset = onsets[0];
            if (onsets.Count > 1)
            {
                // Log dos primeiros 8 onsets pra eyeball.
                var preview = new System.Text.StringBuilder();
                int n = Mathf.Min(8, onsets.Count);
                for (int i = 0; i < n; i++) preview.Append($"{onsets[i]:F3} ");
                Debug.Log($"[BeatMap] Primeiros {n} onsets do piano: {preview}");
            }
        }
        else
        {
            Debug.LogWarning($"[BeatMap] {AnalysisWavName} não encontrado na raiz — usando songStartOffset=0.");
        }
        bm.songStartOffset = startOffset;

        // 2) Gerar notas pelo padrão de seções, em coordenada de música (sem somar offset).
        float beatDur = 60f / Bpm;
        int cycleBeats = 0;
        foreach (var s in Sections) cycleBeats += s.beats;
        float cycleSec = cycleBeats * beatDur;

        var song = AssetDatabase.LoadAssetAtPath<AudioClip>(SongPath);
        float songSec = song != null ? song.length : cycleSec + startOffset;
        // Tempo disponível após o offset.
        float availableSec = Mathf.Max(0f, songSec - startOffset);
        int cycles = Mathf.Max(1, Mathf.CeilToInt(availableSec / cycleSec));

        int beat = 0;
        int noteIdx = 0;
        for (int c = 0; c < cycles; c++)
        {
            foreach (var sec in Sections)
            {
                if (sec.lane.HasValue)
                {
                    for (int i = 0; i < sec.beats; i += BeatsPerNote)
                    {
                        float t = (beat + i) * beatDur;
                        // songTime em jogo == songSource.time - startOffset, então
                        // t é direto no espaço de música; só precisamos truncar pra
                        // não passar do fim do áudio.
                        if (t + 0.4f > availableSec) break;
                        // Padrão melódico simples: rotaciona colunas 0→1→2→1→...
                        // (zigue-zague tipo "do-re-mi-re") pra mão alternar sem
                        // sequência tediosa de A→S→D→A.
                        int[] pattern = { 0, 1, 2, 1 };
                        int col = pattern[noteIdx % pattern.Length];
                        bm.notes.Add(new BeatMap.Note { time = t, lane = sec.lane.Value, column = col });
                        noteIdx++;
                    }
                }
                beat += sec.beats;
            }
        }

        if (existing == null)
            AssetDatabase.CreateAsset(bm, AssetPath);
        else
            EditorUtility.SetDirty(bm);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[BeatMap] Gerado {AssetPath}: {bm.notes.Count} notas, {cycles} ciclos, " +
                  $"songStartOffset={startOffset:F3}s ({onsetCount} onsets no piano), " +
                  $"audio={songSec:F1}s, 1 nota/{BeatsPerNote} tempos, {Bpm} BPM.");
        EditorGUIUtility.PingObject(bm);
    }

    // ----- Onset detection -----

    // Envelope-RMS-based. Pra piano com ataques claros é razoável; pra material
    // mais sustained dá falsos negativos. Retorna lista em segundos, ordenada.
    static List<float> DetectOnsets(string wavPath)
    {
        var result = new List<float>();
        byte[] data;
        try { data = File.ReadAllBytes(wavPath); }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[BeatMap] erro lendo {wavPath}: {e.Message}");
            return result;
        }

        if (data.Length < 44 || data[0] != 'R' || data[1] != 'I' || data[2] != 'F' || data[3] != 'F'
            || data[8] != 'W' || data[9] != 'A' || data[10] != 'V' || data[11] != 'E')
        {
            Debug.LogWarning($"[BeatMap] {wavPath} não é WAV RIFF.");
            return result;
        }

        int sampleRate = 0, channels = 0, bitsPerSample = 0;
        int dataStart = -1, dataLen = 0;
        int p = 12;
        while (p + 8 <= data.Length)
        {
            string id = System.Text.Encoding.ASCII.GetString(data, p, 4);
            int len = System.BitConverter.ToInt32(data, p + 4);
            int contentStart = p + 8;
            if (id == "fmt ")
            {
                channels = System.BitConverter.ToInt16(data, contentStart + 2);
                sampleRate = System.BitConverter.ToInt32(data, contentStart + 4);
                bitsPerSample = System.BitConverter.ToInt16(data, contentStart + 14);
            }
            else if (id == "data")
            {
                dataStart = contentStart;
                dataLen = len;
                break;
            }
            p = contentStart + len;
            if ((len & 1) != 0) p++; // pad byte de alinhamento
        }

        if (dataStart < 0 || sampleRate <= 0 || bitsPerSample != 16 || channels <= 0)
        {
            Debug.LogWarning($"[BeatMap] WAV não-PCM-16bit ou sem 'data' chunk (sr={sampleRate} bps={bitsPerSample} ch={channels}).");
            return result;
        }

        int frameSize = 2 * channels;
        int numFrames = dataLen / frameSize;
        int winFrames = sampleRate / 100; // 10ms
        if (winFrames < 1) winFrames = 1;
        int numWindows = numFrames / winFrames;

        // Envelope RMS (mono mix).
        float[] env = new float[numWindows];
        float peak = 0f;
        for (int w = 0; w < numWindows; w++)
        {
            double sumSq = 0;
            int basePos = dataStart + w * winFrames * frameSize;
            for (int i = 0; i < winFrames; i++)
            {
                int frameOffset = basePos + i * frameSize;
                // Mix L+R pra mono.
                int sumCh = 0;
                for (int ch = 0; ch < channels; ch++)
                {
                    short s = System.BitConverter.ToInt16(data, frameOffset + ch * 2);
                    sumCh += s;
                }
                double mono = sumCh / (double)channels;
                sumSq += mono * mono;
            }
            float rms = (float)System.Math.Sqrt(sumSq / winFrames) / 32768f;
            env[w] = rms;
            if (rms > peak) peak = rms;
        }

        // Detecção de onsets: derivada positiva do envelope acima do limiar,
        // com spacing mínimo de 120ms pra não pegar tremolo/vibração.
        float onsetThresh = peak * 0.10f;
        float deltaThresh = peak * 0.05f;
        int minSpacing = 12; // janelas (120ms)
        int lastOnset = -minSpacing;
        for (int w = 2; w < numWindows; w++)
        {
            float delta = env[w] - env[w - 2];
            if (env[w] > onsetThresh && delta > deltaThresh && (w - lastOnset) >= minSpacing)
            {
                result.Add(w * 0.01f);
                lastOnset = w;
            }
        }

        return result;
    }

    static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder)) return;
        var parent = Path.GetDirectoryName(folder).Replace('\\', '/');
        var leaf = Path.GetFileName(folder);
        if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, leaf);
    }
}
#endif
