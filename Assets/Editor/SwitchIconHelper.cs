#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// Helper compartilhado pelos builders Memory_02/Memory_03: anexa um ícone "alvo"
// (cyberpunk-skills-pixelated-icon-pack/1 Icons/N/Skill-icons_04.png) e um
// componente StoneSwitchIcon que pisca a variante "glitch" (folder 14) quando o
// switch ativa. Cada cor (Yellow/Red/Green/Blue/Purple) mapeia pra uma pasta da
// pack — a pasta 14 é sempre a glitch.
public static class SwitchIconHelper
{
    public enum Color { Yellow, Red, Green, Blue, Purple }

    const string IconBase = "Assets/Sprites/cyberpunk-skills-pixelated-icon-pack/1 Icons";
    const string IconFile = "Skill-icons_28.png"; // illuminati eye — mesma figura nas 13 cores + glitch

    public static void Attach(StoneSwitch sw, Color color)
    {
        if (sw == null) return;
        int folder = FolderFor(color);
        var normal = LoadIcon(folder);
        var glitch = LoadIcon(14);
        if (normal == null || glitch == null)
        {
            Debug.LogWarning($"[SwitchIconHelper] Ícone não encontrado (folder {folder} ou 14). " +
                             $"Rode 'Retroself → Configure Scene Art Imports' uma vez pra reimportar.");
            return;
        }

        var iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(sw.transform, false);
        iconGO.transform.localPosition = Vector3.zero;
        iconGO.transform.localScale = Vector3.one;

        var sr = iconGO.AddComponent<SpriteRenderer>();
        sr.sprite = normal;
        sr.sortingOrder = 7; // acima do quadrado (sortingOrder 6) do StoneSwitch.

        var ic = sw.gameObject.AddComponent<StoneSwitchIcon>();
        ic.iconRenderer = sr;
        ic.normalSprite = normal;
        ic.glitchSprite = glitch;
    }

    static int FolderFor(Color c)
    {
        switch (c)
        {
            case Color.Red:    return 11;
            case Color.Green:  return 6;
            case Color.Blue:   return 13;
            case Color.Purple: return 8;
            default:           return 1; // Yellow
        }
    }

    static Sprite LoadIcon(int folder)
    {
        var path = $"{IconBase}/{folder}/{IconFile}";
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
#endif
