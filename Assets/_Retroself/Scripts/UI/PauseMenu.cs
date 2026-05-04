using Retroself.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Retroself.UI
{
    public class PauseMenu : MonoBehaviour
    {
        public GameObject panel;

        void Update()
        {
            if (InputReader.Instance == null) return;
            if (InputReader.Instance.PausePressed) Toggle();
        }

        public void Toggle()
        {
            bool newState = !GameManager.Instance.IsPaused;
            GameManager.Instance.SetPaused(newState);
            if (panel != null) panel.SetActive(newState);
        }

        public void Resume()
        {
            GameManager.Instance.SetPaused(false);
            if (panel != null) panel.SetActive(false);
        }

        public void GoToHub()
        {
            GameManager.Instance.SetPaused(false);
            SceneManager.LoadScene("Hub");
        }

        public void GoToMain()
        {
            GameManager.Instance.SetPaused(false);
            SceneManager.LoadScene("MainMenu");
        }

        public void Quit() { Application.Quit(); }
    }
}
