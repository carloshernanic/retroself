#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// Aplica TextureImporter settings de pixel art (FilterMode.Point, no compression,
// PPU específico) APENAS nos packs de cenário. Pula as pastas dos personagens
// (Child_3, Homeless_1, Gangsters_2) para preservar o slicing manual feito
// no Sprite Editor — ver lição em CLAUDE.md.
//
// Idempotente: re-rodar reescreve as mesmas settings.
public static class SceneArtImportConfigurator
{
    // Pastas de personagens — NUNCA tocar nelas (slicing manual frágil).
    static readonly string[] SkipPrefixes = new[]
    {
        "Assets/Sprites/criancas/",
        "Assets/Sprites/mendigos/",
        "Assets/Sprites/gangsters/",
    };

    // Regras por pasta. PPU define escala no mundo (PPU 32 = 1 tile 32×32 → 1u).
    // pivotAlignment 9 = Center, 7 = BottomCenter (SpriteAlignment enum).
    struct Rule
    {
        public string folder;
        public int ppu;
        public int alignment;
        public bool recursive;
    }

    static readonly Rule[] Rules = new[]
    {
        // Tilemap residential — 32×32 px → PPU 32, pivot Center pro Tile cair certo na célula.
        new Rule { folder = "Assets/Sprites/residential-area-tileset-pixel-art/1 Tiles", ppu = 32, alignment = (int)SpriteAlignment.Center, recursive = false },

        // Props residential — 32 PPU também, pivot BottomCenter pra sentarem no chão.
        new Rule { folder = "Assets/Sprites/residential-area-tileset-pixel-art/3 Objects", ppu = 32, alignment = (int)SpriteAlignment.BottomCenter, recursive = true },

        // BG residential local (Day/Night, peças soltas) — PPU 100, pivot Center.
        new Rule { folder = "Assets/Sprites/residential-area-tileset-pixel-art/2 Background", ppu = 100, alignment = (int)SpriteAlignment.Center, recursive = true },

        // City parallax (576×324 cada layer). PPU 32 → 18u largura × 10.125u altura,
        // cobre exatamente a câmera ortho size 5 a 16:9 (17.78×10u). PPU 50 era estreito
        // demais e deixava faixas pretas nas laterais — visível no MainMenu/Prologue.
        new Rule { folder = "Assets/Sprites/free-scrolling-city-backgrounds-pixel-art/1 Backgrounds", ppu = 32, alignment = (int)SpriteAlignment.Center, recursive = true },

        // Ghetto Boxes (caso a gente reuse no futuro) — PPU 32.
        new Rule { folder = "Assets/Sprites/ghetto-tileset-pixel-art/3 Objects", ppu = 32, alignment = (int)SpriteAlignment.BottomCenter, recursive = false },
    };

    [MenuItem("Retroself/Configure Scene Art Imports")]
    public static void Configure()
    {
        int totalTouched = 0, totalSkipped = 0;
        foreach (var rule in Rules)
        {
            if (!AssetDatabase.IsValidFolder(rule.folder))
            {
                Debug.LogWarning($"[SceneArtImportConfigurator] pasta não existe: {rule.folder}");
                continue;
            }

            int touched = 0;
            var pngs = CollectPngs(rule.folder, rule.recursive);
            foreach (var path in pngs)
            {
                if (ShouldSkip(path)) { totalSkipped++; continue; }
                if (ApplyRule(path, rule)) touched++;
            }
            Debug.Log($"[SceneArtImportConfigurator] {rule.folder}: {touched}/{pngs.Count} PNGs configurados (PPU {rule.ppu}, align {rule.alignment}).");
            totalTouched += touched;
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[SceneArtImportConfigurator] Total: {totalTouched} PNGs configurados, {totalSkipped} skipped (personagens).");
    }

    static bool ShouldSkip(string path)
    {
        foreach (var prefix in SkipPrefixes)
            if (path.StartsWith(prefix)) return true;
        return false;
    }

    static List<string> CollectPngs(string folder, bool recursive)
    {
        var list = new List<string>();
        var opt = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        // GetFiles devolve paths absolutos; converter pra relativo "Assets/...".
        var abs = Directory.GetFiles(folder, "*.png", opt);
        foreach (var p in abs)
        {
            var rel = p.Replace('\\', '/');
            int idx = rel.IndexOf("/Assets/");
            if (idx >= 0) rel = rel.Substring(idx + 1);
            list.Add(rel);
        }
        return list;
    }

    static bool ApplyRule(string path, Rule rule)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return false;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = rule.ppu;

        // spriteAlignment/spritePivot saíram do TextureImporter direto no Unity 6;
        // o caminho oficial agora é via TextureImporterSettings.
        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteAlignment = rule.alignment;
        settings.spritePivot = AlignmentToPivot(rule.alignment);
        importer.SetTextureSettings(settings);

        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.sRGBTexture = true;
        importer.alphaIsTransparency = true;
        importer.wrapMode = TextureWrapMode.Clamp;

        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();
        return true;
    }

    static Vector2 AlignmentToPivot(int alignment)
    {
        switch ((SpriteAlignment)alignment)
        {
            case SpriteAlignment.Center:       return new Vector2(0.5f, 0.5f);
            case SpriteAlignment.BottomCenter: return new Vector2(0.5f, 0f);
            case SpriteAlignment.BottomLeft:   return new Vector2(0f, 0f);
            case SpriteAlignment.TopCenter:    return new Vector2(0.5f, 1f);
            default:                           return new Vector2(0.5f, 0.5f);
        }
    }
}
#endif
