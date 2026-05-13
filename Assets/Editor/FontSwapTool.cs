#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Troca a fonte de todos os TMP_Text das cenas do projeto sem rebuildar a cena:
// abre cada .unity, walka os componentes TMP_Text, atribui VT323 (body) — exceto
// o logo "Title" do MainMenu que mantém Press Start 2P.
//
// Uso: rodar 'Retroself → Swap Fonts (VT323 body + PressStart2P title)' uma vez.
// Idempotente — re-rodar não faz nada novo se as fontes já estão corretas.
public static class FontSwapTool
{
    // GameObjects que devem MANTER Press Start 2P (logo, etc). Match exato por nome.
    static readonly HashSet<string> TitleNames = new HashSet<string> { "Title" };

    static readonly string[] Scenes =
    {
        "Assets/Scenes/MainMenu.unity",
        "Assets/Scenes/Prologue.unity",
        "Assets/Scenes/Memory_01_Patio.unity",
        "Assets/Scenes/Memory_02_Domingo.unity",
        "Assets/Scenes/Memory_03_Floresta.unity",
    };

    [MenuItem("Retroself/Swap Fonts (VT323 body + PressStart2P title)")]
    public static void SwapAll()
    {
        var body = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(SceneArtCatalog.PixelFontPath);
        var title = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(SceneArtCatalog.TitleFontPath);
        if (body == null)
        {
            Debug.LogError($"[FontSwapTool] Body font não encontrada em {SceneArtCatalog.PixelFontPath}. Rode Retroself → Build Pixel Font Asset primeiro.");
            return;
        }
        if (title == null)
        {
            Debug.LogError($"[FontSwapTool] Title font não encontrada em {SceneArtCatalog.TitleFontPath}.");
            return;
        }

        // Salva a cena ativa antes de mexer noutras.
        var activeScene = EditorSceneManager.GetActiveScene();
        if (activeScene.isDirty)
        {
            EditorSceneManager.SaveScene(activeScene);
        }

        int totalSwapped = 0;
        foreach (var path in Scenes)
        {
            if (!System.IO.File.Exists(path))
            {
                Debug.LogWarning($"[FontSwapTool] Cena {path} não existe — pulando.");
                continue;
            }

            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            int swapped = 0;

            // UI TMP (TextMeshProUGUI) e world-space TMP (TextMeshPro) herdam de TMP_Text.
            var texts = Resources.FindObjectsOfTypeAll<TMP_Text>();
            foreach (var t in texts)
            {
                if (t == null) continue;
                // Pula prefab assets fora da cena.
                if (!t.gameObject.scene.IsValid()) continue;
                if (t.gameObject.scene != scene) continue;

                var targetFont = TitleNames.Contains(t.gameObject.name) ? title : body;
                if (t.font != targetFont)
                {
                    Undo.RecordObject(t, "Swap TMP Font");
                    t.font = targetFont;
                    EditorUtility.SetDirty(t);
                    swapped++;
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, path);
            Debug.Log($"[FontSwapTool] {path}: {swapped} TMP_Text atualizados.");
            totalSwapped += swapped;
        }

        Debug.Log($"[FontSwapTool] Pronto. Total: {totalSwapped} componentes atualizados.");
    }

    // VT323 ocupa muito menos altura visual que Press Start 2P em ponto-tamanho
    // equivalente. Esta utility multiplica fontSize de todos os TMP_Text usando
    // VT323 (não-Title) por BumpFactor. Re-rodar multiplica de novo — calibrar
    // até ficar bom, depois parar.
    const float BumpFactor = 1.6f;

    [MenuItem("Retroself/Bump Body Font Sizes (x1.6)")]
    public static void BumpBodySizes()
    {
        var body = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(SceneArtCatalog.PixelFontPath);
        if (body == null)
        {
            Debug.LogError($"[FontSwapTool] Body font não encontrada em {SceneArtCatalog.PixelFontPath}.");
            return;
        }

        var activeScene = EditorSceneManager.GetActiveScene();
        if (activeScene.isDirty) EditorSceneManager.SaveScene(activeScene);

        int totalBumped = 0;
        foreach (var path in Scenes)
        {
            if (!System.IO.File.Exists(path)) continue;
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            int bumped = 0;

            var texts = Resources.FindObjectsOfTypeAll<TMP_Text>();
            foreach (var t in texts)
            {
                if (t == null) continue;
                if (!t.gameObject.scene.IsValid()) continue;
                if (t.gameObject.scene != scene) continue;
                if (t.font != body) continue; // só os que estão em VT323
                if (TitleNames.Contains(t.gameObject.name)) continue;

                Undo.RecordObject(t, "Bump TMP fontSize");
                t.fontSize *= BumpFactor;
                EditorUtility.SetDirty(t);
                bumped++;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, path);
            Debug.Log($"[FontSwapTool] {path}: {bumped} fontSize bumped por {BumpFactor}.");
            totalBumped += bumped;
        }

        Debug.Log($"[FontSwapTool] Bump pronto. Total: {totalBumped} componentes (×{BumpFactor}).");
    }

    // Copia fontSize de Memory_02 pra Memory_03 indexando TMP_Text por hierarchy path
    // (Canvas/Box/Label, etc). M_03 foi construído antes do bump×1.6×1.6 das outras
    // cenas — esse menu sincroniza sem rebuild. Idempotente.
    [MenuItem("Retroself/Sync Memory_03 Font Sizes With Memory_02")]
    public static void SyncMemory03Sizes()
    {
        const string srcPath = "Assets/Scenes/Memory_02_Domingo.unity";
        const string dstPath = "Assets/Scenes/Memory_03_Floresta.unity";
        if (!System.IO.File.Exists(srcPath) || !System.IO.File.Exists(dstPath))
        {
            Debug.LogError($"[FontSwapTool] Faltam cenas: {srcPath} ou {dstPath}.");
            return;
        }

        var activeScene = EditorSceneManager.GetActiveScene();
        if (activeScene.isDirty) EditorSceneManager.SaveScene(activeScene);

        // 1ª passada: index por path "Canvas/Box/Label" → fontSize de M_02.
        var src = EditorSceneManager.OpenScene(srcPath, OpenSceneMode.Single);
        var sizesByPath = new Dictionary<string, float>();
        foreach (var t in Resources.FindObjectsOfTypeAll<TMP_Text>())
        {
            if (t == null) continue;
            if (!t.gameObject.scene.IsValid() || t.gameObject.scene != src) continue;
            sizesByPath[GetHierarchyPath(t.transform)] = t.fontSize;
        }

        // 2ª passada: aplica em M_03 todos os matches por path. Fallback: match por nome
        // (sufixo do path) — cobre casos onde a hierarquia divergiu mas o nome do GO bate.
        var sizesByName = new Dictionary<string, float>();
        foreach (var kv in sizesByPath)
        {
            var name = kv.Key.Substring(kv.Key.LastIndexOf('/') + 1);
            sizesByName[name] = kv.Value; // últimas escritas vencem — OK pra grupos pequenos.
        }

        var dst = EditorSceneManager.OpenScene(dstPath, OpenSceneMode.Single);
        int synced = 0;
        foreach (var t in Resources.FindObjectsOfTypeAll<TMP_Text>())
        {
            if (t == null) continue;
            if (!t.gameObject.scene.IsValid() || t.gameObject.scene != dst) continue;
            if (TitleNames.Contains(t.gameObject.name)) continue; // Title fica como está (PressStart2P).

            float newSize;
            if (sizesByPath.TryGetValue(GetHierarchyPath(t.transform), out newSize) ||
                sizesByName.TryGetValue(t.gameObject.name, out newSize))
            {
                if (Mathf.Abs(t.fontSize - newSize) > 0.01f)
                {
                    Undo.RecordObject(t, "Sync TMP fontSize");
                    t.fontSize = newSize;
                    EditorUtility.SetDirty(t);
                    synced++;
                }
            }
        }
        EditorSceneManager.MarkSceneDirty(dst);
        EditorSceneManager.SaveScene(dst, dstPath);
        Debug.Log($"[FontSwapTool] Memory_03 sincronizado com Memory_02: {synced} TMP_Text ajustados.");
    }

    static string GetHierarchyPath(Transform t)
    {
        var sb = new System.Text.StringBuilder(t.name);
        var p = t.parent;
        while (p != null)
        {
            sb.Insert(0, "/");
            sb.Insert(0, p.name);
            p = p.parent;
        }
        return sb.ToString();
    }
}
#endif
