using UnityEngine;

namespace Retroself.Core
{
    public static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            var go = new GameObject("[Retroself.Systems]");
            go.AddComponent<GameManager>();
            go.AddComponent<InputReader>();
            go.AddComponent<global::Retroself.Audio.AudioManager>();
            Object.DontDestroyOnLoad(go);
        }
    }
}
