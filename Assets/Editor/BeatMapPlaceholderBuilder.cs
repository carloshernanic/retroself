#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Gera Assets/Settings/BeatMap_M04.asset alinhado com retroself-gh.wav (130 BPM).
// Estrutura musical aprovada pelo usuário (1 ciclo):
//   [4 tempos vazio][8 Jovem][4 vazio][8 Adulto][4 vazio]
//   [4 Jovem][4 Jovem][4 Adulto][8 Jovem]
//
// Densidade: 1 nota a cada 2 tempos (130 BPM → ~1.08 notas/s) — confortável pra
// jogar com música. Total por ciclo: 18 notas em 48 tempos (~22.15s).
//
// Estende o ciclo repetidamente até cobrir toda a duração de retroself-gh.wav.
// Se o AudioClip não existir, gera só 1 ciclo como fallback.
//
// Sempre regera. Pra preservar ajustes manuais, mover o asset pra outro path antes.
public static class BeatMapPlaceholderBuilder
{
    const string AssetPath = "Assets/Settings/BeatMap_M04.asset";
    const string SongPath = "Assets/Audio/retroself-gh.wav";
    const float Bpm = 130f;
    // 1 nota a cada N tempos. 2 = cada meio-compasso (mais confortável que 1 por tempo).
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

        float beatDur = 60f / Bpm;

        int cycleBeats = 0;
        foreach (var s in Sections) cycleBeats += s.beats;
        float cycleSec = cycleBeats * beatDur;

        var song = AssetDatabase.LoadAssetAtPath<AudioClip>(SongPath);
        float targetSec = song != null ? song.length : cycleSec;
        int cycles = song != null ? Mathf.Max(1, Mathf.CeilToInt(targetSec / cycleSec)) : 1;

        int beat = 0;
        for (int c = 0; c < cycles; c++)
        {
            foreach (var sec in Sections)
            {
                if (sec.lane.HasValue)
                {
                    for (int i = 0; i < sec.beats; i += BeatsPerNote)
                    {
                        float t = (beat + i) * beatDur;
                        if (song != null && t > targetSec - 0.4f) break;
                        bm.notes.Add(new BeatMap.Note { time = t, lane = sec.lane.Value });
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

        float totalSec = beat * beatDur;
        string songInfo = song != null ? $", song {targetSec:F1}s" : ", sem audio clip (fallback 1 ciclo)";
        Debug.Log($"[BeatMapPlaceholderBuilder] Gerado {AssetPath}: {bm.notes.Count} notas em {totalSec:F1}s ({cycles} ciclos, {Bpm} BPM, 1/{BeatsPerNote} densidade{songInfo}).");
        EditorGUIUtility.PingObject(bm);
    }

    static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder)) return;
        var parent = System.IO.Path.GetDirectoryName(folder).Replace('\\', '/');
        var leaf = System.IO.Path.GetFileName(folder);
        if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, leaf);
    }
}
#endif
