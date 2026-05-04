using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TypewriterText : MonoBehaviour
{
    public TMP_Text target;
    public float charsPerSecond = 30f;
    public AudioSource blipSource;
    public AudioClip blipClip;
    public float blipPitch = 1f;
    public int blipEveryNChars = 2;

    private Coroutine current;
    private string fullText = "";
    private bool isPlaying;
    private Action onCompleteCallback;

    public bool IsPlaying => isPlaying;

    public void Play(string text, Action onComplete)
    {
        if (current != null) StopCoroutine(current);
        fullText = text ?? "";
        onCompleteCallback = onComplete;
        current = StartCoroutine(Run());
    }

    public void SkipToEnd()
    {
        if (!isPlaying) return;
        if (current != null) StopCoroutine(current);
        if (target != null) target.text = fullText;
        isPlaying = false;
        var cb = onCompleteCallback;
        onCompleteCallback = null;
        cb?.Invoke();
    }

    IEnumerator Run()
    {
        if (target == null) { isPlaying = false; yield break; }
        isPlaying = true;
        target.text = "";
        float interval = 1f / Mathf.Max(charsPerSecond, 1f);

        for (int i = 0; i < fullText.Length; i++)
        {
            target.text += fullText[i];
            if (blipSource != null && blipClip != null && fullText[i] != ' ' && i % Mathf.Max(blipEveryNChars, 1) == 0)
            {
                blipSource.pitch = blipPitch;
                blipSource.PlayOneShot(blipClip);
            }
            yield return new WaitForSeconds(interval);
        }

        isPlaying = false;
        var cb = onCompleteCallback;
        onCompleteCallback = null;
        cb?.Invoke();
    }
}
