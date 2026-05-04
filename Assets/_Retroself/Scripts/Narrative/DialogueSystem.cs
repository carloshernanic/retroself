using System;
using System.Collections;
using System.Collections.Generic;
using Retroself.Audio;
using Retroself.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Retroself.Narrative
{
    public class DialogueSystem : MonoBehaviour
    {
        public static DialogueSystem Instance { get; private set; }

        public bool IsActive { get; private set; }

        GameObject panelRoot;
        Text speakerText;
        Text bodyText;
        Graphic continueBlinker;
        Coroutine running;
        Action onComplete;
        Queue<DialogueLine> queue = new Queue<DialogueLine>();
        DialogueLine current;
        bool typing;
        string fullText;

        public float charsPerSecond = 32f;

        void Awake() { Instance = this; }
        void OnDestroy() { if (Instance == this) Instance = null; }

        public void Bind(GameObject panel, Text speaker, Text body, Graphic blinker)
        {
            panelRoot = panel;
            speakerText = speaker;
            bodyText = body;
            continueBlinker = blinker;
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        public void Play(IEnumerable<DialogueLine> lines, Action onDone = null)
        {
            queue.Clear();
            foreach (var l in lines) queue.Enqueue(l);
            onComplete = onDone;
            IsActive = true;
            if (panelRoot != null) panelRoot.SetActive(true);
            Next();
        }

        void Next()
        {
            if (queue.Count == 0)
            {
                Close();
                return;
            }
            current = queue.Dequeue();
            if (speakerText != null) speakerText.text = current.speaker;
            fullText = current.text;
            if (running != null) StopCoroutine(running);
            running = StartCoroutine(Typewriter());
        }

        IEnumerator Typewriter()
        {
            typing = true;
            if (bodyText != null) bodyText.text = "";
            float t = 0f;
            int last = -1;
            while (t < fullText.Length)
            {
                t += charsPerSecond * Time.unscaledDeltaTime;
                int n = Mathf.Min(fullText.Length, Mathf.FloorToInt(t));
                if (n != last && bodyText != null)
                {
                    bodyText.text = fullText.Substring(0, n);
                    if (n > 0 && n > last && fullText[n - 1] != ' ' && AudioManager.Instance != null)
                        AudioManager.Instance.PlayBeep(current.pitch * 480f, 0.025f, 0.1f);
                    last = n;
                }
                yield return null;
            }
            if (bodyText != null) bodyText.text = fullText;
            typing = false;
        }

        void Update()
        {
            if (!IsActive) return;
            if (InputReader.Instance != null && InputReader.Instance.AdvanceDialoguePressed)
            {
                if (typing)
                {
                    if (running != null) StopCoroutine(running);
                    typing = false;
                    if (bodyText != null) bodyText.text = fullText;
                }
                else Next();
            }
            if (continueBlinker != null && !typing)
            {
                var c = continueBlinker.color;
                c.a = (Mathf.Sin(Time.unscaledTime * 6f) + 1f) * 0.5f;
                continueBlinker.color = c;
            }
        }

        void Close()
        {
            IsActive = false;
            if (panelRoot != null) panelRoot.SetActive(false);
            onComplete?.Invoke();
        }
    }
}
