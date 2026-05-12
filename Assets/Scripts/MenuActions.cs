using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuActions : MonoBehaviour
{
    public GameObject creditsPanel;

    public void Comecar()
    {
        SceneManager.LoadScene(SceneNames.Prologue);
    }

    public void AbrirCreditos()
    {
        if (creditsPanel != null)
            creditsPanel.SetActive(true);
    }

    public void FecharCreditos()
    {
        if (creditsPanel != null)
            creditsPanel.SetActive(false);
    }

    public void Sair()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void VoltarMenu()
    {
        SceneManager.LoadScene(SceneNames.MainMenu);
    }
}
