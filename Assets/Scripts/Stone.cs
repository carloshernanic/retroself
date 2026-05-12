using UnityEngine;

public class Stone : MonoBehaviour
{
    public float speed = 12f;
    public float lifetime = 1.5f;
    public int damage = 1;

    private Vector2 direction = Vector2.right;
    private float timer;

    public void Launch(Vector2 dir)
    {
        direction = dir.normalized;
        if (direction.sqrMagnitude < 0.01f) direction = Vector2.right;
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
        timer += Time.deltaTime;
        if (timer >= lifetime) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) return;
        var hp = other.GetComponent<EnemyHealth>();
        if (hp != null) hp.TakeDamage(damage, transform.position);
        Destroy(gameObject);
    }
}
