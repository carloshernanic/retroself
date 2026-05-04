using UnityEngine;

namespace Retroself.Core
{
    public class InputReader : MonoBehaviour
    {
        public static InputReader Instance { get; private set; }

        public float Horizontal { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool JumpHeld { get; private set; }
        public bool InteractPressed { get; private set; }
        public bool FreezePressed { get; private set; }
        public bool FreezeHeld { get; private set; }
        public bool SwitchPressed { get; private set; }
        public bool PausePressed { get; private set; }
        public bool AdvanceDialoguePressed { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Update()
        {
            float h = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h += 1f;
            Horizontal = h;

            JumpPressed = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
            JumpHeld = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
            InteractPressed = Input.GetKeyDown(KeyCode.E);
            FreezePressed = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
            FreezeHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            SwitchPressed = Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Q);
            PausePressed = Input.GetKeyDown(KeyCode.Escape);
            AdvanceDialoguePressed = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E);
        }
    }
}
