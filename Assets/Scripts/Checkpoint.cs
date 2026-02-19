using UnityEngine;

/// <summary>
/// Script para los Checkpoints del nivel.
/// Cuando el jugador lo toca, guarda su posición de respawn.
/// </summary>
public class Checkpoint : MonoBehaviour
{
    private bool isActivated = false;
    public Color activeColor = new Color(0f, 1f, 0.4f, 1f); // Verde brillante

    private void OnTriggerEnter(Collider other)
    {
        if (isActivated) return;

        // Buscar PlayerController tanto en el propio collider como en su jerarquía padre
        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc == null) pc = other.GetComponentInParent<PlayerController>();

        if (pc != null)
        {
            // Guardamos la posición encima del checkpoint para que el jugador
            // aparezca de pie sobre él, no dentro.
            pc.SetCheckpoint(transform.position + Vector3.up * 1.5f);
            isActivated = true;
            ActivateVisual();
        }
    }

    void ActivateVisual()
    {
        Renderer r = GetComponent<Renderer>();
        if (r == null) return;

        // URP o Standard
        if (r.material.HasProperty("_BaseColor"))
            r.material.SetColor("_BaseColor", activeColor);
        else
            r.material.color = activeColor;

        // Emisión para brillo
        r.material.EnableKeyword("_EMISSION");
        if (r.material.HasProperty("_EmissionColor"))
            r.material.SetColor("_EmissionColor", activeColor * 2f);
    }
}
