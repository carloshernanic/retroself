#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// Gera Assets/Settings/BeatMap_M04.asset com ~30 notas distribuídas em ~25s.
// Padrão alternante (4 notas Young, 4 Adult, ...) pra ensinar Tab swap.
// Densidade ~1.2 notas/s — controlável pelo `passThreshold` do GH.
//
// Idempotente: se asset existir e tiver notas, NÃO sobrescreve (preserva ajustes
// manuais). Se vazio ou inexistente, gera placeholder. Pra regerar, deletar o
// asset antes.
public static class BeatMapPlaceholderBuilder
{
    const string AssetPath = "Assets/Settings/BeatMap_M04.asset";
    const float Lead = 2.5f;           // tempo inicial antes da 1ª nota
    const int   NoteCount = 30;
    const float NoteInterval = 0.8f;   // ~1.25 notas/s
    const int   SwapEvery = 4;         // alterna lane a cada N notas

    [MenuItem("Retroself/Build BeatMap Placeholder")]
    public static void BuildPlaceholder()
    {
        EnsureFolder("Assets/Settings");

        var existing = AssetDatabase.LoadAssetAtPath<BeatMap>(AssetPath);
        if (existing != null && existing.notes != null && existing.notes.Count > 0)
        {
            Debug.Log($"[BeatMapPlaceholderBuilder] {AssetPath} já existe com {existing.notes.Count} notas — pulei (delete o asset pra regerar).");
            EditorGUIUtility.PingObject(existing);
            return;
        }

        var bm = existing != null ? existing : ScriptableObject.CreateInstance<BeatMap>();
        bm.notes.Clear();
        int currentLane = 0;
        for (int i = 0; i < NoteCount; i++)
        {
            if (i > 0 && i % SwapEvery == 0) currentLane = 1 - currentLane;
            bm.notes.Add(new BeatMap.Note { time = Lead + i * NoteInterval, lane = currentLane });
        }

        if (existing == null)
            AssetDatabase.CreateAsset(bm, AssetPath);
        else
            EditorUtility.SetDirty(bm);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[BeatMapPlaceholderBuilder] Gerado {AssetPath} com {bm.notes.Count} notas (lead {Lead}s, intervalo {NoteInterval}s, swap a cada {SwapEvery}).");
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
