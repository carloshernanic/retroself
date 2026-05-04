using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Retroself.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public int phasesCompleted;
        public HashSet<string> phasesDoneIds = new HashSet<string>();
        public int collectiblesFound;

        public bool IsPaused { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            SceneManager.sceneLoaded += (s, m) => SetPaused(false);
        }

        public void MarkPhaseComplete(string phaseId)
        {
            if (phasesDoneIds.Add(phaseId))
            {
                phasesCompleted++;
            }
        }

        public bool IsPhaseComplete(string phaseId) => phasesDoneIds.Contains(phaseId);

        public void LoadScene(string sceneName)
        {
            SetPaused(false);
            SceneManager.LoadScene(sceneName);
        }

        public void SetPaused(bool paused)
        {
            IsPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
        }

        public void TogglePause() => SetPaused(!IsPaused);

        public void ResetRun()
        {
            phasesCompleted = 0;
            phasesDoneIds.Clear();
            collectiblesFound = 0;
        }
    }
}
