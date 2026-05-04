using System;

namespace Retroself.Narrative
{
    [Serializable]
    public class DialogueLine
    {
        public string speaker;
        public string text;
        public float pitch = 1f;
    }
}
