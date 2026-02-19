using UnityEngine;

/// <summary>
/// Este script garantiza que GameSetup SIEMPRE se ejecute al dar Play, aunque no haya ningún objeto en la escena.
/// Solo debe estar en la carpeta Assets. Se auto-instancia en runtime si no existe GameSetup.
/// </summary>
[DefaultExecutionOrder(-2000)]
public class ForceGameSetup : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void EnsureGameSetup()
    {
        if (Object.FindAnyObjectByType<GameSetup>() == null)
        {
            var go = new GameObject("GameSetup-Auto");
            go.AddComponent<GameSetup>();
            Debug.Log("[ForceGameSetup] GameSetup creado automáticamente antes de la escena.");
        }
    }
}