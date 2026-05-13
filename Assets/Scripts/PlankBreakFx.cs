using UnityEngine;

// Spawn de partículas de madeira + som quando a BreakablePlank é destruída.
// Subscreve em EnemyHealth.OnDefeated (rodando ANTES do Destroy(gameObject))
// e instancia um ParticleSystem independente que sobrevive ao parent ser deletado.
[RequireComponent(typeof(EnemyHealth))]
public class PlankBreakFx : MonoBehaviour
{
    public Color particleColor = new Color(0.45f, 0.30f, 0.18f, 1f);
    public int particleCount = 16;
    public float burstRadius = 0.35f;

    void Awake()
    {
        var hp = GetComponent<EnemyHealth>();
        hp.OnDefeated += SpawnDebris;
    }

    void SpawnDebris()
    {
        // GO inativo → AddComponent não dispara auto-play → podemos setar duration
        // e burstCount sem warnings. Ativa no fim pro PS começar.
        var go = new GameObject("PlankDebris");
        go.SetActive(false);
        go.transform.position = transform.position;

        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.5f;
        main.loop = false;
        main.playOnAwake = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2.5f, 5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
        main.startColor = particleColor;
        main.gravityModifier = 1.6f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.burstCount = 1; // precisa setar ANTES de SetBurst(0, ...).
        emission.SetBurst(0, new ParticleSystem.Burst(0f, particleCount));

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = burstRadius;

        // Cor varia ao longo da vida pra escurecer no fim (madeira caindo no chão).
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] {
                new GradientColorKey(particleColor, 0f),
                new GradientColorKey(particleColor * 0.5f, 1f)
            },
            new[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 0.7f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = grad;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.sortingOrder = 10;

        go.SetActive(true);
        SfxBeep.PlayBreak();
        Destroy(go, 2f);
    }
}
