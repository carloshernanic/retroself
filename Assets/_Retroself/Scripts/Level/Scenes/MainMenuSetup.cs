using Retroself.Audio;
using Retroself.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Retroself.Level.Scenes
{
    public class MainMenuSetup : MonoBehaviour
    {
        void Start()
        {
            GameManager.Instance?.SetPaused(false);
            SceneBuilder.CreateCamera(new Color(0.04f, 0.05f, 0.10f), follow: false);

            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            canvasGo.AddComponent<GraphicRaycaster>();

            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // Title
            var titleGo = new GameObject("Title"); titleGo.transform.SetParent(canvasGo.transform, false);
            var title = titleGo.AddComponent<Text>();
            title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            title.text = "retroself";
            title.fontSize = 86;
            title.color = new Color(1f, 0.85f, 0.4f);
            title.alignment = TextAnchor.MiddleCenter;
            var trt = title.rectTransform;
            trt.anchorMin = new Vector2(0.5f, 0.6f); trt.anchorMax = new Vector2(0.5f, 0.6f); trt.pivot = new Vector2(0.5f, 0.5f);
            trt.sizeDelta = new Vector2(800, 120);

            var subGo = new GameObject("Sub"); subGo.transform.SetParent(canvasGo.transform, false);
            var sub = subGo.AddComponent<Text>();
            sub.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            sub.text = "\"E se você pudesse voltar e ajudar a si mesmo a não desistir?\"";
            sub.fontSize = 18;
            sub.color = new Color(0.85f, 0.85f, 0.95f, 0.85f);
            sub.alignment = TextAnchor.MiddleCenter;
            var srt = sub.rectTransform;
            srt.anchorMin = new Vector2(0.5f, 0.5f); srt.anchorMax = new Vector2(0.5f, 0.5f); srt.pivot = new Vector2(0.5f, 0.5f);
            srt.anchoredPosition = new Vector2(0, 30);
            srt.sizeDelta = new Vector2(900, 40);

            // Buttons
            CreateButton(canvasGo.transform, "Começar", new Vector2(0, -60), () => SceneManager.LoadScene("Prologue"));
            CreateButton(canvasGo.transform, "Continuar (Hub)", new Vector2(0, -120), () => SceneManager.LoadScene("Hub"));
            CreateButton(canvasGo.transform, "Sair", new Vector2(0, -180), () => Application.Quit());

            // Controls hint
            var ctrlGo = new GameObject("Ctrl"); ctrlGo.transform.SetParent(canvasGo.transform, false);
            var ctrl = ctrlGo.AddComponent<Text>();
            ctrl.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            ctrl.text = "Mover: A/D ou ←/→   Pular: ESPAÇO   Trocar: TAB   Pegar/Soltar: E   Congelar tempo: SHIFT   Pausar: ESC";
            ctrl.fontSize = 14;
            ctrl.color = new Color(1, 1, 1, 0.55f);
            ctrl.alignment = TextAnchor.LowerCenter;
            var crt = ctrl.rectTransform;
            crt.anchorMin = new Vector2(0, 0); crt.anchorMax = new Vector2(1, 0); crt.pivot = new Vector2(0.5f, 0);
            crt.anchoredPosition = new Vector2(0, 24); crt.sizeDelta = new Vector2(-40, 30);

            AudioManager.Instance?.StartMusic(220f, 70f);
        }

        Button CreateButton(Transform parent, string label, Vector2 anchoredPos, System.Action onClick)
        {
            var go = new GameObject(label); go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>(); img.color = new Color(0.12f, 0.14f, 0.20f, 0.9f);
            var rt = img.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f); rt.anchorMax = new Vector2(0.5f, 0.5f); rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos; rt.sizeDelta = new Vector2(280, 44);

            var txtGo = new GameObject("Text"); txtGo.transform.SetParent(go.transform, false);
            var txt = txtGo.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.text = label; txt.alignment = TextAnchor.MiddleCenter; txt.color = Color.white; txt.fontSize = 22;
            var trt = txt.rectTransform; trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one; trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.normalColor = new Color(0.12f, 0.14f, 0.20f, 0.9f);
            colors.highlightedColor = new Color(0.20f, 0.22f, 0.30f, 1f);
            colors.pressedColor = new Color(0.30f, 0.25f, 0.10f, 1f);
            btn.colors = colors;
            btn.onClick.AddListener(() => onClick?.Invoke());
            return btn;
        }
    }
}
