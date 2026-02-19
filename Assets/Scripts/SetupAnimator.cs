using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

/// <summary>
/// Script de utilidad para configurar automáticamente el Animator Controller con animaciones de Idle, Walk y Jump.
/// </summary>
public class SetupAnimator : MonoBehaviour
{
    public GameObject characterModel;
    public AnimationClip walkAnimationClip;
    public AnimationClip idleAnimationClip;
    public AnimationClip jumpAnimationClip;

    [ContextMenu("Configurar Animator Automatically")]
    public void Setup()
    {
#if UNITY_EDITOR
        if (characterModel == null)
        {
            Debug.LogError("Asigna el modelo del personaje al script.");
            return;
        }

        // Buscar animaciones si no están asignadas
        if (walkAnimationClip == null) walkAnimationClip = FindClip("Ch24_nonPBR@Walking");
        if (idleAnimationClip == null) idleAnimationClip = FindClip("Idle");
        if (jumpAnimationClip == null) jumpAnimationClip = FindClip("Jump");

        if (walkAnimationClip == null || idleAnimationClip == null || jumpAnimationClip == null)
        {
            Debug.LogError("Faltan clips de animación. Asegúrate de que Walking, Idle y Jump existan.");
            return;
        }

        // 1. Crear el Animator Controller
        string controllerPath = "Assets/Animation/NinjaController.controller";
        if (!AssetDatabase.IsValidFolder("Assets/Animation"))
        {
            AssetDatabase.CreateFolder("Assets", "Animation");
        }

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        // 2. Parámetros
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);

        // 3. Estados
        AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;
        
        AnimatorState idleState = rootStateMachine.AddState("Idle");
        idleState.motion = idleAnimationClip;

        AnimatorState walkState = rootStateMachine.AddState("Walk");
        walkState.motion = walkAnimationClip;

        AnimatorState jumpState = rootStateMachine.AddState("Jump");
        jumpState.motion = jumpAnimationClip;

        // 4. Transiciones
        // Idle <-> Walk
        AnimatorStateTransition walkTransition = idleState.AddTransition(walkState);
        walkTransition.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        walkTransition.duration = 0.25f;

        AnimatorStateTransition idleTransition = walkState.AddTransition(idleState);
        idleTransition.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        idleTransition.duration = 0.25f;

        // Jump desde cualquier estado
        AnimatorStateTransition jumpAnyTransition = rootStateMachine.AddAnyStateTransition(jumpState);
        jumpAnyTransition.AddCondition(AnimatorConditionMode.If, 0, "Jump");
        jumpAnyTransition.duration = 0.1f;

        // Salida de Jump a Idle o Walk basado en IsGrounded y Speed
        AnimatorStateTransition jumpToIdle = jumpState.AddTransition(idleState);
        jumpToIdle.AddCondition(AnimatorConditionMode.If, 0, "IsGrounded");
        jumpToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        jumpToIdle.duration = 0.25f;

        AnimatorStateTransition jumpToWalk = jumpState.AddTransition(walkState);
        jumpToWalk.AddCondition(AnimatorConditionMode.If, 0, "IsGrounded");
        jumpToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        jumpToWalk.duration = 0.25f;

        // 5. Asignar al modelo
        Animator animator = characterModel.GetComponent<Animator>();
        if (animator == null) animator = characterModel.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;

        Debug.Log("¡Animator Controller configurado con Idle, Walk y Jump exitosamente!");
        Selection.activeObject = controller;
#else
        Debug.LogWarning("Este script solo funciona en el Editor de Unity.");
#endif
    }

#if UNITY_EDITOR
    private AnimationClip FindClip(string name)
    {
        string[] animGUIDs = AssetDatabase.FindAssets(name);
        foreach (var guid in animGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip != null) return clip;
        }
        return null;
    }
#endif
}
