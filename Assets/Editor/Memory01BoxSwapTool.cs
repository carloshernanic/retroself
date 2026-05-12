#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Patcher one-shot pra trocar o sprite da HeavyBox em Memory_01_Patio.unity pelo
// BasementBox (mesmo da fase 2), mantendo o BoxCollider2D existente intacto.
// Visual fica em child "BoxVisual" stretched pra cobrir o espaço do collider,
// compensando o pivot (0,0) bottom-left do PNG. Idempotente: re-rodar substitui
// só o BoxVisual.
public static class Memory01BoxSwapTool
{
    const string ScenePath = "Assets/Scenes/Memory_01_Patio.unity";

    [MenuItem("Retroself/Swap Memory_01 Box to BasementBox")]
    public static void Swap()
    {
        if (!System.IO.File.Exists(ScenePath))
        {
            Debug.LogError($"[Memory01BoxSwapTool] Cena não existe: {ScenePath}");
            return;
        }

        var active = EditorSceneManager.GetActiveScene();
        if (active.isDirty) EditorSceneManager.SaveScene(active);

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject box = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            box = FindChildRecursive(root.transform, "HeavyBox")?.gameObject;
            if (box != null) break;
        }
        if (box == null)
        {
            Debug.LogError("[Memory01BoxSwapTool] HeavyBox não encontrado na cena.");
            return;
        }

        var col = box.GetComponent<BoxCollider2D>();
        if (col == null)
        {
            Debug.LogError("[Memory01BoxSwapTool] HeavyBox não tem BoxCollider2D — abortando.");
            return;
        }

        // Desliga o SpriteRenderer original no root (se houver) — vamos usar child.
        var rootSr = box.GetComponent<SpriteRenderer>();
        if (rootSr != null) rootSr.enabled = false;

        // Remove BoxVisual antigo se já existe (idempotência).
        var existing = box.transform.Find("BoxVisual");
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SceneArtCatalog.BasementBox);
        if (sprite == null)
        {
            Debug.LogError($"[Memory01BoxSwapTool] Sprite não encontrado: {SceneArtCatalog.BasementBox}");
            return;
        }

        var visual = new GameObject("BoxVisual");
        visual.transform.SetParent(box.transform, false);
        var sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = rootSr != null ? rootSr.sortingOrder : 8;

        var native = sprite.bounds.size;
        Vector2 fit = col.size;
        visual.transform.localScale = new Vector3(fit.x / native.x, fit.y / native.y, 1f);
        var pivotNorm = new Vector2(
            sprite.pivot.x / sprite.rect.width,
            sprite.pivot.y / sprite.rect.height);
        visual.transform.localPosition = new Vector3(
            col.offset.x + (pivotNorm.x - 0.5f) * fit.x,
            col.offset.y + (pivotNorm.y - 0.5f) * fit.y, 0f);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        Debug.Log("[Memory01BoxSwapTool] HeavyBox visual trocado pra BasementBox.");
    }

    static Transform FindChildRecursive(Transform t, string name)
    {
        if (t.name == name) return t;
        for (int i = 0; i < t.childCount; i++)
        {
            var f = FindChildRecursive(t.GetChild(i), name);
            if (f != null) return f;
        }
        return null;
    }
}
#endif
