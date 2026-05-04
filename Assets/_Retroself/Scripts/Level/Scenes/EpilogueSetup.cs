using System.Collections.Generic;
using Retroself.Audio;
using Retroself.Core;
using Retroself.Narrative;
using Retroself.Player;
using Retroself.UI;
using UnityEngine;

namespace Retroself.Level.Scenes
{
    public class EpilogueSetup : MonoBehaviour
    {
        void Start()
        {
            int collected = GameManager.Instance != null ? GameManager.Instance.collectiblesFound : 0;
            int phases = GameManager.Instance != null ? GameManager.Instance.phasesCompleted : 0;
            // Better ending if more collected
            float warmth = Mathf.Clamp01((collected + phases) / 4f);
            Color bg = Color.Lerp(new Color(0.10f, 0.12f, 0.18f), new Color(0.95f, 0.75f, 0.40f), warmth);
            var (cam, follow) = SceneBuilder.CreateCamera(bg);

            var (world, players, party, freeze, carry, ckpt) = SceneBuilder.CreateWorld();
            freeze.unlocked = false;
            party.allowSwitch = false;

            // Ground
            SceneBuilder.CreateSolid(new Vector2(0, -1), new Vector2(60, 2), Color.Lerp(new Color(0.18f, 0.18f, 0.22f), new Color(0.55f, 0.45f, 0.30f), warmth), world, "Ground");
            // Bench
            SceneBuilder.CreateSolid(new Vector2(0, 0.3f), new Vector2(2.5f, 0.3f), new Color(0.45f, 0.30f, 0.20f), world, "Bench");
            SceneBuilder.CreateSolid(new Vector2(-1, 0.7f), new Vector2(0.2f, 0.8f), new Color(0.45f, 0.30f, 0.20f), world, "BenchLeg");
            SceneBuilder.CreateSolid(new Vector2(1, 0.7f), new Vector2(0.2f, 0.8f), new Color(0.45f, 0.30f, 0.20f), world, "BenchLeg");
            // Blanket / coat over bench (more if more progress)
            if (warmth > 0.3f)
                SceneBuilder.CreateDeco(new Vector2(0, 0.55f), new Vector2(2.5f, 0.4f), new Color(0.85f, 0.40f, 0.35f), world, 1, "Blanket");
            if (warmth > 0.7f)
                SceneBuilder.CreateDeco(new Vector2(0, 0.85f), new Vector2(1.5f, 0.6f), new Color(0.55f, 0.30f, 0.20f), world, 2, "Coat");

            // Sun / clouds
            SceneBuilder.CreateDeco(new Vector2(8, 6f), new Vector2(2f, 2f), new Color(1f, 0.92f, 0.55f, warmth * 0.9f + 0.1f), world, -1, "Sun");

            // Adult laying / standing on bench
            var adult = SceneBuilder.CreateAdult(new Vector2(0f, 0.7f), players);
            adult.GetComponent<CharacterMotor>().ControlEnabled = false;
            party.Register(adult);

            SceneBuilder.CreateUI(out var hud, out var pause, out var fader, out var dialogue, showHud: false);
            AudioManager.Instance?.StartMusic(180f + warmth * 60f, 60f);

            string ending;
            if (warmth >= 0.9f)
                ending = "Ele acorda. Não está despejado. Tem um cobertor por cima e um casaco no apoio do banco. A cidade respira, e ele também.";
            else if (warmth >= 0.5f)
                ending = "Ele acorda. O frio diminuiu um pouco. Há um cobertor estranho onde antes só havia chuva.";
            else
                ending = "Ele acorda. A praça é a mesma. Mas algo dentro dele se moveu — bem pouquinho. Já é alguma coisa.";

            dialogue.Play(new List<DialogueLine> {
                new DialogueLine{ speaker = "Woody", text = ending, pitch = 0.9f },
                new DialogueLine{ speaker = "Woody", text = "Obrigado, garoto.", pitch = 0.9f },
            }, () => {
                // Show credits / back to main
                ShowEndCard(warmth);
            });
        }

        void ShowEndCard(float warmth)
        {
            var go = new GameObject("EndCard");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 30;
            go.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            var bg = new GameObject("BG"); bg.transform.SetParent(go.transform, false);
            var bgImg = bg.AddComponent<UnityEngine.UI.Image>(); bgImg.color = new Color(0, 0, 0, 0.85f);
            var brt = bgImg.rectTransform; brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one; brt.offsetMin = Vector2.zero; brt.offsetMax = Vector2.zero;

            var txtGo = new GameObject("Text"); txtGo.transform.SetParent(go.transform, false);
            var txt = txtGo.AddComponent<UnityEngine.UI.Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.text = $"retroself\n\n{(warmth >= 0.9f ? "FIM — Final A" : warmth >= 0.5f ? "FIM — Final B" : "FIM — Final C")}\n\nObrigado por jogar.\n\nPressione ENTER para voltar ao menu.";
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.fontSize = 28;
            var trt = txt.rectTransform; trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one; trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;

            go.AddComponent<EndCardInput>();
        }
    }

    public class EndCardInput : MonoBehaviour
    {
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
            {
                GameManager.Instance?.ResetRun();
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
        }
    }
}
