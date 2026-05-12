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
}
#endif
