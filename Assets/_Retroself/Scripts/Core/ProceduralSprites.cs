using System.Collections.Generic;
using UnityEngine;

namespace Retroself
{
    public static class ProceduralSprites
    {
        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        public static Sprite GetSquare(int size = 16, Color? color = null)
        {
            Color c = color ?? Color.white;
            string key = $"sq_{size}_{(int)(c.r*255)}_{(int)(c.g*255)}_{(int)(c.b*255)}";
            if (cache.TryGetValue(key, out var s)) return s;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            var pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = c;
            tex.SetPixels(pixels);
            tex.Apply();
            s = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
            cache[key] = s;
            return s;
        }

        public static Sprite GetRect(int w, int h, Color? color = null)
        {
            Color c = color ?? Color.white;
            string key = $"rc_{w}_{h}_{(int)(c.r*255)}_{(int)(c.g*255)}_{(int)(c.b*255)}";
            if (cache.TryGetValue(key, out var s)) return s;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var pixels = new Color[w * h];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = c;
            tex.SetPixels(pixels);
            tex.Apply();
            s = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16f);
            cache[key] = s;
            return s;
        }

        public static Sprite GetCircle(int size = 16, Color? color = null)
        {
            Color c = color ?? Color.white;
            string key = $"ci_{size}_{(int)(c.r*255)}_{(int)(c.g*255)}_{(int)(c.b*255)}";
            if (cache.TryGetValue(key, out var s)) return s;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            float r = size / 2f - 0.5f;
            Vector2 mid = new Vector2(size / 2f - 0.5f, size / 2f - 0.5f);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), mid);
                    tex.SetPixel(x, y, d <= r ? c : Color.clear);
                }
            tex.Apply();
            s = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
            cache[key] = s;
            return s;
        }

        public static Sprite GetWoodyAdult()
        {
            const string key = "woody_adult";
            if (cache.TryGetValue(key, out var s)) return s;
            int W = 16, H = 32;
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color skin = new Color(0.85f, 0.7f, 0.55f);
            Color coat = new Color(0.42f, 0.30f, 0.22f);
            Color pants = new Color(0.20f, 0.18f, 0.18f);
            Color hair = new Color(0.18f, 0.12f, 0.10f);
            Color eye = new Color(0.05f, 0.05f, 0.05f);
            Color clear = Color.clear;
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++) tex.SetPixel(x, y, clear);
            // pants
            for (int y = 0; y < 10; y++) for (int x = 4; x < 12; x++) tex.SetPixel(x, y, pants);
            // coat
            for (int y = 10; y < 22; y++) for (int x = 3; x < 13; x++) tex.SetPixel(x, y, coat);
            // head
            for (int y = 22; y < 30; y++) for (int x = 5; x < 11; x++) tex.SetPixel(x, y, skin);
            // hair
            for (int y = 28; y < 31; y++) for (int x = 4; x < 12; x++) tex.SetPixel(x, y, hair);
            // eyes
            tex.SetPixel(7, 26, eye); tex.SetPixel(9, 26, eye);
            tex.Apply();
            s = Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0f), 16f);
            cache[key] = s;
            return s;
        }

        public static Sprite GetWoodyYoung()
        {
            const string key = "woody_young";
            if (cache.TryGetValue(key, out var s)) return s;
            int W = 16, H = 16;
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color skin = new Color(0.95f, 0.78f, 0.6f);
            Color shirt = new Color(0.95f, 0.85f, 0.25f);
            Color pants = new Color(0.30f, 0.45f, 0.85f);
            Color hair = new Color(0.25f, 0.18f, 0.12f);
            Color eye = Color.black;
            Color clear = Color.clear;
            for (int y = 0; y < H; y++) for (int x = 0; x < W; x++) tex.SetPixel(x, y, clear);
            for (int y = 0; y < 5; y++) for (int x = 5; x < 11; x++) tex.SetPixel(x, y, pants);
            for (int y = 5; y < 10; y++) for (int x = 4; x < 12; x++) tex.SetPixel(x, y, shirt);
            for (int y = 10; y < 14; y++) for (int x = 5; x < 11; x++) tex.SetPixel(x, y, skin);
            for (int y = 13; y < 15; y++) for (int x = 5; x < 12; x++) tex.SetPixel(x, y, hair);
            tex.SetPixel(7, 12, eye); tex.SetPixel(9, 12, eye);
            tex.Apply();
            s = Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0f), 16f);
            cache[key] = s;
            return s;
        }
    }
}
