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

        // Cyberpunk vending machines (usado como "portal" no Panel_03_Cabine do Prologue).
        // Restringido só à pasta Vending machines/ — outras subpastas de 3 Objects/ (ARMs,
        // Boxes, etc.) ficam de fora pra não tocarmos no que não estamos usando.
        new Rule { folder = "Assets/Sprites/cyberpunk-market-street-pixel-art/3 Objects/Vending machines", ppu = 32, alignment = (int)SpriteAlignment.BottomCenter, recursive = false },

        // Basement tileset (Memory_02_Domingo, interior casa). Tiles 32×32 → PPU 32.
        new Rule { folder = "Assets/Sprites/basement-tileset-pixel-art/1 Tiles", ppu = 32, alignment = (int)SpriteAlignment.Center, recursive = false },

        // Basement props (1 Pipe, 2 Box, 3 Decoration, 4 Illumination). Pivot BottomCenter.
        new Rule { folder = "Assets/Sprites/basement-tileset-pixel-art/3 Objects", ppu = 32, alignment = (int)SpriteAlignment.BottomCenter, recursive = true },

        // Basement BG (interior). PPU 100 pra peças soltas — usado como overlay decorativo.
        new Rule { folder = "Assets/Sprites/basement-tileset-pixel-art/2 Background", ppu = 100, alignment = (int)SpriteAlignment.Center, recursive = true },

        // Green-zone tileset (Memory_03_Floresta, cenário de floresta). Tiles 32×32 → PPU 32.
        new Rule { folder = "Assets/Sprites/green-zone-tileset-pixel-art/1 Tiles", ppu = 32, alignment = (int)SpriteAlignment.Center, recursive = false },

        // Green-zone props (árvores, bushes, fence, stones, fountain, etc.). Pivot BottomCenter pra sentarem no chão.
        new Rule { folder = "Assets/Sprites/green-zone-tileset-pixel-art/3 Objects", ppu = 32, alignment = (int)SpriteAlignment.BottomCenter, recursive = true },

        // Green-zone BG layers (Day/Night, 5 layers cada, 576×324). PPU 32 cobre exatamente ortho size 5 em 16:9.
        new Rule { folder = "Assets/Sprites/green-zone-tileset-pixel-art/2 Background", ppu = 32, alignment = (int)SpriteAlignment.Center, recursive = true },

        // Cyberpunk skills icons (1..13 cores, 14 glitch). PNG 32×32 com 5px de borda
        // transparente → conteúdo 22×22. Single mode + PPU 32 → 1u no mundo, mesma
        // métrica do quadrado do StoneSwitch (scale 1×1). Filtra Point pra pixel art.
        new Rule { folder = "Assets/Sprites/cyberpunk-skills-pixelated-icon-pack/1 Icons", ppu = 32, alignment = (int)SpriteAlignment.Center, recursive = true },

        // Business-center fences (3 Objects/Fence1..10) — usados como visual da
        // BreakablePlank em Memory_02/03 empilhando bottom/middle/top. 32×32, PPU 32,
        // pivot Center pra empilhamento ficar alinhado pelo centro.
        new Rule { folder = "Assets/Sprites/business-center-tileset-pixel-art/3 Objects", ppu = 32, alignment = (int)SpriteAlignment.Center, recursive = false },

        // Cyberpunk-market tileset (Memory_04_Sala, mercado). 32×32 px → PPU 32.
        new Rule { folder = "Assets/Sprites/cyberpunk-market-street-pixel-art/1 Tiles", ppu = 32, alignment = (int)SpriteAlignment.Center, recursive = false },

        // Cyberpunk-market BG night (5 layers 576×324) — PPU 32 cobre ortho size 5 em 16:9.
        new Rule { folder = "Assets/Sprites/cyberpunk-market-street-pixel-art/2 Background", ppu = 32, alignment = (int)SpriteAlignment.Center, recursive = true },

        // Cyberpunk-market props (Lamps, Signboards, Terrace) — sentam no chão.
        new Rule { folder = "Assets/Sprites/cyberpunk-market-street-pixel-art/3 Objects/Lamps", ppu = 32, alignment = (int)SpriteAlignment.BottomCenter, recursive = false },
        new Rule { folder = "Assets/Sprites/cyberpunk-market-street-pixel-art/3 Objects/Signboards", ppu = 32, alignment = (int)SpriteAlignment.BottomCenter, recursive = false },
        new Rule { folder = "Assets/Sprites/cyberpunk-market-street-pixel-art/3 Objects/Terrace", ppu = 32, alignment = (int)SpriteAlignment.BottomCenter, recursive = false },

        // Food icon packs (street-food / street-snacks) — 32×32 cada icon. Single,
        // PPU 32, pivot Center pra ficarem em escala 1u no mundo (combina com tilemap).
        new Rule { folder = "Assets/Sprites/street-food-for-cyberpunk-pixel-art-32x32-icons/1 Icons", ppu = 32, alignment = (int)SpriteAlignment.Center, recursive = false },
        new Rule { folder = "Assets/Sprites/street-snacks-pixel-art-32x32-icon-pack/1 Icons", ppu = 32, alignment = (int)SpriteAlignment.Center, recursive = false },

        // NPC guitarrista (pasta 6/) é animado: PlayGuitar.png (384×64 = 6 frames) e
        // Idle.png (256×64 = 4 frames) são sheets multi-frame. O slicing é feito pelo
        // SpriteImportConfigurator (Multiple mode), NÃO aqui — esta rule sobrescreveria
        // pro Single mode. 6.png e StopPlaying.png ficam Single (não usados como anim).
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
        // FullRect é necessário pra SpriteRenderer.drawMode=Tiled funcionar (Tight
        // recorta o mesh nos pixels opacos e Tiled cai em fallback). Memory_03 usa
        // Tiled pra tilear as sprites do green-zone no chão (grama/terra).
        settings.spriteMeshType = SpriteMeshType.FullRect;
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
