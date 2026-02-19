using UnityEngine;

/// <summary>
/// Anima la estrella de la meta con rotación y movimiento orbital.
/// </summary>
public class GoalStarAnimation : MonoBehaviour
{
    [Header("Rotación")]
    public float rotationSpeed = 180f;

    [Header("Movimiento orbital")]
    public float orbitSpeed = 2f;
    public float orbitRadius = 2f;
    
    [Header("Pulso")]
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.2f;

    private Vector3 startPosition;
    private Vector3 orbitCenter;

    void Start()
    {
        startPosition = transform.position;
        orbitCenter = new Vector3(startPosition.x, startPosition.y - 1.5f, startPosition.z);
    }

    void Update()
    {
        // Rotación en todos los ejes
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        transform.Rotate(Vector3.right, rotationSpeed * 0.5f * Time.deltaTime, Space.Self);

        // Órbita alrededor del punto central
        float angle = Time.time * orbitSpeed;
        Vector3 orbitalPos = orbitCenter + new Vector3(
            Mathf.Cos(angle) * orbitRadius,
            Mathf.Sin(Time.time * pulseSpeed * 0.5f) * 0.3f,
            Mathf.Sin(angle) * orbitRadius
        );
        transform.position = orbitalPos;

        // Pulso de escala
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = Vector3.one * 1.5f * pulse;
    }
}
