#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Aplica TextureImporter settings de pixel art e fatia spritesheets por contagem
// horizontal de frames. **Só usado pra packs novos** que ainda não foram sliced
// manualmente — reescreve `importer.spritesheet` sempre, então rodar isso em
// cima de um sheet já fatiado no Sprite Editor zera o slicing/pivot manual.
//
// Os 3 personagens (Child_3, Homeless_1, Gangsters_2) **NÃO ficam mais aqui** —
// o usuário já sliciou eles à mão e qualquer rerodagem deste menu antes resetava
// o trabalho. Pra adicionar um pack novo, coloca uma entry em `Sheets` e roda
// 1× pelo menu; depois disso, **deletar a entry** pra proteger os slices.
public static class SpriteImportConfigurator
{
    const int PPU_Alien = 32;     // monstros decorativos do prologue
    const int PPU_DoorPortal = 32; // doors-and-portals pack (32×64 / 64×64 frames)

    // Pra cada PNG: (frame count, PPU). Lista deve ficar **vazia** quando não há
    // pack novo pra slicar — assim o menu vira no-op seguro.
    static readonly Dictionary<string, (int frames, int ppu)> Sheets = new Dictionary<string, (int, int)>
    {
        // Aliens decorativos do Prologue (Panel_05/06). Os 3 monstros do tiny-hero pack —
        // só Idle por enquanto. Suffix "_4" no nome do arquivo = 4 frames horizontais.
        // Depois de rodar 1× e os slices ficarem registrados no .meta, **remover** estas
        // 3 linhas pra proteger eventuais ajustes manuais futuros.
        { "Assets/Sprites/free-pixel-art-tiny-hero-sprites/1 Pink_Monster/Pink_Monster_Idle_4.png",  (4, PPU_Alien) },
        { "Assets/Sprites/free-pixel-art-tiny-hero-sprites/2 Owlet_Monster/Owlet_Monster_Idle_4.png", (4, PPU_Alien) },
        { "Assets/Sprites/free-pixel-art-tiny-hero-sprites/3 Dude_Monster/Dude_Monster_Idle_4.png",  (4, PPU_Alien) },

        // Doors and portals pack — porta de madeira 6 frames 32×64, portal de pedra 6 frames 64×64.
        // Pivot BottomCenter (default do ConfigureSheet) → sentam no chão. Após 1ª rodada, remover.
        { "Assets/Sprites/doors-and-portals-pixel-art-asset-pack/1 Doors/1.png",   (6, PPU_DoorPortal) },
        { "Assets/Sprites/doors-and-portals-pixel-art-asset-pack/2 Portals/2.png", (6, PPU_DoorPortal) },

        // NPC guitarrista da cabine GH (Memory_04). PlayGuitar.png = 384×64 → 6 frames 64×64;
        // Idle.png = 256×64 → 4 frames 64×64. PPU 32 (consistente com props do pack). Após 1ª rodada,
        // remover pra preservar slices manuais.
        { "Assets/Sprites/cyberpunk-pixel-bar-cafe-npc-asset-pack/6/PlayGuitar.png", (6, 32) },
        { "Assets/Sprites/cyberpunk-pixel-bar-cafe-npc-asset-pack/6/Idle.png",       (4, 32) },
    };

    [MenuItem("Retroself/Slice New Sprite Sheets")]
    public static void Configure()
    {
        if (Sheets.Count == 0)
        {
            Debug.Log("[SpriteImportConfigurator] Nada pra slicear — dicionário Sheets está vazio (esperado quando não há pack novo).");
            return;
        }

        int ok = 0, miss = 0;
        foreach (var kv in Sheets)
        {
            if (ConfigureSheet(kv.Key, kv.Value.frames, kv.Value.ppu)) ok++; else miss++;
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[SpriteImportConfigurator] {ok} sheets configurados, {miss} ausentes. **Lembrete:** remova as entries em Sheets agora pra evitar resetar slicing manual em rebuilds futuros.");
    }

    static bool ConfigureSheet(string path, int frames, int ppu)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning($"[SpriteImportConfigurator] não achei TextureImporter em {path}");
            return false;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = ppu;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.sRGBTexture = true;
        importer.alphaIsTransparency = true;
        importer.wrapMode = TextureWrapMode.Clamp;

        // Slice horizontal por contagem. Precisa da textura carregada pra ler width/height.
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex == null)
        {
            Debug.LogWarning($"[SpriteImportConfigurator] textura não carregou: {path}");
            return false;
        }

        int frameW = Mathf.Max(1, tex.width / Mathf.Max(1, frames));
        int frameH = tex.height;

        var meta = new SpriteMetaData[frames];
        string baseName = System.IO.Path.GetFileNameWithoutExtension(path);
        for (int i = 0; i < frames; i++)
        {
            meta[i] = new SpriteMetaData
            {
                name = $"{baseName}_{i}",
                rect = new Rect(i * frameW, 0, frameW, frameH),
                alignment = (int)SpriteAlignment.BottomCenter,
                pivot = new Vector2(0.5f, 0f),
            };
        }
        importer.spritesheet = meta;

        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();
        // Força import síncrono — sem isso, o pipeline às vezes deixa o slice em
        // fila e LoadAllAssetsAtPath devolve ainda o sprite whole-texture.
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        return true;
    }
}
#endif
