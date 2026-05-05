#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Aplica TextureImporter settings de pixel art e fatia os spritesheets dos
// 3 personagens (Child_3, Homeless_1, Gangsters_2) por contagem horizontal de
// frames. Idempotente — re-rodar reescreve as mesmas configs e o mesmo slice.
public static class SpriteImportConfigurator
{
    // Pixels-per-unit por personagem. PPU menor = sprite renderiza maior.
    // Adulto e porteiro com PPU bem mais baixo que a criança pra estabelecer
    // hierarquia de altura e ficar visível no mundo (a frame inteira tem muito
    // padding em volta do char, então PPU 64 ficava todo mundo do mesmo tamanho).
    const int PPU_Crianca = 64;   // kid pequeno
    const int PPU_Adulto = 28;    // adulto ~2× kid
    const int PPU_Porteiro = 32;  // porteiro intimidador

    // Pra cada PNG: (frame count, PPU).
    static readonly Dictionary<string, (int frames, int ppu)> Sheets = new Dictionary<string, (int, int)>
    {
        // Woody criança
        { "Assets/Sprites/criancas/Child_3/Idle.png", (6, PPU_Crianca) },
        { "Assets/Sprites/criancas/Child_3/Walk.png", (10, PPU_Crianca) },

        // Woody adulto
        { "Assets/Sprites/mendigos/Homeless_1/Idle.png", (6, PPU_Adulto) },
        { "Assets/Sprites/mendigos/Homeless_1/Walk.png", (10, PPU_Adulto) },
        { "Assets/Sprites/mendigos/Homeless_1/Jump.png", (1, PPU_Adulto) },

        // Porteiro (= bully reframed)
        { "Assets/Sprites/gangsters/Gangsters_2/Idle.png", (7, PPU_Porteiro) },
        { "Assets/Sprites/gangsters/Gangsters_2/Walk.png", (10, PPU_Porteiro) },
        { "Assets/Sprites/gangsters/Gangsters_2/Jump.png", (1, PPU_Porteiro) },
    };

    [MenuItem("Retroself/Configure Character Sprites")]
    public static void Configure()
    {
        int ok = 0, miss = 0;
        foreach (var kv in Sheets)
        {
            if (ConfigureSheet(kv.Key, kv.Value.frames, kv.Value.ppu)) ok++; else miss++;
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[SpriteImportConfigurator] {ok} sheets configurados, {miss} ausentes (PPUs por pasta: kid={PPU_Crianca}, adulto={PPU_Adulto}, porteiro={PPU_Porteiro}).");
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
        // fila e LoadAllAssetsAtPath devolve ainda o sprite whole-texture (efeito
        // "todos os frames juntos como um sprite gigante" no PlayerAdult).
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        return true;
    }
}
#endif
