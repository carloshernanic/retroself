using UnityEngine;
using TMPro;

public class TitleGlitch : MonoBehaviour
{
    public TMP_Text titleText;
    public float minInterval = 3f;
    public float maxInterval = 6f;
    public float glitchDuration = 0.12f;

    private string originalText;
    private float nextGlitchTime;
    private const string glitchChars = "█▓▒░#@&%*¥¤§";

    void Start()
    {
        if (titleText == null)
            titleText = GetComponent<TMP_Text>();

        originalText = titleText.text;
        ScheduleNext();
    }

    void Update()
    {
        if (Time.time >= nextGlitchTime)
        {
            StartCoroutine(DoGlitch());
            ScheduleNext();
        }
    }

    void ScheduleNext()
    {
        nextGlitchTime = Time.time + Random.Range(minInterval, maxInterval);
    }

    System.Collections.IEnumerator DoGlitch()
    {
        float elapsed = 0f;
        while (elapsed < glitchDuration)
        {
            char[] chars = originalText.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] != ' ' && Random.value < 0.3f)
                    chars[i] = glitchChars[Random.Range(0, glitchChars.Length)];
            }
            titleText.text = new string(chars);
            elapsed += 0.04f;
            yield return new WaitForSeconds(0.04f);
        }
        titleText.text = originalText;
    }
}
