using UnityEngine;

/// <summary>
/// Configura automáticamente el Animator con el CharacterAnimator controller.
/// Se ejecuta en Awake para que las animaciones funcionen directamente.
/// </summary>
public class AnimatorSetup : MonoBehaviour
{
    void Awake()
    {
        // Obtener el Animator del jugador
        Animator animator = GetComponent<Animator>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator == null)
        {
            Debug.LogError("[AnimatorSetup] No Animator found on " + gameObject.name);
            return;
        }

        // Cargar el controller que ya existe en Assets/Animation/
        RuntimeAnimatorController controller = Resources.Load<RuntimeAnimatorController>("Animation/CharacterAnimator");
        
        // Si no está en Resources, intentar desde el path directo
        if (controller == null)
        {
            #if UNITY_EDITOR
            controller = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animation/CharacterAnimator.controller");
            #endif
        }

        if (controller != null)
        {
            animator.runtimeAnimatorController = controller;
            Debug.Log("[AnimatorSetup] ✓ CharacterAnimator.controller asignado correctamente");
        }
        else
        {
            Debug.LogError("[AnimatorSetup] ❌ CharacterAnimator.controller no encontrado. Crea uno manualmente en Assets/Animation/");
        }
    }
}
