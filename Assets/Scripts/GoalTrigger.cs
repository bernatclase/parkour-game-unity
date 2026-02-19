using UnityEngine;

/// <summary>
/// Trigger invisible sobre la plataforma de meta.
/// Cuando el jugador pisa la meta, se activa la victoria.
/// </summary>
public class GoalTrigger : MonoBehaviour
{
    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        // Verificar que es el jugador
        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc == null) pc = other.GetComponentInParent<PlayerController>();

        if (pc != null)
        {
            triggered = true;
            Debug.Log("<color=green>[Goal]</color> ¡El jugador ha llegado a la meta!");

            if (GameManager.Instance != null && !GameManager.Instance.isGameOver)
            {
                GameManager.Instance.ReachGoal();
            }
        }
    }
}
