using UnityEngine;

// Comida coletável da Memory_04. Diferente de KeyPickup (flag global única),
// cada FoodPickup tem um `kind` e adiciona em FoodInventory.Items. VendorStall
// consulta o inventário pra destravar.
[RequireComponent(typeof(Collider2D))]
public class FoodPickup : MonoBehaviour
{
    public FoodKind kind = FoodKind.Burger;

    void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() == null) return;
        FoodInventory.Add(kind);
        SfxBeep.PlayFoodPickup();
        Destroy(gameObject);
    }
}
