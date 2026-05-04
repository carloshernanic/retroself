using UnityEngine;

namespace Retroself.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        AudioSource sfxSource;
        AudioSource musicSource;
        float musicTargetVolume = 0.35f;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = musicTargetVolume;
        }

        public void PlayBeep(float frequency, float duration, float volume = 0.3f)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            var clip = AudioClip.Create("beep", sampleCount, 1, sampleRate, false);
            float[] data = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float env = 1f - (t / duration);
                float square = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * frequency * t));
                data[i] = square * 0.6f * env * volume;
            }
            clip.SetData(data, 0);
            sfxSource.PlayOneShot(clip);
        }

        public void PlayJump() => PlayBeep(660f, 0.08f, 0.25f);
        public void PlayLand() => PlayBeep(180f, 0.06f, 0.2f);
        public void PlaySwitch() => PlayBeep(440f, 0.06f, 0.2f);
        public void PlayFreezeStart() => PlayBeep(220f, 0.18f, 0.3f);
        public void PlayFreezeEnd() => PlayBeep(330f, 0.12f, 0.25f);
        public void PlayPickup() => PlayBeep(880f, 0.12f, 0.35f);

        public void StartMusic(float baseFreq, float bpm = 80f)
        {
            int sampleRate = 22050;
            float beatDur = 60f / bpm;
            int totalBeats = 16;
            int sampleCount = Mathf.CeilToInt(sampleRate * beatDur * totalBeats);
            var clip = AudioClip.Create("loop", sampleCount, 1, sampleRate, false);
            float[] data = new float[sampleCount];
            float[] notes = { 1f, 1.2f, 1.5f, 1.2f, 0.8f, 1f, 1.2f, 1f,
                              1f, 1.5f, 1.2f, 1f, 0.75f, 1f, 0.8f, 1f };
            for (int b = 0; b < totalBeats; b++)
            {
                float freq = baseFreq * notes[b % notes.Length];
                int start = Mathf.FloorToInt(sampleRate * beatDur * b);
                int len = Mathf.FloorToInt(sampleRate * beatDur);
                for (int i = 0; i < len; i++)
                {
                    float t = (float)i / sampleRate;
                    float env = Mathf.Min(1f, (len - i) / (float)sampleRate * 4f);
                    data[start + i] = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * freq * t)) * 0.18f * env;
                }
            }
            clip.SetData(data, 0);
            musicSource.clip = clip;
            musicSource.volume = musicTargetVolume;
            musicSource.Play();
        }

        public void StopMusic()
        {
            musicSource.Stop();
        }
    }
}
