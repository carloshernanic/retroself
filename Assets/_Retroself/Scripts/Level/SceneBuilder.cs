using Retroself.Audio;
using Retroself.Core;
using Retroself.Mechanics;
using Retroself.Narrative;
using Retroself.Player;
using Retroself.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Retroself.Level
{
    public static class SceneBuilder
    {
        public static int GroundLayer => LayerMask.NameToLayer("Default");

        // ---------- Camera + bootstrap UI ----------
        public static (Camera cam, CameraFollow follow) CreateCamera(Color bg, bool follow = true, Vector2? min = null, Vector2? max = null)
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.625f; // 180/32 -> at 16ppu = 5.625
            cam.backgroundColor = bg;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.nearClipPlane = -100f;
            cam.farClipPlane = 100f;
            go.transform.position = new Vector3(0, 2, -10);
            go.AddComponent<AudioListener>();
            CameraFollow cf = null;
            if (follow)
            {
                cf = go.AddComponent<CameraFollow>();
                if (min.HasValue) cf.minBounds = min.Value;
                if (max.HasValue) cf.maxBounds = max.Value;
            }
            return (cam, cf);
        }

        // ---------- Solid blocks ----------
        public static GameObject CreateSolid(Vector2 center, Vector2 size, Color color, Transform parent = null, string name = "Solid")
        {
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.position = center;
            int w = Mathf.Max(1, Mathf.RoundToInt(size.x * 16f));
            int h = Mathf.Max(1, Mathf.RoundToInt(size.y * 16f));
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSprites.GetRect(w, h, color);
            sr.sortingOrder = 0;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = size;
            return go;
        }

        public static GameObject CreateDeco(Vector2 center, Vector2 size, Color color, Transform parent = null, int order = -2, string name = "Deco")
        {
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.position = center;
            int w = Mathf.Max(1, Mathf.RoundToInt(size.x * 16f));
            int h = Mathf.Max(1, Mathf.RoundToInt(size.y * 16f));
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSprites.GetRect(w, h, color);
            sr.sortingOrder = order;
            return go;
        }

        public static GameObject CreateKillZone(Vector2 center, Vector2 size, Transform parent = null)
        {
            var go = new GameObject("KillZone");
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.position = center;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = size;
            col.isTrigger = true;
            go.AddComponent<KillZone>();
            return go;
        }

        // ---------- Players ----------
        public static WoodyController CreateAdult(Vector2 spawn, Transform parent = null)
        {
            var go = new GameObject("Woody_Adult");
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.position = spawn;

            var visual = new GameObject("Sprite");
            visual.transform.SetParent(go.transform, false);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSprites.GetWoodyAdult();
            sr.sortingOrder = 10;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.freezeRotation = true;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.7f, 1.95f);
            col.offset = new Vector2(0f, 0.95f);

            var groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.SetParent(go.transform, false);
            groundCheck.transform.localPosition = new Vector3(0f, -0.05f, 0f);

            var motor = go.AddComponent<CharacterMotor>();
            motor.moveSpeed = 5.5f;
            motor.jumpVelocity = 11f;
            motor.gravityScale = 3.2f;
            motor.groundMask = LayerMask.GetMask("Default");
            motor.groundCheck = groundCheck.transform;

            var w = go.AddComponent<WoodyController>();
            w.kind = WoodyKind.Adult;
            w.body = sr;
            return w;
        }

        public static WoodyController CreateYoung(Vector2 spawn, Transform parent = null)
        {
            var go = new GameObject("Woody_Young");
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.position = spawn;

            var visual = new GameObject("Sprite");
            visual.transform.SetParent(go.transform, false);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSprites.GetWoodyYoung();
            sr.sortingOrder = 10;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 2.6f;
            rb.freezeRotation = true;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.55f, 0.95f);
            col.offset = new Vector2(0f, 0.5f);

            var groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.SetParent(go.transform, false);
            groundCheck.transform.localPosition = new Vector3(0f, -0.05f, 0f);

            var motor = go.AddComponent<CharacterMotor>();
            motor.moveSpeed = 5.0f;
            motor.jumpVelocity = 9.5f;
            motor.gravityScale = 2.6f;
            motor.groundMask = LayerMask.GetMask("Default");
            motor.groundCheck = groundCheck.transform;

            var w = go.AddComponent<WoodyController>();
            w.kind = WoodyKind.Young;
            w.body = sr;
            return w;
        }

        // ---------- Mechanics ----------
        public static GameObject CreateBox(Vector2 pos, float size = 1f, Transform parent = null)
        {
            var go = new GameObject("PushBox");
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.position = pos;
            int s = Mathf.Max(1, Mathf.RoundToInt(size * 16f));
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSprites.GetRect(s, s, new Color(0.6f, 0.45f, 0.25f));
            sr.sortingOrder = 5;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.freezeRotation = true;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(size, size);
            go.AddComponent<PushableBox>();
            return go;
        }

        public static GameObject CreateCollectible(Vector2 pos, Color color, Transform parent = null, string id = "photo")
        {
            var go = new GameObject("Collectible");
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSprites.GetRect(8, 8, color);
            sr.sortingOrder = 6;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.35f;
            var c = go.AddComponent<Collectible>();
            c.collectibleId = id;
            return go;
        }

        public static GameObject CreatePlate(Vector2 pos, PressurePlate.WeightRequirement req, Transform parent = null)
        {
            var go = new GameObject("Plate");
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSprites.GetRect(24, 4, new Color(0.5f, 0.5f, 0.55f));
            sr.sortingOrder = 1;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.5f, 0.25f);
            col.isTrigger = true;
            var p = go.AddComponent<PressurePlate>();
            p.requirement = req;
            p.plateSprite = sr;
            return go;
        }

        public static GameObject CreateGoal(Vector2 pos, string phaseId, string nextScene, Transform parent = null, bool both = true)
        {
            var go = new GameObject("PhaseGoal");
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSprites.GetRect(24, 32, new Color(1f, 0.85f, 0.4f, 0.55f));
            sr.sortingOrder = 2;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.5f, 2f);
            col.isTrigger = true;
            var g = go.AddComponent<PhaseGoal>();
            g.phaseId = phaseId;
            g.nextScene = nextScene;
            g.requiresBothWoodys = both;
            return go;
        }

        public static GameObject CreateCheckpoint(Vector2 pos, Vector2 adultSpawn, Vector2 youngSpawn, Transform parent = null)
        {
            var go = new GameObject("Checkpoint");
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSprites.GetRect(4, 16, new Color(0.5f, 0.9f, 1f, 0.6f));
            sr.sortingOrder = 1;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.4f, 1f);
            col.isTrigger = true;
            var as_ = new GameObject("AdultSpawn");
            as_.transform.SetParent(go.transform, false);
            as_.transform.position = adultSpawn;
            var ys_ = new GameObject("YoungSpawn");
            ys_.transform.SetParent(go.transform, false);
            ys_.transform.position = youngSpawn;
            var cp = go.AddComponent<Checkpoint>();
            cp.adultSpawn = as_.transform;
            cp.youngSpawn = ys_.transform;
            return go;
        }

        public static GameObject CreatePatrolEnemy(Vector2 pos, float halfRange, float speed, Color color, Transform parent = null)
        {
            var go = new GameObject("Enemy");
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var visual = new GameObject("Sprite");
            visual.transform.SetParent(go.transform, false);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSprites.GetRect(20, 24, color);
            sr.sortingOrder = 6;
            var pe = go.AddComponent<PatrollingEnemy>();
            pe.patrolHalfRange = halfRange;
            pe.speed = speed;
            pe.body = visual.transform;
            return go;
        }

        public static GameObject CreateSoundEmitter(Vector2 pos, Vector2 dir, float interval, Transform parent = null)
        {
            var go = new GameObject("SoundEmitter");
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSprites.GetRect(8, 8, new Color(1f, 0.5f, 0.5f));
            sr.sortingOrder = 1;
            var em = go.AddComponent<SoundWaveEmitter>();
            em.direction = dir;
            em.interval = interval;
            return go;
        }

        // ---------- UI ----------
        public static GameObject CreateUI(out HUD hud, out PauseMenu pause, out SceneFader fader, out DialogueSystem dialogue, bool showHud = true)
        {
            var canvasGo = new GameObject("UI Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // Freeze overlay
            var overlayGo = new GameObject("FreezeOverlay");
            overlayGo.transform.SetParent(canvasGo.transform, false);
            var ov = overlayGo.AddComponent<Image>();
            ov.color = new Color(0.5f, 0.7f, 1f, 0f);
            ov.raycastTarget = false;
            var ovrt = ov.rectTransform;
            ovrt.anchorMin = Vector2.zero; ovrt.anchorMax = Vector2.one;
            ovrt.offsetMin = Vector2.zero; ovrt.offsetMax = Vector2.zero;

            // Energy bar
            GameObject energyRoot = new GameObject("EnergyRoot");
            energyRoot.transform.SetParent(canvasGo.transform, false);
            var erRT = energyRoot.AddComponent<RectTransform>();
            erRT.anchorMin = new Vector2(0f, 1f); erRT.anchorMax = new Vector2(0f, 1f);
            erRT.pivot = new Vector2(0f, 1f);
            erRT.anchoredPosition = new Vector2(20, -20);
            erRT.sizeDelta = new Vector2(220, 30);

            var bgGo = new GameObject("BG"); bgGo.transform.SetParent(energyRoot.transform, false);
            var bgImg = bgGo.AddComponent<Image>(); bgImg.color = new Color(0f, 0f, 0f, 0.5f);
            var bgRT = bgImg.rectTransform; bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one; bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;

            var fillGo = new GameObject("Fill"); fillGo.transform.SetParent(energyRoot.transform, false);
            var fillImg = fillGo.AddComponent<Image>(); fillImg.color = new Color(0.85f, 0.7f, 0.3f);
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillAmount = 1f;
            fillImg.sprite = null;
            var fillRT = fillImg.rectTransform; fillRT.anchorMin = new Vector2(0,0); fillRT.anchorMax = new Vector2(1,1); fillRT.offsetMin = new Vector2(3,3); fillRT.offsetMax = new Vector2(-3,-3);

            var labelGo = new GameObject("Label"); labelGo.transform.SetParent(energyRoot.transform, false);
            var labelTxt = labelGo.AddComponent<Text>(); labelTxt.text = "TEMPO";
            labelTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelTxt.alignment = TextAnchor.MiddleCenter;
            labelTxt.color = Color.white;
            labelTxt.fontSize = 14;
            var labelRT = labelTxt.rectTransform; labelRT.anchorMin = Vector2.zero; labelRT.anchorMax = Vector2.one; labelRT.offsetMin = Vector2.zero; labelRT.offsetMax = Vector2.zero;

            // Character label
            var charGo = new GameObject("CharLabel");
            charGo.transform.SetParent(canvasGo.transform, false);
            var charTxt = charGo.AddComponent<Text>();
            charTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            charTxt.color = new Color(1f, 1f, 1f, 0.7f);
            charTxt.fontSize = 14;
            charTxt.alignment = TextAnchor.UpperRight;
            var crt = charTxt.rectTransform; crt.anchorMin = new Vector2(1,1); crt.anchorMax = new Vector2(1,1); crt.pivot = new Vector2(1,1);
            crt.anchoredPosition = new Vector2(-20, -20); crt.sizeDelta = new Vector2(260, 24);

            // Dialogue
            var dlgGo = new GameObject("DialoguePanel");
            dlgGo.transform.SetParent(canvasGo.transform, false);
            var dlgImg = dlgGo.AddComponent<Image>(); dlgImg.color = new Color(0.05f, 0.05f, 0.1f, 0.9f);
            var dlgRT = dlgImg.rectTransform; dlgRT.anchorMin = new Vector2(0.05f, 0f); dlgRT.anchorMax = new Vector2(0.95f, 0f); dlgRT.pivot = new Vector2(0.5f, 0f);
            dlgRT.anchoredPosition = new Vector2(0, 30); dlgRT.sizeDelta = new Vector2(0, 150);

            var spkGo = new GameObject("Speaker"); spkGo.transform.SetParent(dlgGo.transform, false);
            var spk = spkGo.AddComponent<Text>();
            spk.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            spk.color = new Color(1f, 0.85f, 0.4f);
            spk.fontStyle = FontStyle.Bold;
            spk.fontSize = 18;
            var spkRT = spk.rectTransform; spkRT.anchorMin = new Vector2(0,1); spkRT.anchorMax = new Vector2(1,1); spkRT.pivot = new Vector2(0,1);
            spkRT.anchoredPosition = new Vector2(20, -10); spkRT.sizeDelta = new Vector2(-40, 28);

            var bdyGo = new GameObject("Body"); bdyGo.transform.SetParent(dlgGo.transform, false);
            var bdy = bdyGo.AddComponent<Text>();
            bdy.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            bdy.color = Color.white;
            bdy.fontSize = 16;
            bdy.horizontalOverflow = HorizontalWrapMode.Wrap;
            var bdyRT = bdy.rectTransform; bdyRT.anchorMin = new Vector2(0,0); bdyRT.anchorMax = new Vector2(1,1); bdyRT.pivot = new Vector2(0.5f, 0.5f);
            bdyRT.offsetMin = new Vector2(20, 20); bdyRT.offsetMax = new Vector2(-20, -40);

            var blinkGo = new GameObject("Continue"); blinkGo.transform.SetParent(dlgGo.transform, false);
            var blink = blinkGo.AddComponent<Text>();
            blink.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            blink.text = "▼";
            blink.color = new Color(1f, 1f, 1f, 0f);
            blink.fontSize = 16;
            blink.alignment = TextAnchor.MiddleRight;
            var blnkRT = blink.rectTransform; blnkRT.anchorMin = new Vector2(1,0); blnkRT.anchorMax = new Vector2(1,0); blnkRT.pivot = new Vector2(1,0);
            blnkRT.anchoredPosition = new Vector2(-20, 10); blnkRT.sizeDelta = new Vector2(40, 24);

            // Pause panel
            var pauseGo = new GameObject("PausePanel");
            pauseGo.transform.SetParent(canvasGo.transform, false);
            var pImg = pauseGo.AddComponent<Image>(); pImg.color = new Color(0, 0, 0, 0.8f);
            var pRT = pImg.rectTransform; pRT.anchorMin = Vector2.zero; pRT.anchorMax = Vector2.one; pRT.offsetMin = Vector2.zero; pRT.offsetMax = Vector2.zero;
            var pTxtGo = new GameObject("Title"); pTxtGo.transform.SetParent(pauseGo.transform, false);
            var pTxt = pTxtGo.AddComponent<Text>();
            pTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            pTxt.text = "PAUSADO\n\nESC retoma\nM volta ao Hub";
            pTxt.alignment = TextAnchor.MiddleCenter;
            pTxt.color = Color.white;
            pTxt.fontSize = 28;
            var pTxtRT = pTxt.rectTransform; pTxtRT.anchorMin = Vector2.zero; pTxtRT.anchorMax = Vector2.one; pTxtRT.offsetMin = Vector2.zero; pTxtRT.offsetMax = Vector2.zero;
            pauseGo.SetActive(false);

            // Fade overlay (always on top)
            var fadeGo = new GameObject("FadeOverlay");
            fadeGo.transform.SetParent(canvasGo.transform, false);
            var fadeImg = fadeGo.AddComponent<Image>(); fadeImg.color = new Color(0, 0, 0, 1f);
            fadeImg.raycastTarget = false;
            var fadeRT = fadeImg.rectTransform; fadeRT.anchorMin = Vector2.zero; fadeRT.anchorMax = Vector2.one; fadeRT.offsetMin = Vector2.zero; fadeRT.offsetMax = Vector2.zero;

            // Components
            var sysGo = new GameObject("UI Systems");
            sysGo.transform.SetParent(canvasGo.transform, false);

            hud = sysGo.AddComponent<HUD>();
            hud.energyFill = fillImg;
            hud.energyRoot = energyRoot;
            hud.freezeOverlay = ov;
            hud.characterLabel = charTxt;
            energyRoot.SetActive(showHud);
            charGo.SetActive(showHud);

            pause = sysGo.AddComponent<PauseMenu>();
            pause.panel = pauseGo;

            fader = sysGo.AddComponent<SceneFader>();
            fader.overlay = fadeImg;

            dialogue = sysGo.AddComponent<DialogueSystem>();
            dialogue.Bind(dlgGo, spk, bdy, blink);
            dlgGo.SetActive(false);

            return canvasGo;
        }

        // ---------- Common containers ----------
        public static (Transform world, Transform players, PartyManager party, TimeFreezeSystem freeze, CarrySystem carry, CheckpointManager ckpt) CreateWorld()
        {
            var world = new GameObject("World").transform;
            var players = new GameObject("Players").transform;

            var systems = new GameObject("LevelSystems");
            var party = systems.AddComponent<PartyManager>();
            var freeze = systems.AddComponent<TimeFreezeSystem>();
            var carry = systems.AddComponent<CarrySystem>();
            var ckpt = systems.AddComponent<CheckpointManager>();
            return (world, players, party, freeze, carry, ckpt);
        }
    }
}
