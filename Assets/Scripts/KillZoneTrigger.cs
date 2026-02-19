using UnityEngine;

/// <summary>
/// Detecta cuando el jugador entra en la zona de muerte (caída al vacío).
/// Colocado en el trigger invisible MUY por debajo del nivel.
/// </summary>
public class KillZoneTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // Buscar PlayerController tanto en el GO directo como en su padre
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) player = other.GetComponentInParent<PlayerController>();

        if (player != null)
        {
            player.FallDeath();
        }
    }
}
