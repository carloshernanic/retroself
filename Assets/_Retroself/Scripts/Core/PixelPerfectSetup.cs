using UnityEngine;

namespace Retroself.Core
{
    [RequireComponent(typeof(Camera))]
    public class PixelPerfectSetup : MonoBehaviour
    {
        public int referenceWidth = 320;
        public int referenceHeight = 180;
        public int pixelsPerUnit = 16;

        Camera cam;

        void Awake()
        {
            cam = GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = (referenceHeight / 2f) / pixelsPerUnit;
            cam.backgroundColor = new Color(0.05f, 0.06f, 0.10f, 1f);
        }
    }
}
