using UnityEngine;

/// <summary>
/// Enemigo simple que patrulla entre dos puntos.
/// Asignar tag "Enemy" al GameObject.
/// </summary>
public class EnemyPatrol : MonoBehaviour
{
    public float speed = 3f;
    public float patrolDistance = 4f;
    public float rotationSpeed = 360f;
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.1f;

    private Vector3 startPos;
    private Vector3 originalScale;
    private int direction = 1;

    void Start()
    {
        startPos = transform.position;
        originalScale = transform.localScale;
    }

    void Update()
    {
        // Movimiento
        transform.position += Vector3.right * (direction * speed * Time.deltaTime);

        // Rotación continua
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        // Pulso visual (expansión y contracción)
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = originalScale * pulse;

        // Cambiar dirección al llegar al límite
        if (Vector3.Distance(startPos, transform.position) >= patrolDistance)
        {
            direction *= -1;
        }
    }
}
