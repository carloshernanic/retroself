using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HazardZone : MonoBehaviour
{
    public int damage = 99;

    void Reset()
    {
        var c = GetComponent<Collider2D>();
        if (c != null) c.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var hp = other.GetComponent<PlayerHealth>();
        if (hp != null) hp.TakeDamage(damage, transform.position);
    }
}
