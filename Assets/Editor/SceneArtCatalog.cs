#if UNITY_EDITOR
using System.Collections.Generic;
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

    // ----- Woody adulto sentado (sleep loop do prologue) -----
    // Idle_2.png já vem sliced em 11 sub-sprites (Idle_2_0..10) — postura curvada/sentada.
    public const string WoodySittingSheet = "Assets/Sprites/mendigos/Homeless_1/Idle_2.png";

    // ----- Portal cyberpunk (vending machine #5 do cyberpunk-market-street pack) -----
    public const string PortalVendingMachine = "Assets/Sprites/cyberpunk-market-street-pixel-art/3 Objects/Vending machines/5.png";

    // ----- Aliens (free-pixel-art-tiny-hero-sprites: 3 monstros, Idle 4 frames cada) -----
    public const string AlienPinkSheet  = "Assets/Sprites/free-pixel-art-tiny-hero-sprites/1 Pink_Monster/Pink_Monster_Idle_4.png";
    public const string AlienOwletSheet = "Assets/Sprites/free-pixel-art-tiny-hero-sprites/2 Owlet_Monster/Owlet_Monster_Idle_4.png";
    public const string AlienDudeSheet  = "Assets/Sprites/free-pixel-art-tiny-hero-sprites/3 Dude_Monster/Dude_Monster_Idle_4.png";

    // ----- Tile assets gerados em Settings/Tiles -----
    public const string TileAssetGround   = "Assets/Settings/Tiles/Residential_Ground.asset";
    public const string TileAssetGrassTop = "Assets/Settings/Tiles/Residential_GrassTop.asset";
    public const string TileAssetWall     = "Assets/Settings/Tiles/Residential_Wall.asset";
    public const string TileAssetPlatform = "Assets/Settings/Tiles/Residential_Platform.asset";

    // ----- Tilemap (basement-tileset) — usado em Memory_02_Domingo (interior casa) -----
    // Tile_*.png são 32×32 px, PPU 32 → 1 cell = 1u no Grid (mesma escala que residential).
    // Indices escolhidos como ponto de partida (basement tem 70 tiles); ajustar visualmente.
    public const string BasementTileGround   = "Assets/Sprites/basement-tileset-pixel-art/1 Tiles/Tile_01.png";
    public const string BasementTileWall     = "Assets/Sprites/basement-tileset-pixel-art/1 Tiles/Tile_05.png";
    public const string BasementTilePlatform = "Assets/Sprites/basement-tileset-pixel-art/1 Tiles/Tile_09.png";

    // ----- Props basement (3 Objects/{1 Pipe, 2 Box, 3 Decoration}) -----
    public const string BasementBox      = "Assets/Sprites/basement-tileset-pixel-art/3 Objects/2 Box/1.png";
    public const string BasementPipe     = "Assets/Sprites/basement-tileset-pixel-art/3 Objects/1 Pipe/1.png";
    public const string BasementDecor1   = "Assets/Sprites/basement-tileset-pixel-art/3 Objects/3 Decoration/1.png";
    public const string BasementDecor4   = "Assets/Sprites/basement-tileset-pixel-art/3 Objects/3 Decoration/4.png";

    // ----- BG basement (interior fechado, sem parallax) -----
    public const string BasementBG       = "Assets/Sprites/basement-tileset-pixel-art/2 Background/Overlay_illumination.png";

    // ----- Fence quebrável (business-center-tileset/3 Objects/Fence7..10) -----
    // 4 peças 32×32 que empilham verticalmente pra formar a parede destrutível
    // (BuildBreakablePlank). Convenção: Bottom = âncora c/ barra horizontal grossa;
    // MiddleA/B = variantes do meio (decoração); Top = topo limpo.
    public const string FenceBottom  = "Assets/Sprites/business-center-tileset-pixel-art/3 Objects/Fence8.png";
    public const string FenceMiddleA = "Assets/Sprites/business-center-tileset-pixel-art/3 Objects/Fence9.png";
    public const string FenceMiddleB = "Assets/Sprites/business-center-tileset-pixel-art/3 Objects/Fence7.png";
    public const string FenceTop     = "Assets/Sprites/business-center-tileset-pixel-art/3 Objects/Fence10.png";

    // ----- Doors and portals (doors-and-portals-pixel-art-asset-pack) -----
    // Sheet 1.png = porta de madeira, 6 frames horizontais 32×64 px (animação fechada→aberta).
    // Sheet 2.png = portal de pedra/azul, 6 frames 64×64 (idle com brilho/estrelas).
    // PPU 32 → door 1u×2u, portal 2u×2u. Pivot BottomCenter (sentam no chão).
    public const string DoorWoodenSheet  = "Assets/Sprites/doors-and-portals-pixel-art-asset-pack/1 Doors/1.png";
    public const string PortalStoneSheet = "Assets/Sprites/doors-and-portals-pixel-art-asset-pack/2 Portals/2.png";

    // ----- Tile assets gerados pro basement -----
    public const string TileAssetBasementGround   = "Assets/Settings/Tiles/Basement_Ground.asset";
    public const string TileAssetBasementWall     = "Assets/Settings/Tiles/Basement_Wall.asset";
    public const string TileAssetBasementPlatform = "Assets/Settings/Tiles/Basement_Platform.asset";

    // ----- Tilemap (green-zone) — usado em Memory_03_Floresta (cenário externo, floresta) -----
    // Tile_*.png são 32×32 px, PPU 32 (igual residential/basement). Índices iniciais
    // batem com os usados em Memory_01 — ajustar visualmente se o pack tiver layout
    // diferente. Pack tem 96 tiles no total.
    // Tile_03 = espigas de grama verdes limpas (sem flores vermelhas). Tile_30 = brick roxo
    // estilizado (pedra/rocha). Tile_04 = sólido roxo-escuro (terra profunda). Inspeção
    // visual confirmou que o pack tem paleta roxa-escura stylizada, não dirt marrom natural.
    public const string GreenZoneTileGround   = "Assets/Sprites/green-zone-tileset-pixel-art/1 Tiles/Tile_04.png"; // dirt sólido profundo
    public const string GreenZoneTileGrassTop = "Assets/Sprites/green-zone-tileset-pixel-art/1 Tiles/Tile_03.png"; // grama verde clean
    public const string GreenZoneTileStone    = "Assets/Sprites/green-zone-tileset-pixel-art/1 Tiles/Tile_30.png"; // pedra brick roxa
    public const string GreenZoneTileWall     = "Assets/Sprites/green-zone-tileset-pixel-art/1 Tiles/Tile_05.png";
    public const string GreenZoneTilePlatform = "Assets/Sprites/green-zone-tileset-pixel-art/1 Tiles/Tile_15.png";

    // ----- Props green-zone (3 Objects/{Bushes, Stones, Fence, Other (trees/etc.)}) -----
    public const string GreenZoneTree1   = "Assets/Sprites/green-zone-tileset-pixel-art/3 Objects/Other/Tree1.png";
    public const string GreenZoneTree2   = "Assets/Sprites/green-zone-tileset-pixel-art/3 Objects/Other/Tree2.png";
    public const string GreenZoneTree3   = "Assets/Sprites/green-zone-tileset-pixel-art/3 Objects/Other/Tree3.png";
    public const string GreenZoneTree4   = "Assets/Sprites/green-zone-tileset-pixel-art/3 Objects/Other/Tree4.png";
    public const string GreenZoneBush1   = "Assets/Sprites/green-zone-tileset-pixel-art/3 Objects/Bushes/1.png";
    public const string GreenZoneBush2   = "Assets/Sprites/green-zone-tileset-pixel-art/3 Objects/Bushes/2.png";
    public const string GreenZoneBush3   = "Assets/Sprites/green-zone-tileset-pixel-art/3 Objects/Bushes/3.png";
    public const string GreenZoneStone1  = "Assets/Sprites/green-zone-tileset-pixel-art/3 Objects/Stones/1.png";
    public const string GreenZoneStone2  = "Assets/Sprites/green-zone-tileset-pixel-art/3 Objects/Stones/2.png";
    public const string GreenZoneStone3  = "Assets/Sprites/green-zone-tileset-pixel-art/3 Objects/Stones/3.png";
    public const string GreenZoneFence   = "Assets/Sprites/green-zone-tileset-pixel-art/3 Objects/Fence/1.png";

    // ----- BG layers green-zone (2 Background/Day, 5 layers parallax) -----
    // Index 0 = mais distante (céu), 4 = mais próximo (foreground/chão).
    public static readonly string[] GreenZoneBgDayLayers = new[]
    {
        "Assets/Sprites/green-zone-tileset-pixel-art/2 Background/Day/1.png",
        "Assets/Sprites/green-zone-tileset-pixel-art/2 Background/Day/2.png",
        "Assets/Sprites/green-zone-tileset-pixel-art/2 Background/Day/3.png",
        "Assets/Sprites/green-zone-tileset-pixel-art/2 Background/Day/4.png",
        "Assets/Sprites/green-zone-tileset-pixel-art/2 Background/Day/5.png",
    };

    // ----- Tile assets gerados pro green-zone -----
    public const string TileAssetGreenZoneGround   = "Assets/Settings/Tiles/GreenZone_Ground.asset";
    public const string TileAssetGreenZoneGrassTop = "Assets/Settings/Tiles/GreenZone_GrassTop.asset";
    public const string TileAssetGreenZoneWall     = "Assets/Settings/Tiles/GreenZone_Wall.asset";
    public const string TileAssetGreenZonePlatform = "Assets/Settings/Tiles/GreenZone_Platform.asset";

    // ----- Tilemap cyberpunk-market (Memory_04_Sala, mercado cyberpunk noturno) -----
    // Tile_*.png são 32×32 px, PPU 32 → 1 cell = 1u no Grid. Pack tem 100 tiles;
    // índices iniciais escolhidos por inspeção visual (asfalto/calçada). Ajustar
    // visualmente abrindo o spritesheet se necessário.
    // Tile_01 tem coluna esquerda quase toda transparente (tile de canto) — quando
    // Tiled repete horizontalmente, a faixa transparente vira gap entre cópias. Use
    // Tile_02 (sólido edge-to-edge, com lip de topo) pro chão.
    public const string MarketTileGround   = "Assets/Sprites/cyberpunk-market-street-pixel-art/1 Tiles/Tile_02.png";
    public const string MarketTileGrassTop = "Assets/Sprites/cyberpunk-market-street-pixel-art/1 Tiles/Tile_02.png";
    public const string MarketTileWall     = "Assets/Sprites/cyberpunk-market-street-pixel-art/1 Tiles/Tile_05.png";
    public const string MarketTilePlatform = "Assets/Sprites/cyberpunk-market-street-pixel-art/1 Tiles/Tile_15.png";

    // ----- Props cyberpunk-market (3 Objects) -----
    public const string MarketLamp1      = "Assets/Sprites/cyberpunk-market-street-pixel-art/3 Objects/Lamps/1.png";
    public const string MarketLamp2      = "Assets/Sprites/cyberpunk-market-street-pixel-art/3 Objects/Lamps/2.png";
    public const string MarketLamp3      = "Assets/Sprites/cyberpunk-market-street-pixel-art/3 Objects/Lamps/3.png";
    public const string MarketSign1      = "Assets/Sprites/cyberpunk-market-street-pixel-art/3 Objects/Signboards/1.png";
    public const string MarketSign4      = "Assets/Sprites/cyberpunk-market-street-pixel-art/3 Objects/Signboards/4.png";
    public const string MarketSign7      = "Assets/Sprites/cyberpunk-market-street-pixel-art/3 Objects/Signboards/7.png";
    public const string MarketTerrace1   = "Assets/Sprites/cyberpunk-market-street-pixel-art/3 Objects/Terrace/1.png";
    public const string MarketTerrace3   = "Assets/Sprites/cyberpunk-market-street-pixel-art/3 Objects/Terrace/3.png";
    public const string MarketVending1   = "Assets/Sprites/cyberpunk-market-street-pixel-art/3 Objects/Vending machines/1.png";
    public const string MarketVending2   = "Assets/Sprites/cyberpunk-market-street-pixel-art/3 Objects/Vending machines/2.png";
    public const string MarketVending3   = "Assets/Sprites/cyberpunk-market-street-pixel-art/3 Objects/Vending machines/3.png";
    public const string MarketVending4   = "Assets/Sprites/cyberpunk-market-street-pixel-art/3 Objects/Vending machines/4.png";
    public const string MarketVending6   = "Assets/Sprites/cyberpunk-market-street-pixel-art/3 Objects/Vending machines/6.png";
    public const string MarketVending7   = "Assets/Sprites/cyberpunk-market-street-pixel-art/3 Objects/Vending machines/7.png";

    // ----- Vending machines do business-center pack (3 Objects/Vending_machine1..7) -----
    // Substituem MarketVending* nas cabines de fliperama de M04. Sprites são pixel art
    // de máquinas mais "arcade-looking" (CRT, neons). Pivot Center (regra existente do
    // pack pra fences) — BuildArcadeCabin/AddProp compensam via pivot-aware offset.
    public const string BusinessVending1 = "Assets/Sprites/business-center-tileset-pixel-art/3 Objects/Vending_machine1.png";
    public const string BusinessVending2 = "Assets/Sprites/business-center-tileset-pixel-art/3 Objects/Vending_machine2.png";
    public const string BusinessVending3 = "Assets/Sprites/business-center-tileset-pixel-art/3 Objects/Vending_machine3.png";
    public const string BusinessVending4 = "Assets/Sprites/business-center-tileset-pixel-art/3 Objects/Vending_machine4.png";
    public const string BusinessVending5 = "Assets/Sprites/business-center-tileset-pixel-art/3 Objects/Vending_machine5.png";
    public const string BusinessVending6 = "Assets/Sprites/business-center-tileset-pixel-art/3 Objects/Vending_machine6.png";
    public const string BusinessVending7 = "Assets/Sprites/business-center-tileset-pixel-art/3 Objects/Vending_machine7.png";

    // ----- BG cyberpunk-market (2 Background/Night, 5 layers parallax) -----
    public static readonly string[] MarketBgNightLayers = new[]
    {
        "Assets/Sprites/cyberpunk-market-street-pixel-art/2 Background/Night/1.png",
        "Assets/Sprites/cyberpunk-market-street-pixel-art/2 Background/Night/2.png",
        "Assets/Sprites/cyberpunk-market-street-pixel-art/2 Background/Night/3.png",
        "Assets/Sprites/cyberpunk-market-street-pixel-art/2 Background/Night/4.png",
        "Assets/Sprites/cyberpunk-market-street-pixel-art/2 Background/Night/5.png",
    };

    // ----- Tile assets gerados pro market -----
    public const string TileAssetMarketGround   = "Assets/Settings/Tiles/Market_Ground.asset";
    public const string TileAssetMarketGrassTop = "Assets/Settings/Tiles/Market_GrassTop.asset";
    public const string TileAssetMarketWall     = "Assets/Settings/Tiles/Market_Wall.asset";
    public const string TileAssetMarketPlatform = "Assets/Settings/Tiles/Market_Platform.asset";

    // ----- Food / snack icons (32×32) — comidas pra FoodPickup e Snake -----
    // Icon3_NN.png (street-food) e Icon5_NN.png (street-snacks). 15 icons cada pack.
    // PPU 32 → 1u no mundo. Pivot Center.
    public const string FoodIconBurger  = "Assets/Sprites/street-food-for-cyberpunk-pixel-art-32x32-icons/1 Icons/Icon3_01.png";
    public const string FoodIconNoodle  = "Assets/Sprites/street-food-for-cyberpunk-pixel-art-32x32-icons/1 Icons/Icon3_02.png";
    public const string FoodIconSushi   = "Assets/Sprites/street-food-for-cyberpunk-pixel-art-32x32-icons/1 Icons/Icon3_05.png";
    public const string FoodIconDrink   = "Assets/Sprites/street-food-for-cyberpunk-pixel-art-32x32-icons/1 Icons/Icon3_07.png";
    public const string FoodIconDessert = "Assets/Sprites/street-snacks-pixel-art-32x32-icon-pack/1 Icons/Icon5_03.png";

    // Lista completa pra Snake escolher sprite random pra cada comida.
    public static readonly string[] FoodSnakePool = new[]
    {
        "Assets/Sprites/street-food-for-cyberpunk-pixel-art-32x32-icons/1 Icons/Icon3_01.png",
        "Assets/Sprites/street-food-for-cyberpunk-pixel-art-32x32-icons/1 Icons/Icon3_02.png",
        "Assets/Sprites/street-food-for-cyberpunk-pixel-art-32x32-icons/1 Icons/Icon3_03.png",
        "Assets/Sprites/street-food-for-cyberpunk-pixel-art-32x32-icons/1 Icons/Icon3_05.png",
        "Assets/Sprites/street-food-for-cyberpunk-pixel-art-32x32-icons/1 Icons/Icon3_07.png",
        "Assets/Sprites/street-food-for-cyberpunk-pixel-art-32x32-icons/1 Icons/Icon3_09.png",
        "Assets/Sprites/street-snacks-pixel-art-32x32-icon-pack/1 Icons/Icon5_01.png",
        "Assets/Sprites/street-snacks-pixel-art-32x32-icon-pack/1 Icons/Icon5_03.png",
        "Assets/Sprites/street-snacks-pixel-art-32x32-icon-pack/1 Icons/Icon5_05.png",
        "Assets/Sprites/street-snacks-pixel-art-32x32-icon-pack/1 Icons/Icon5_08.png",
    };

    // ----- NPC guitarrista (cyberpunk-pixel-bar-cafe-npc-asset-pack/6) -----
    // PlayGuitar.png é sheet 384×64 (6 frames horizontais) — sliciado pelo
    // SpriteImportConfigurator e carregado via LoadSpriteFrames pra animar o NPC
    // da fachada da cabine GH como SimpleSpriteAnimator. Idle.png (256×64, 4 frames)
    // pode ser usado quando o NPC parar de tocar; atualmente usamos só PlayGuitar.
    public const string MarketGuitarNpcPlay = "Assets/Sprites/cyberpunk-pixel-bar-cafe-npc-asset-pack/6/PlayGuitar.png";
    public const string MarketGuitarNpcIdle = "Assets/Sprites/cyberpunk-pixel-bar-cafe-npc-asset-pack/6/Idle.png";

    // ----- Foodtruck (business-center) e Money (coin do P1 da M04) -----
    public const string MarketFoodtruck = "Assets/Sprites/business-center-tileset-pixel-art/3 Objects/Foodtruck1.png";
    public const string MarketCoin      = "Assets/Sprites/business-center-tileset-pixel-art/3 Objects/Money.png";

    // ----- Fontes pixel (TMP SDF) -----
    // Body = VT323 (default UI/diálogos — mais legível em corpo pequeno).
    // Title = Press Start 2P (usado SÓ no logo "RETROSELF" — estética 8-bit).
    public const string PixelFontPath = "Assets/Fonts/VT323-SDF.asset";
    public const string TitleFontPath = "Assets/Fonts/PressStart2P-SDF.asset";

    // Carrega só os sub-sprites (slices) de um spritesheet em Multiple mode,
    // ordenados pelo índice numérico no sufixo "_N". Filtra fora a Texture2D
    // principal. Espelha a lógica que o Memory01Builder já usa pros personagens.
    public static Sprite[] LoadSpriteFrames(string sheetPath, string namePrefix = null)
    {
        var subs = AssetDatabase.LoadAllAssetRepresentationsAtPath(sheetPath);
        string prefix = namePrefix ?? (System.IO.Path.GetFileNameWithoutExtension(sheetPath) + "_");
        var list = new List<Sprite>();
        foreach (var obj in subs)
        {
            if (obj is Sprite s && s.name.StartsWith(prefix))
                list.Add(s);
        }
        list.Sort((a, b) => SpriteIndex(a.name).CompareTo(SpriteIndex(b.name)));
        if (list.Count == 0)
            Debug.LogWarning($"[SceneArtCatalog] Nenhum sprite com prefixo '{prefix}' em {sheetPath} — rode Retroself → Configure Character Sprites?");
        return list.ToArray();
    }

    static int SpriteIndex(string spriteName)
    {
        int u = spriteName.LastIndexOf('_');
        if (u < 0 || u + 1 >= spriteName.Length) return 0;
        return int.TryParse(spriteName.Substring(u + 1), out var i) ? i : 0;
    }

    public static Sprite LoadSprite(string path)
    {
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (s == null) Debug.LogWarning($"[SceneArtCatalog] Sprite não encontrado: {path} — rode Retroself → Configure Scene Art Imports?");
        return s;
    }

    static TMP_FontAsset _cachedBodyFont;
    static bool _bodyFontLookupAttempted;
    public static TMP_FontAsset GetPixelFont()
    {
        if (_cachedBodyFont != null) return _cachedBodyFont;
        if (_bodyFontLookupAttempted) return null;
        _bodyFontLookupAttempted = true;

        _cachedBodyFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(PixelFontPath);
        if (_cachedBodyFont == null)
            Debug.LogWarning($"[SceneArtCatalog] Pixel font não encontrada em {PixelFontPath} — rode Retroself → Build Pixel Font Asset. Texto vai ficar com fonte default.");
        return _cachedBodyFont;
    }

    static TMP_FontAsset _cachedTitleFont;
    static bool _titleFontLookupAttempted;
    public static TMP_FontAsset GetTitleFont()
    {
        if (_cachedTitleFont != null) return _cachedTitleFont;
        if (_titleFontLookupAttempted) return null;
        _titleFontLookupAttempted = true;

        _cachedTitleFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TitleFontPath);
        if (_cachedTitleFont == null)
            Debug.LogWarning($"[SceneArtCatalog] Title font não encontrada em {TitleFontPath} — rode Retroself → Build Pixel Font Asset.");
        return _cachedTitleFont;
    }
}
#endif
