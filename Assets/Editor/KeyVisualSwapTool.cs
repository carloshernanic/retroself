#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Patcher one-shot pra trocar o visual da Key bake-time em Memory_02_Domingo.unity
// pelo sprite cyberpunk (Assets/Resources/Sprites/key.png). Idempotente: se já
// estiver usando o sprite novo, só re-alinha scale/cor. Roda só na cena salva,
// sem rebuildar tudo.
public static class KeyVisualSwapTool
{
    const string ScenePath = "Assets/Scenes/Memory_02_Domingo.unity";
    const string SpritePath = "Assets/Resources/Sprites/key.png";

    [MenuItem("Retroself/Swap Memory_02 Key Visual")]
    public static void Swap()
    {
        if (!System.IO.File.Exists(ScenePath))
        {
            Debug.LogError($"[KeyVisualSwapTool] Cena não existe: {ScenePath}");
            return;
        }
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        if (sprite == null)
        {
            Debug.LogError($"[KeyVisualSwapTool] Sprite não encontrado: {SpritePath}");
            return;
        }

        var active = EditorSceneManager.GetActiveScene();
        if (active.isDirty) EditorSceneManager.SaveScene(active);

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject key = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            key = FindKeyRecursive(root.transform)?.gameObject;
            if (key != null) break;
        }
        if (key == null)
        {
            Debug.LogError("[KeyVisualSwapTool] GameObject 'Key' (com KeyPickup) não encontrado.");
            return;
        }

        var visualTr = key.transform.Find("Visual");
        if (visualTr == null)
        {
            Debug.LogError("[KeyVisualSwapTool] Filho 'Visual' não encontrado em Key.");
            return;
        }
        var sr = visualTr.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError("[KeyVisualSwapTool] SpriteRenderer ausente em Key/Visual.");
            return;
        }

        sr.sprite = sprite;
        sr.color = Color.white;
        visualTr.localScale = Vector3.one;
        sr.sortingOrder = 9;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        Debug.Log("[KeyVisualSwapTool] Key visual trocado pro sprite cyberpunk.");
    }

    static Transform FindKeyRecursive(Transform t)
    {
        if (t.GetComponent<KeyPickup>() != null) return t;
        for (int i = 0; i < t.childCount; i++)
        {
            var f = FindKeyRecursive(t.GetChild(i));
            if (f != null) return f;
        }
        return null;
    }
}
#endif
