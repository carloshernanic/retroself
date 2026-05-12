using UnityEngine;

// Anexado ao Bully: quando o EnemyHealth dispara OnDefeated, instancia uma chave
// no chão na posição do inimigo. Captura a posição no momento da morte porque o
// GameObject do bully é destruído logo em seguida (em EnemyHealth.TakeDamage).
[RequireComponent(typeof(EnemyHealth))]
public class KeyDropper : MonoBehaviour
{
    // Offset relativo ao pivot do inimigo. Bully tem pivot ~no meio do corpo;
    // y=-0.3 deixa a chave próxima dos pés sem afundar no chão.
    public Vector3 dropOffset = new Vector3(0f, -0.3f, 0f);

    private EnemyHealth health;

    void Awake()
    {
        health = GetComponent<EnemyHealth>();
        if (health != null) health.OnDefeated += DropKey;
    }

    void OnDestroy()
    {
        if (health != null) health.OnDefeated -= DropKey;
    }

    void DropKey()
    {
        Vector3 pos = transform.position + dropOffset;
        KeyPickup.Spawn(pos);
    }
}
