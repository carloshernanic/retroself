#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;

// Paths centralizados dos packs de pixel art usados nas 3 cenas. Trocar de pack
// (ex: residential → ghetto) ou de tile = editar constante aqui sem caçar pelos
// builders. Os builders consomem essas constantes via AssetDatabase.LoadAssetAtPath.
public static class SceneArtCatalog
{
    // ----- Tilemap (residential-area) -----
    // Tile_*.png são 32×32 px, PPU 32 → 1 cell = 1u no Grid.
    // Índices escolhidos como ponto de partida; ajustar visualmente abrindo o PNG.
    public const string TileGround   = "Assets/Sprites/residential-area-tileset-pixel-art/1 Tiles/Tile_01.png";
    public const string TileGrassTop = "Assets/Sprites/residential-area-tileset-pixel-art/1 Tiles/Tile_02.png";
    public const string TileWall     = "Assets/Sprites/residential-area-tileset-pixel-art/1 Tiles/Tile_05.png";
    public const string TilePlatform = "Assets/Sprites/residential-area-tileset-pixel-art/1 Tiles/Tile_15.png";

    // ----- Props (residential-area / 3 Objects) -----
    public const string PropBench    = "Assets/Sprites/residential-area-tileset-pixel-art/3 Objects/Bench.png";
    public const string PropLamp     = "Assets/Sprites/residential-area-tileset-pixel-art/3 Objects/Lamp_post.png";
    public const string PropTrashcan = "Assets/Sprites/residential-area-tileset-pixel-art/3 Objects/Trashcan.png";
    public const string PropBox      = "Assets/Sprites/residential-area-tileset-pixel-art/3 Objects/Boxes/1.png";

    // ----- City parallax (Pack 1 Night, 5 layers, 576×324 cada) -----
    // Index 0 = mais distante (sky/skyline), 4 = mais próximo (chão/foreground).
    public static readonly string[] CityNightLayers = new[]
    {
        "Assets/Sprites/free-scrolling-city-backgrounds-pixel-art/1 Backgrounds/1/Night/1.png",
        "Assets/Sprites/free-scrolling-city-backgrounds-pixel-art/1 Backgrounds/1/Night/2.png",
        "Assets/Sprites/free-scrolling-city-backgrounds-pixel-art/1 Backgrounds/1/Night/3.png",
        "Assets/Sprites/free-scrolling-city-backgrounds-pixel-art/1 Backgrounds/1/Night/4.png",
        "Assets/Sprites/free-scrolling-city-backgrounds-pixel-art/1 Backgrounds/1/Night/5.png",
    };

    // ----- Tile assets gerados em Settings/Tiles -----
    public const string TileAssetGround   = "Assets/Settings/Tiles/Residential_Ground.asset";
    public const string TileAssetGrassTop = "Assets/Settings/Tiles/Residential_GrassTop.asset";
    public const string TileAssetWall     = "Assets/Settings/Tiles/Residential_Wall.asset";
    public const string TileAssetPlatform = "Assets/Settings/Tiles/Residential_Platform.asset";

    // ----- Fonte pixel (TMP SDF gerado a partir de Press Start 2P) -----
    public const string PixelFontPath = "Assets/Fonts/PressStart2P-SDF.asset";

    public static Sprite LoadSprite(string path)
    {
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (s == null) Debug.LogWarning($"[SceneArtCatalog] Sprite não encontrado: {path} — rode Retroself → Configure Scene Art Imports?");
        return s;
    }

    static TMP_FontAsset _cachedFont;
    static bool _fontLookupAttempted;
    public static TMP_FontAsset GetPixelFont()
    {
        if (_cachedFont != null) return _cachedFont;
        if (_fontLookupAttempted) return null;
        _fontLookupAttempted = true;

        _cachedFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(PixelFontPath);
        if (_cachedFont == null)
            Debug.LogWarning($"[SceneArtCatalog] Pixel font não encontrada em {PixelFontPath} — rode Retroself → Build Pixel Font Asset (precisa Assets/Fonts/PressStart2P-Regular.ttf primeiro). Texto vai ficar com fonte default.");
        return _cachedFont;
    }
}
#endif
