using UnityEngine;

/// <summary>
/// Si no existe un objeto con GameSetup en la escena, lo crea automáticamente y lo ejecuta.
/// Solo necesitas tener este script en cualquier objeto o en la carpeta Assets.
/// </summary>
[DefaultExecutionOrder(-1000)]
public class AutoGameSetup : MonoBehaviour
{
    void Awake()
    {
        if (FindAnyObjectByType<GameSetup>() == null)
        {
            GameObject go = new GameObject("GameSetup");
            go.AddComponent<GameSetup>();
            Debug.Log("[AutoGameSetup] GameSetup creado automáticamente.");
        }
    }
}
