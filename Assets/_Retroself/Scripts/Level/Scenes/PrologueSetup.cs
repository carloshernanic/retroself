using System.Collections.Generic;
using Retroself.Audio;
using Retroself.Narrative;
using Retroself.Player;
using Retroself.UI;
using UnityEngine;

namespace Retroself.Level.Scenes
{
    public class PrologueSetup : MonoBehaviour
    {
        void Start()
        {
            var (cam, follow) = SceneBuilder.CreateCamera(new Color(0.05f, 0.06f, 0.12f));
            var (world, players, party, freeze, carry, ckpt) = SceneBuilder.CreateWorld();
            freeze.unlocked = false;

            // Ground
            SceneBuilder.CreateSolid(new Vector2(0, -1), new Vector2(60, 2), new Color(0.12f, 0.14f, 0.18f), world, "Ground");
            // Sky deco — rain drops
            for (int i = 0; i < 30; i++)
            {
                Vector2 p = new Vector2(Random.Range(-25f, 25f), Random.Range(2f, 10f));
                SceneBuilder.CreateDeco(p, new Vector2(0.1f, 0.5f), new Color(0.6f, 0.7f, 0.95f, 0.45f), world, -1, "Rain");
            }
            // Bench
            SceneBuilder.CreateSolid(new Vector2(-6, 0.3f), new Vector2(2.5f, 0.3f), new Color(0.45f, 0.30f, 0.20f), world, "Bench");
            SceneBuilder.CreateSolid(new Vector2(-7, 0.7f), new Vector2(0.2f, 0.8f), new Color(0.45f, 0.30f, 0.20f), world, "BenchLeg");
            SceneBuilder.CreateSolid(new Vector2(-5, 0.7f), new Vector2(0.2f, 0.8f), new Color(0.45f, 0.30f, 0.20f), world, "BenchLeg");
            // Tree
            SceneBuilder.CreateSolid(new Vector2(6, 1f), new Vector2(0.6f, 2f), new Color(0.30f, 0.20f, 0.12f), world, "TreeTrunk");
            SceneBuilder.CreateDeco(new Vector2(6, 3f), new Vector2(2.5f, 1.8f), new Color(0.10f, 0.30f, 0.22f), world, -1, "TreeTop");
            // Alien booth (behind tree)
            var booth = SceneBuilder.CreateSolid(new Vector2(8.5f, 1.2f), new Vector2(1.8f, 2.4f), new Color(0.25f, 0.10f, 0.40f), world, "Booth");
            Destroy(booth.GetComponent<BoxCollider2D>());
            SceneBuilder.CreateDeco(new Vector2(8.5f, 1.4f), new Vector2(1.0f, 0.6f), new Color(0.65f, 0.95f, 1f), world, -1, "BoothGlow");

            // Player (only adult)
            var adult = SceneBuilder.CreateAdult(new Vector2(-6f, 0.7f), players);
            party.Register(adult);
            party.SetActive(WoodyKind.Adult);
            party.allowSwitch = false;
            ckpt.fallbackAdultSpawn = new Vector2(-6f, 0.7f);
            ckpt.fallbackYoungSpawn = Vector2.zero;

            // UI
            SceneBuilder.CreateUI(out var hud, out var pause, out var fader, out var dialogue, showHud: false);

            AudioManager.Instance?.StartMusic(180f, 60f);

            // Trigger to alien booth
            var trig = new GameObject("EnterBooth");
            trig.transform.position = new Vector3(8.5f, 1.2f, 0);
            var col = trig.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.6f, 2.4f);
            col.isTrigger = true;
            var enter = trig.AddComponent<TriggerOnce>();
            enter.onEnter = () => {
                dialogue.Play(new List<DialogueLine> {
                    new DialogueLine{ speaker = "Woody", text = "Outra noite assim. Que cabine é essa atrás da árvore?", pitch = 0.9f },
                    new DialogueLine{ speaker = "??? ", text = "Você não é mau. Só tropeçou nas piores horas.", pitch = 1.4f },
                    new DialogueLine{ speaker = "??? ", text = "Volte. Ajude a si mesmo. Quatro lembranças. Um relógio.", pitch = 1.4f },
                    new DialogueLine{ speaker = "Woody", text = "...você é real?", pitch = 0.9f },
                    new DialogueLine{ speaker = "??? ", text = "Mais real do que você se permitiu ser por anos.", pitch = 1.4f },
                }, () => fader.FadeAndLoad("Hub"));
            };

            // Initial dialogue
            dialogue.Play(new List<DialogueLine> {
                new DialogueLine{ speaker = "Woody", text = "Chovendo de novo. O banco já decorou meu corpo.", pitch = 0.9f },
                new DialogueLine{ speaker = "Woody", text = "Tem uma luz... atrás da árvore. Ali. (caminhe até a luz roxa)", pitch = 0.9f },
            });
        }
    }

    public class TriggerOnce : MonoBehaviour
    {
        public System.Action onEnter;
        bool fired;
        void OnTriggerEnter2D(Collider2D other)
        {
            if (fired) return;
            var w = other.GetComponentInParent<WoodyController>();
            if (w == null) return;
            fired = true;
            onEnter?.Invoke();
        }
    }
}
