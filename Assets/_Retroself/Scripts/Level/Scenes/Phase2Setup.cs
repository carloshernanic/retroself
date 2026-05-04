using System.Collections.Generic;
using Retroself.Audio;
using Retroself.Mechanics;
using Retroself.Narrative;
using Retroself.Player;
using Retroself.UI;
using UnityEngine;

namespace Retroself.Level.Scenes
{
    public class Phase2Setup : MonoBehaviour
    {
        void Start()
        {
            var bg = new Color(0.20f, 0.12f, 0.10f);
            var (cam, follow) = SceneBuilder.CreateCamera(bg);
            follow.minBounds = new Vector2(-5, 2);
            follow.maxBounds = new Vector2(40, 5);

            var (world, players, party, freeze, carry, ckpt) = SceneBuilder.CreateWorld();
            freeze.unlocked = true;
            freeze.maxEnergy = 4f;
            freeze.drainPerSecond = 1f;
            freeze.rechargePerSecond = 0.8f;
            party.allowSwitch = true;

            // House background — wallpaper
            for (int i = 0; i < 12; i++)
            {
                Vector2 p = new Vector2(i * 3.5f - 4f, 3f);
                SceneBuilder.CreateDeco(p, new Vector2(3.4f, 5f), new Color(0.45f, 0.30f, 0.22f, 0.55f), world, -2, "Wall");
            }
            // Picture frames deco
            for (int i = 0; i < 4; i++)
            {
                SceneBuilder.CreateDeco(new Vector2(i * 7f, 3.2f), new Vector2(0.6f, 0.6f), new Color(0.85f, 0.75f, 0.55f), world, -1, "Pic");
            }

            // Ground (floor)
            SceneBuilder.CreateSolid(new Vector2(20, -1), new Vector2(60, 2), new Color(0.50f, 0.30f, 0.20f), world, "Floor");

            // ====== Section 1: introduce freeze with sound waves ======
            // Emitter shoots waves rightward, player must freeze to cross
            SceneBuilder.CreateSoundEmitter(new Vector2(2f, 1.5f), Vector2.right, 1.2f, world);
            // Some wall to slow movement and force timing
            SceneBuilder.CreateSolid(new Vector2(6f, 1.0f), new Vector2(0.5f, 1f), new Color(0.4f, 0.25f, 0.18f), world, "Bump");

            // ====== Section 2: shelf to push to block sound + plate combo ======
            SceneBuilder.CreateBox(new Vector2(8f, 0.5f), 1f, world);
            // Plate that opens door, weighted by box (adult must push)
            var door = SceneBuilder.CreateSolid(new Vector2(13f, 1.5f), new Vector2(0.5f, 3f), new Color(0.55f, 0.35f, 0.22f), world, "Door");
            var plate = SceneBuilder.CreatePlate(new Vector2(11f, 0.05f), PressurePlate.WeightRequirement.AdultOrBox, world);
            var ps = plate.GetComponent<PressurePlate>();
            var dCol = door.GetComponent<BoxCollider2D>(); var dSr = door.GetComponent<SpriteRenderer>();
            ps.onActivated.AddListener(() => { dCol.enabled = false; var c = dSr.color; c.a = 0.25f; dSr.color = c; });
            ps.onDeactivated.AddListener(() => { dCol.enabled = true; var c = dSr.color; c.a = 1f; dSr.color = c; });

            // ====== Section 3: emitter-corridor needing freeze + carry ======
            SceneBuilder.CreateSoundEmitter(new Vector2(20f, 1.5f), Vector2.left, 1.0f, world);
            SceneBuilder.CreateSoundEmitter(new Vector2(15f, 2.5f), Vector2.right, 1.4f, world);
            // Narrow gap only kid passes
            var blocker = SceneBuilder.CreateSolid(new Vector2(17f, 0.25f), new Vector2(0.6f, 0.5f), new Color(0.6f, 0.40f, 0.22f), world, "Crawl");
            SceneBuilder.CreateSolid(new Vector2(17f, 1.5f), new Vector2(0.6f, 2f), new Color(0.42f, 0.28f, 0.18f), world, "CrawlTop");
            var gapTriggerGo = new GameObject("GapTrigger");
            gapTriggerGo.transform.SetParent(world, false);
            gapTriggerGo.transform.position = new Vector3(17f, 0.25f, 0);
            var gtc = gapTriggerGo.AddComponent<BoxCollider2D>();
            gtc.size = new Vector2(1.5f, 0.6f); gtc.isTrigger = true;
            var gt = gapTriggerGo.AddComponent<GapTrigger>();
            gt.allowedKind = WoodyKind.Young;
            gt.blocker = blocker.GetComponent<Collider2D>();

            // ====== Section 4: rising platforms, freeze a sound emitter to pass ======
            SceneBuilder.CreateSolid(new Vector2(22f, 1.5f), new Vector2(2f, 0.3f), new Color(0.50f, 0.30f, 0.20f), world, "Step1");
            SceneBuilder.CreateSolid(new Vector2(25f, 2.5f), new Vector2(2f, 0.3f), new Color(0.50f, 0.30f, 0.20f), world, "Step2");
            SceneBuilder.CreateSoundEmitter(new Vector2(28f, 3.0f), Vector2.left, 0.9f, world);

            // Photo collectible
            SceneBuilder.CreateCollectible(new Vector2(25f, 3.5f), new Color(1f, 0.85f, 0.4f), world, "phase2_photo");

            // Kill zone
            SceneBuilder.CreateKillZone(new Vector2(20, -6), new Vector2(60, 1), world);

            // Checkpoints
            SceneBuilder.CreateCheckpoint(new Vector2(13.5f, 0.5f), new Vector2(13.5f, 0.5f), new Vector2(14f, 0.5f), world);
            SceneBuilder.CreateCheckpoint(new Vector2(22f, 1.9f), new Vector2(22f, 1.9f), new Vector2(22.6f, 1.9f), world);

            // Goal
            SceneBuilder.CreateGoal(new Vector2(33f, 0.7f), "phase_2", "Hub", world, both: true);

            // Players
            Vector2 adultSpawn = new Vector2(-2f, 0.7f);
            Vector2 youngSpawn = new Vector2(-1f, 0.5f);
            var adult = SceneBuilder.CreateAdult(adultSpawn, players);
            var young = SceneBuilder.CreateYoung(youngSpawn, players);
            party.Register(adult);
            party.Register(young);
            party.SetActive(WoodyKind.Adult);
            ckpt.fallbackAdultSpawn = adultSpawn;
            ckpt.fallbackYoungSpawn = youngSpawn;

            // UI
            SceneBuilder.CreateUI(out var hud, out var pause, out var fader, out var dialogue);

            AudioManager.Instance?.StartMusic(196f, 70f);

            // Intro
            dialogue.Play(new List<DialogueLine> {
                new DialogueLine{ speaker = "Woody (criança)", text = "Eles estão brigando de novo. Eu odeio domingos.", pitch = 1.5f },
                new DialogueLine{ speaker = "Woody", text = "A gente sai pelos fundos. Eu seguro o tempo, você corre.", pitch = 0.9f },
                new DialogueLine{ speaker = "Woody", text = "SHIFT congela o tempo. Só funciona enquanto eu estou ativo (TAB).", pitch = 0.9f },
            });
        }
    }
}
