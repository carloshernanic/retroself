using System.Collections.Generic;
using Retroself.Audio;
using Retroself.Mechanics;
using Retroself.Narrative;
using Retroself.Player;
using Retroself.UI;
using UnityEngine;

namespace Retroself.Level.Scenes
{
    public class Phase1Setup : MonoBehaviour
    {
        void Start()
        {
            var bg = new Color(0.55f, 0.50f, 0.20f);
            var (cam, follow) = SceneBuilder.CreateCamera(bg);
            follow.minBounds = new Vector2(-5, 2);
            follow.maxBounds = new Vector2(40, 4);

            var (world, players, party, freeze, carry, ckpt) = SceneBuilder.CreateWorld();
            freeze.unlocked = false;
            party.allowSwitch = true;

            // Background bricks
            for (int i = 0; i < 20; i++)
            {
                Vector2 p = new Vector2(i * 3f - 5f, 4f);
                SceneBuilder.CreateDeco(p, new Vector2(2.6f, 5f), new Color(0.45f, 0.30f, 0.20f, 0.5f), world, -2, "BG_Brick");
            }

            // Ground
            SceneBuilder.CreateSolid(new Vector2(20, -1), new Vector2(60, 2), new Color(0.30f, 0.50f, 0.20f), world, "Ground");

            // ============= Section 1: tutorial of duality =============
            // Wall too tall for the kid
            SceneBuilder.CreateSolid(new Vector2(5f, 1.0f), new Vector2(0.8f, 2f), new Color(0.42f, 0.28f, 0.18f), world, "Wall_Tall");
            // Box that adult can push to make a step
            SceneBuilder.CreateBox(new Vector2(3f, 0.5f), 1f, world);

            // ============= Section 2: gap that only kid fits =============
            // High wall both can't pass + low gap below
            SceneBuilder.CreateSolid(new Vector2(10f, 1.5f), new Vector2(0.6f, 3f), new Color(0.42f, 0.28f, 0.18f), world, "GapWall");
            // Gap blocker (only young passes by squeezing)
            var gapBlocker = SceneBuilder.CreateSolid(new Vector2(10f, 0.25f), new Vector2(0.6f, 0.5f), new Color(0.55f, 0.35f, 0.20f), world, "GapBlocker");
            var gapTriggerGo = new GameObject("GapTrigger");
            gapTriggerGo.transform.SetParent(world, false);
            gapTriggerGo.transform.position = new Vector3(10f, 0.25f, 0);
            var gtCol = gapTriggerGo.AddComponent<BoxCollider2D>();
            gtCol.size = new Vector2(1.5f, 0.6f);
            gtCol.isTrigger = true;
            var gt = gapTriggerGo.AddComponent<GapTrigger>();
            gt.allowedKind = WoodyKind.Young;
            gt.blocker = gapBlocker.GetComponent<Collider2D>();

            // ============= Section 3: pressure plate (adult) opens door for kid =============
            // Plate at x=14, door at x=17
            var door = SceneBuilder.CreateSolid(new Vector2(17f, 1.5f), new Vector2(0.5f, 3f), new Color(0.55f, 0.35f, 0.20f), world, "Door");
            var plate = SceneBuilder.CreatePlate(new Vector2(14f, 0.05f), PressurePlate.WeightRequirement.AdultOrBox, world);
            var plateScript = plate.GetComponent<PressurePlate>();
            var doorBox = door.GetComponent<BoxCollider2D>();
            var doorSr = door.GetComponent<SpriteRenderer>();
            plateScript.onActivated.AddListener(() => { doorBox.enabled = false; var c = doorSr.color; c.a = 0.25f; doorSr.color = c; });
            plateScript.onDeactivated.AddListener(() => { doorBox.enabled = true; var c = doorSr.color; c.a = 1f; doorSr.color = c; });

            // Bonus box for plate weight
            SceneBuilder.CreateBox(new Vector2(13f, 0.5f), 1f, world);

            // ============= Section 4: bully patrol — needs to time with kid =============
            // Bully blocks middle, while plate-door must stay open. Easier with carry.
            var bully = SceneBuilder.CreatePatrolEnemy(new Vector2(22f, 0.7f), 2.5f, 2.2f, new Color(0.85f, 0.30f, 0.25f), world);
            // Lower platform for kid to hide under
            SceneBuilder.CreateSolid(new Vector2(22f, 1.6f), new Vector2(3f, 0.3f), new Color(0.42f, 0.28f, 0.18f), world, "LowRoof");
            SceneBuilder.CreateSolid(new Vector2(20.7f, 1.0f), new Vector2(0.3f, 1.4f), new Color(0.42f, 0.28f, 0.18f), world, "RoofPillarL");
            SceneBuilder.CreateSolid(new Vector2(23.3f, 1.0f), new Vector2(0.3f, 1.4f), new Color(0.42f, 0.28f, 0.18f), world, "RoofPillarR");

            // Collectible (photo)
            SceneBuilder.CreateCollectible(new Vector2(22f, 2f), new Color(1f, 0.85f, 0.4f), world, "phase1_photo");

            // Kill zone if fall (just in case)
            SceneBuilder.CreateKillZone(new Vector2(20, -6), new Vector2(60, 1), world);

            // Checkpoint after door
            SceneBuilder.CreateCheckpoint(new Vector2(18f, 0.5f), new Vector2(18f, 0.5f), new Vector2(18.6f, 0.5f), world);

            // Goal at the end
            SceneBuilder.CreateGoal(new Vector2(28f, 0.7f), "phase_1", "Hub", world, both: true);

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

            AudioManager.Instance?.StartMusic(330f, 90f);

            // Intro dialogue
            dialogue.Play(new List<DialogueLine> {
                new DialogueLine{ speaker = "Woody (criança)", text = "Quem é você, tio? Como entrou na minha escola?", pitch = 1.5f },
                new DialogueLine{ speaker = "Woody", text = "Sou... um amigo. Vou te ajudar a atravessar o pátio.", pitch = 0.9f },
                new DialogueLine{ speaker = "Woody", text = "TAB troca quem você controla. E eu posso pegar a criança no colo (E).", pitch = 0.9f },
            });
        }
    }
}
