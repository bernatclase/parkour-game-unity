using UnityEngine;
using System.Collections;

/// <summary>
/// Sistema de escudos que protegen al jugador contra un golpe.
/// Se puede recoger en plataformas y se visualiza en el jugador.
/// </summary>
public class Shield : MonoBehaviour
{
    [Header("Configuración")]
    public float rotationSpeed = 180f;
    public float bobSpeed = 2f;
    public float bobAmount = 0.1f;
    
    private Vector3 startPosition;
    private bool isCollected = false;

    void Awake()
    {
        // Asegurar que el collider sea trigger
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            // Crear un SphereCollider si no existe
            col = gameObject.AddComponent<SphereCollider>();
        }
        col.isTrigger = true;
        
        // Log para debugging
        Debug.Log($"Shield creado en {transform.position}");
    }

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (isCollected)
            return;

        // Rotación visual
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);

        // Movimiento ondulante
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected)
            return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            Debug.Log("¡Escudo recogido!");
            player.GainShield();
            isCollected = true;
            
            // Efecto visual: escala y desaparición
            StartCoroutine(CollectAnimation());
        }
    }

    private IEnumerator CollectAnimation()
    {
        float duration = 0.3f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = startScale * (1f - t);
            yield return null;
        }

        Destroy(gameObject);
    }
}
