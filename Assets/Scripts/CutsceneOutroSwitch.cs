using UnityEngine;

// Wire o método SwitchToCredits como listener do CutsceneController.OnFinished.
// Desativa o canvas da cutscene e ativa o canvas dos créditos — mais simples que
// montar dois persistent listeners de SetActive(bool) via UnityEventTools.
public class CutsceneOutroSwitch : MonoBehaviour
{
    public GameObject cutsceneCanvas;
    public GameObject creditsCanvas;

    public void SwitchToCredits()
    {
        if (cutsceneCanvas != null) cutsceneCanvas.SetActive(false);
        if (creditsCanvas != null) creditsCanvas.SetActive(true);
    }
}
