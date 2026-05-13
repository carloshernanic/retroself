using System.Collections.Generic;
using UnityEngine;

// Inventário global de comidas coletadas pelo jogador na Memory_04 (mercado).
// Espelha o padrão de KeyPickup.Collected (flag estática), mas tipado por
// FoodKind pra que VendorStall possa exigir items específicos.
//
// Cross-cena: estático sobrevive a SceneManager.LoadScene; SceneStartReset
// chama ResetInventory no Awake da cena pra zerar entre fases.
public static class FoodInventory
{
    public static readonly List<FoodKind> Items = new List<FoodKind>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void BootReset() { Items.Clear(); }

    public static void ResetInventory() { Items.Clear(); }

    public static void Add(FoodKind kind) { Items.Add(kind); }

    public static int Count(FoodKind kind)
    {
        int n = 0;
        for (int i = 0; i < Items.Count; i++) if (Items[i] == kind) n++;
        return n;
    }

    public static bool Has(FoodKind kind, int min = 1) { return Count(kind) >= min; }
}

// Tipos de comida pra puzzles. Mapeamento de sprite fica no Memory04Builder
// (asset paths) — script gameplay só conhece o enum.
public enum FoodKind
{
    Burger,
    Sushi,
    Noodle,
    Drink,
    Dessert
}
