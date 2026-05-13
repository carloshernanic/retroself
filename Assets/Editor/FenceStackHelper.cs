#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// Empilha os 4 sprites de fence (FenceBottom/MiddleA/MiddleB/Top do business-center
// pack) preenchendo (w,h) world units em torno de worldPos, como visual da
// BreakablePlank. Cada PNG é 32×32 (PPU 32, pivot Center via
// SceneArtImportConfigurator).
//
// Layout: bottom = âncora (1ª peça), middleA/B intercalados pra dar variedade nos
// tiles do meio, top = última peça. Se a plank for curta (h ≤ 2u) usa só 2 peças
// (bottom + top); senão preenche o meio com nTiles = round(h / 1u).
public static class FenceStackHelper
{
    public static GameObject Build(Transform parent, string namePrefix, float w, float h, int sortingOrder)
    {
        var sprBot = AssetDatabase.LoadAssetAtPath<Sprite>(SceneArtCatalog.FenceBottom);
        var sprMidA = AssetDatabase.LoadAssetAtPath<Sprite>(SceneArtCatalog.FenceMiddleA);
        var sprMidB = AssetDatabase.LoadAssetAtPath<Sprite>(SceneArtCatalog.FenceMiddleB);
        var sprTop = AssetDatabase.LoadAssetAtPath<Sprite>(SceneArtCatalog.FenceTop);
        if (sprBot == null || sprTop == null)
        {
            Debug.LogWarning("[FenceStackHelper] Fence sprites não encontrados. " +
                             "Rode 'Retroself → Configure Scene Art Imports' uma vez.");
            return null;
        }

        var root = new GameObject(namePrefix);
        root.transform.SetParent(parent, false);
        root.transform.localPosition = Vector3.zero;

        // Quantos tiles ao todo (mínimo 2: bottom+top). Cada tile = h/n de altura.
        int n = Mathf.Max(2, Mathf.RoundToInt(h / 1f));
        float tileH = h / n;

        for (int i = 0; i < n; i++)
        {
            Sprite spr;
            if (i == 0)            spr = sprBot;
            else if (i == n - 1)   spr = sprTop;
            else                   spr = (i % 2 == 0) ? sprMidA : sprMidB;

            var tile = new GameObject($"{namePrefix}_{i}");
            tile.transform.SetParent(root.transform, false);

            // Centro do tile em y = -h/2 + tileH*(i+0.5).
            float cy = -h * 0.5f + tileH * (i + 0.5f);
            tile.transform.localPosition = new Vector3(0, cy, 0);

            var sr = tile.AddComponent<SpriteRenderer>();
            sr.sprite = spr;
            sr.sortingOrder = sortingOrder;

            // Sprite é 1×1u (PPU 32) — scale = (w, tileH) preenche exatamente.
            tile.transform.localScale = new Vector3(w, tileH, 1f);
        }

        return root;
    }
}
#endif
