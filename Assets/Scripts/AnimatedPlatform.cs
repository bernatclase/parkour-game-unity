using UnityEngine;

/// <summary>
/// Anima plataformas con rotación suave y movimiento ondulante.
/// </summary>
public class AnimatedPlatform : MonoBehaviour
{
    [Header("Rotación")]
    public float rotationSpeed = 5f;
    
    [Header("Ondulación")]
    public float bobSpeed = 2f;
    public float bobAmount = 0.05f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // Rotación lenta
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);

        // Movimiento ondulante
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
}
