using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Retroself.UI
{
    public class SceneFader : MonoBehaviour
    {
        public Image overlay;
        public float fadeDuration = 0.6f;

        void Start()
        {
            if (overlay != null) StartCoroutine(Fade(1f, 0f));
        }

        public void FadeAndLoad(string scene)
        {
            StartCoroutine(FadeOutAndLoad(scene));
        }

        IEnumerator FadeOutAndLoad(string scene)
        {
            yield return Fade(0f, 1f);
            SceneManager.LoadScene(scene);
        }

        IEnumerator Fade(float from, float to)
        {
            if (overlay == null) yield break;
            float t = 0f;
            var c = overlay.color;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                c.a = Mathf.Lerp(from, to, t / fadeDuration);
                overlay.color = c;
                yield return null;
            }
            c.a = to;
            overlay.color = c;
        }
    }
}
