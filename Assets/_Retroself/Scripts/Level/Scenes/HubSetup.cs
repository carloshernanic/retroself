using System.Collections.Generic;
using Retroself.Audio;
using Retroself.Core;
using Retroself.Narrative;
using Retroself.Player;
using Retroself.UI;
using UnityEngine;

namespace Retroself.Level.Scenes
{
    public class HubSetup : MonoBehaviour
    {
        void Start()
        {
            var bg = new Color(0.05f, 0.03f, 0.10f);
            var (cam, follow) = SceneBuilder.CreateCamera(bg);
            var (world, players, party, freeze, carry, ckpt) = SceneBuilder.CreateWorld();
            freeze.unlocked = false;
            party.allowSwitch = false;

            // Floor
            SceneBuilder.CreateSolid(new Vector2(0, -1), new Vector2(60, 2), new Color(0.18f, 0.10f, 0.28f), world, "Floor");
            // Ceiling deco - alien dots
            for (int i = 0; i < 60; i++)
            {
                Vector2 p = new Vector2(Random.Range(-30f, 30f), Random.Range(2f, 9f));
                SceneBuilder.CreateDeco(p, new Vector2(0.15f, 0.15f), new Color(0.6f, 1f, 0.9f, Random.Range(0.2f, 0.7f)), world, -2, "Star");
            }

            // Praça window — changes with progress
            int done = GameManager.Instance != null ? GameManager.Instance.phasesCompleted : 0;
            Color sky = Color.Lerp(new Color(0.12f, 0.14f, 0.22f), new Color(0.95f, 0.75f, 0.40f), Mathf.Clamp01(done / 4f));
            SceneBuilder.CreateDeco(new Vector2(-9, 4f), new Vector2(4, 3f), new Color(0.10f, 0.10f, 0.18f), world, -1, "WindowFrame");
            SceneBuilder.CreateDeco(new Vector2(-9, 4f), new Vector2(3.6f, 2.6f), sky, world, 0, "WindowSky");

            // Portals
            CreatePortal(world, new Vector2(-2.5f, 1f), new Color(0.95f, 0.85f, 0.30f), "Phase1", "phase_1");
            CreatePortal(world, new Vector2(2.5f, 1f), new Color(0.85f, 0.55f, 0.40f), "Phase2", "phase_2");

            // Aliens (decoration NPCs)
            SceneBuilder.CreateDeco(new Vector2(8, 0.5f), new Vector2(0.8f, 0.8f), new Color(1f, 0.4f, 0.4f), world, 1, "Alien_R");
            SceneBuilder.CreateDeco(new Vector2(9, 0.5f), new Vector2(0.8f, 0.8f), new Color(0.4f, 0.6f, 1f), world, 1, "Alien_B");
            SceneBuilder.CreateDeco(new Vector2(10, 0.5f), new Vector2(0.8f, 0.8f), new Color(0.95f, 0.85f, 0.30f), world, 1, "Alien_Y");

            // Player (adult only in hub)
            var adult = SceneBuilder.CreateAdult(new Vector2(0f, 0.7f), players);
            party.Register(adult);
            party.SetActive(WoodyKind.Adult);

            ckpt.fallbackAdultSpawn = new Vector2(0f, 0.7f);

            SceneBuilder.CreateUI(out var hud, out var pause, out var fader, out var dialogue, showHud: false);
            AudioManager.Instance?.StartMusic(260f, 65f);

            // Welcome dialogue
            var welcomeLines = new List<DialogueLine>();
            if (done == 0)
            {
                welcomeLines.Add(new DialogueLine { speaker = "Alienígena Vermelho", text = "Bem-vindo de volta. Escolha uma lembrança.", pitch = 1.4f });
                welcomeLines.Add(new DialogueLine { speaker = "Alienígena Azul", text = "Cada portal é uma idade sua. Você não vai mudar o passado — só ajudá-lo a atravessar.", pitch = 1.5f });
            }
            else if (done >= 2)
            {
                welcomeLines.Add(new DialogueLine { speaker = "Alienígena Amarelo", text = "A praça já está mais clara. Você está se cuidando.", pitch = 1.6f });
            }
            else
            {
                welcomeLines.Add(new DialogueLine { speaker = "Alienígena Vermelho", text = "Mais uma. Você consegue.", pitch = 1.4f });
            }

            if (done >= 2)
            {
                welcomeLines.Add(new DialogueLine { speaker = "Alienígena Azul", text = "Quando estiver pronto, durma. A praça espera.", pitch = 1.5f });
            }
            dialogue.Play(welcomeLines);

            // Endgame portal if both phases done
            if (done >= 2)
            {
                CreatePortal(world, new Vector2(0f, 4f), new Color(1f, 0.8f, 0.6f), "Epilogue", "epilogue", floating: true);
            }
        }

        void CreatePortal(Transform parent, Vector2 pos, Color color, string sceneName, string phaseId, bool floating = false)
        {
            string label;
            switch (sceneName)
            {
                case "Phase1": label = "PÁTIO\n(7 anos)"; break;
                case "Phase2": label = "DOMINGO\n(12 anos)"; break;
                case "Epilogue": label = "DORMIR"; break;
                default: label = sceneName; break;
            }

            var go = new GameObject($"Portal_{sceneName}");
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSprites.GetRect(28, 36, color);
            sr.sortingOrder = 2;
            sr.color = new Color(color.r, color.g, color.b, 0.75f);

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.75f, 2.25f);
            col.isTrigger = true;

            // Show "completed" check
            bool completed = GameManager.Instance != null && GameManager.Instance.IsPhaseComplete(phaseId);
            if (completed)
            {
                var check = new GameObject("Check");
                check.transform.SetParent(go.transform, false);
                check.transform.localPosition = new Vector3(0, 1.5f, 0);
                var csr = check.AddComponent<SpriteRenderer>();
                csr.sprite = ProceduralSprites.GetCircle(16, new Color(0.5f, 1f, 0.6f));
                csr.sortingOrder = 3;
            }

            // Label below
            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            labelGo.transform.localPosition = new Vector3(0, -1.4f, 0);
            var canvas = labelGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 5;
            var rt = labelGo.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(3f, 0.6f);
            labelGo.transform.localScale = Vector3.one * 0.05f;
            var txtGo = new GameObject("Text"); txtGo.transform.SetParent(labelGo.transform, false);
            var txt = txtGo.AddComponent<UnityEngine.UI.Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.text = label;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.fontSize = 16;
            var trt = txt.rectTransform; trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one; trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;

            var trig = go.AddComponent<PortalTrigger>();
            trig.targetScene = sceneName;
        }
    }

    public class PortalTrigger : MonoBehaviour
    {
        public string targetScene;
        bool fired;
        void OnTriggerEnter2D(Collider2D other)
        {
            if (fired) return;
            var w = other.GetComponentInParent<WoodyController>();
            if (w == null || !w.IsActive) return;
            fired = true;
            var fader = FindFirstObjectByType<SceneFader>();
            if (fader != null) fader.FadeAndLoad(targetScene);
            else UnityEngine.SceneManagement.SceneManager.LoadScene(targetScene);
        }
    }
}
