#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

// Gera os TMP_FontAssets SDF do projeto:
// - Press Start 2P → usado SÓ no logo "RETROSELF" (estética 8-bit forte).
// - VT323         → fonte default de UI/diálogos (mais legível em corpo pequeno).
// Ambos cobrem ASCII + acentuações pt-BR (áéíóúâêôãõçÁÉÍÓÚÂÊÔÃÕÇ etc).
//
// Uso: rodar 'Retroself → Build Pixel Font Asset' uma vez. Re-rodar reescreve
// os assets existentes.
public static class PixelFontBuilder
{
    const string TitleTtfPath = "Assets/Fonts/PressStart2P-Regular.ttf";
    const string TitleOutPath = "Assets/Fonts/PressStart2P-SDF.asset";

    const string BodyTtfPath = "Assets/TextMesh Pro/Fonts/VT323/VT323-Regular.ttf";
    const string BodyOutPath = "Assets/Fonts/VT323-SDF.asset";

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
        BuildOne(TitleTtfPath, TitleOutPath, "PressStart2P-SDF");
        BuildOne(BodyTtfPath, BodyOutPath, "VT323-SDF");
    }

    static void BuildOne(string ttfPath, string outPath, string assetName)
    {
        var ttf = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
        if (ttf == null)
        {
            Debug.LogError($"[PixelFontBuilder] Não achei o TTF em {ttfPath}.");
            return;
        }

        // Atlas 512×512 SDF padding 5 — suficiente pros charsets pixel-font.
        const int atlasSize = 512;
        const int padding = 5;
        const int samplingPointSize = 64;

        var fontAsset = TMP_FontAsset.CreateFontAsset(
            ttf,
            samplingPointSize,
            padding,
            GlyphRenderMode.SDFAA,
            atlasSize, atlasSize,
            AtlasPopulationMode.Dynamic);

        if (fontAsset == null)
        {
            Debug.LogError($"[PixelFontBuilder] CreateFontAsset retornou null pra {ttfPath}.");
            return;
        }

        fontAsset.name = assetName;

        fontAsset.TryAddCharacters(Charset, out string missing);
        if (!string.IsNullOrEmpty(missing))
            Debug.LogWarning($"[PixelFontBuilder] {assetName} não suporta: '{missing}' — vão renderizar como □.");

        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(outPath);
        if (existing != null) AssetDatabase.DeleteAsset(outPath);

        AssetDatabase.CreateAsset(fontAsset, outPath);

        if (fontAsset.atlasTextures != null)
        {
            foreach (var tex in fontAsset.atlasTextures)
                if (tex != null && !AssetDatabase.IsSubAsset(tex)) AssetDatabase.AddObjectToAsset(tex, fontAsset);
        }
        if (fontAsset.material != null && !AssetDatabase.IsSubAsset(fontAsset.material))
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);

        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(outPath);
        Debug.Log($"[PixelFontBuilder] {outPath} criado com {fontAsset.characterTable.Count} caracteres.");
    }
}
#endif
