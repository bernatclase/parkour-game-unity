using UnityEngine;

/// <summary>
/// Zona de muerte: cuando el jugador cae al vacío, pierde una vida.
/// Se coloca un plano grande con trigger debajo del nivel.
/// Asignar tag "KillZone".
/// </summary>
public class KillZone : MonoBehaviour
{
    // La detección se hace en PlayerController.OnTriggerEnter
    // Este script solo sirve para identificar el objeto.
}
