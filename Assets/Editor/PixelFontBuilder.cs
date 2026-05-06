#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

// Gera um TMP_FontAsset SDF a partir de Press Start 2P TTF. Inclui o ASCII básico
// + acentuações pt-BR (áéíóúâêôãõçÁÉÍÓÚÂÊÔÃÕÇ) — o suficiente pra todos os
// diálogos das 3 cenas existentes.
//
// Uso: rodar 'Retroself → Build Pixel Font Asset' uma vez. Re-rodar reescreve
// o asset existente.
public static class PixelFontBuilder
{
    const string TtfPath = "Assets/Fonts/PressStart2P-Regular.ttf";
    const string OutPath = "Assets/Fonts/PressStart2P-SDF.asset";

    // Caracteres do glyph table. Press Start 2P tem suporte limitado a acentos —
    // se algum não renderizar, o TMP cai no missing-glyph (□). Aceitável.
    const string Charset =
        " !\"#$%&'()*+,-./0123456789:;<=>?@" +
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
        "[\\]^_`" +
        "abcdefghijklmnopqrstuvwxyz" +
        "{|}~" +
        "áéíóúâêôãõçÁÉÍÓÚÂÊÔÃÕÇ" +
        "àÀüÜñÑ" +
        "—–…“”‘’«»·";

    [MenuItem("Retroself/Build Pixel Font Asset")]
    public static void Build()
    {
        var ttf = AssetDatabase.LoadAssetAtPath<Font>(TtfPath);
        if (ttf == null)
        {
            Debug.LogError($"[PixelFontBuilder] Não achei o TTF em {TtfPath}. Coloque PressStart2P-Regular.ttf lá e rode de novo.");
            return;
        }

        // Atlas 512×512 SDF padding 5 — suficiente pro charset acima em pixel font 8px.
        const int atlasSize = 512;
        const int padding = 5;
        const int samplingPointSize = 64; // SDF sample size; o tamanho real do texto é set por TMP fontSize.

        var fontAsset = TMP_FontAsset.CreateFontAsset(
            ttf,
            samplingPointSize,
            padding,
            GlyphRenderMode.SDFAA,
            atlasSize, atlasSize,
            AtlasPopulationMode.Dynamic);

        if (fontAsset == null)
        {
            Debug.LogError("[PixelFontBuilder] CreateFontAsset retornou null.");
            return;
        }

        fontAsset.name = "PressStart2P-SDF";

        // Pré-popula o atlas com os caracteres do Charset (Dynamic mode aceita
        // glifos novos em runtime, mas pré-popular evita pop-in no primeiro typewriter).
        // Overload de TryAddCharacters que aceita string + out string missingChars.
        fontAsset.TryAddCharacters(Charset, out string missing);
        if (!string.IsNullOrEmpty(missing))
            Debug.LogWarning($"[PixelFontBuilder] Press Start 2P não suporta: '{missing}' — vão renderizar como □.");

        // Salva como asset no projeto.
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(OutPath);
        if (existing != null) AssetDatabase.DeleteAsset(OutPath);

        AssetDatabase.CreateAsset(fontAsset, OutPath);

        // Sub-assets do atlas (Texture2D + Material) precisam ser embutidos no .asset
        // pra persistir. CreateFontAsset gera essas refs internas.
        if (fontAsset.atlasTextures != null)
        {
            foreach (var tex in fontAsset.atlasTextures)
                if (tex != null && !AssetDatabase.IsSubAsset(tex)) AssetDatabase.AddObjectToAsset(tex, fontAsset);
        }
        if (fontAsset.material != null && !AssetDatabase.IsSubAsset(fontAsset.material))
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);

        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(OutPath);
        Debug.Log($"[PixelFontBuilder] {OutPath} criado com {fontAsset.characterTable.Count} caracteres.");
    }
}
#endif
