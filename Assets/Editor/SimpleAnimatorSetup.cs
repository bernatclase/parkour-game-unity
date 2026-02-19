using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

/// <summary>
/// Crea automáticamente el Animator Controller al cargar el proyecto.
/// Configura los clips de Walking e Idle para que hagan loop y reconstruye
/// el controller con estados Idle, Walk y Jump.
/// </summary>
[InitializeOnLoad]
public class SimpleAnimatorSetup
{
    static SimpleAnimatorSetup()
    {
        EditorApplication.delayCall += () =>
        {
            // NUNCA ejecutar en Play Mode: borrar el controller invalida la referencia del Animator
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureController();
        };
    }

    /// <summary>
    /// Verifica si el controller ya existe con clips válidos; si no, lo crea.
    /// </summary>
    static void EnsureController()
    {
        string controllerPath = "Assets/Animation/CharacterAnimator.controller";
        AnimatorController existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);

        if (existing != null && existing.layers.Length > 0)
        {
            var states = existing.layers[0].stateMachine.states;
            if (states.Length >= 3)
            {
                bool allClipsValid = true;
                foreach (var s in states)
                {
                    if (s.state.motion == null) { allClipsValid = false; break; }
                }
                if (allClipsValid)
                {
                    Debug.Log("[AnimatorSetup] Controller existe con clips válidos. No se recrea.");
                    return;
                }
            }
        }

        CreateController();
    }

    [MenuItem("Tools/Rebuild Character Animator")]
    public static void CreateController()
    {
        string controllerPath = "Assets/Animation/CharacterAnimator.controller";

        // Crear carpeta Animation si no existe
        if (!AssetDatabase.IsValidFolder("Assets/Animation"))
        {
            AssetDatabase.CreateFolder("Assets", "Animation");
        }

        // ── 1. Forzar loop en Walking e Idle ──────────────────────────────
        SetFBXLoopTime("Assets/Scripts/Ch24_nonPBR@Walking.fbx", true);
        SetFBXLoopTime("Assets/Scripts/Idle.fbx", true);
        SetFBXLoopTime("Assets/Scripts/Jump.fbx", false);

        // ── 2. Borrar y recrear controller ────────────────────────────────
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) != null)
        {
            AssetDatabase.DeleteAsset(controllerPath);
        }

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        // Cargar los clips de los FBX
        AnimationClip idleClip = FindFirstClip("Assets/Scripts/Idle.fbx");
        AnimationClip walkClip = FindFirstClip("Assets/Scripts/Ch24_nonPBR@Walking.fbx");
        AnimationClip jumpClip = FindFirstClip("Assets/Scripts/Jump.fbx");

        Debug.Log($"[AnimatorSetup] Clips: Idle={idleClip?.name ?? "NULL"}, Walk={walkClip?.name ?? "NULL"}, Jump={jumpClip?.name ?? "NULL"}");

        // ── 3. Parámetros ─────────────────────────────────────────────────
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);

        // ── 4. Estados ────────────────────────────────────────────────────
        AnimatorStateMachine sm = controller.layers[0].stateMachine;

        AnimatorState idleState = sm.AddState("Idle");
        idleState.motion = idleClip;

        AnimatorState walkState = sm.AddState("Walk");
        walkState.motion = walkClip;

        AnimatorState jumpState = sm.AddState("Jump");
        jumpState.motion = jumpClip;

        sm.defaultState = idleState;

        // ── 5. Transiciones ───────────────────────────────────────────────
        // Idle → Walk  (Speed > 0.1)
        var idleToWalk = idleState.AddTransition(walkState);
        idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        idleToWalk.duration = 0.15f;
        idleToWalk.hasExitTime = false;

        // Walk → Idle  (Speed < 0.1)
        var walkToIdle = walkState.AddTransition(idleState);
        walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        walkToIdle.duration = 0.15f;
        walkToIdle.hasExitTime = false;

        // Any State → Jump  (Jump trigger, solo si está en el suelo)
        var anyToJump = sm.AddAnyStateTransition(jumpState);
        anyToJump.AddCondition(AnimatorConditionMode.If, 0, "Jump");
        anyToJump.duration = 0.1f;
        anyToJump.hasExitTime = false;
        anyToJump.canTransitionToSelf = false;

        // Jump → Idle  (IsGrounded && Speed < 0.1)
        var jumpToIdle = jumpState.AddTransition(idleState);
        jumpToIdle.AddCondition(AnimatorConditionMode.If, 0, "IsGrounded");
        jumpToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        jumpToIdle.duration = 0.2f;
        jumpToIdle.hasExitTime = false;

        // Jump → Walk  (IsGrounded && Speed > 0.1)
        var jumpToWalk = jumpState.AddTransition(walkState);
        jumpToWalk.AddCondition(AnimatorConditionMode.If, 0, "IsGrounded");
        jumpToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        jumpToWalk.duration = 0.2f;
        jumpToWalk.hasExitTime = false;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[AnimatorSetup] ✓ CharacterAnimator.controller creado con Idle (loop), Walk (loop) y Jump");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Configura el loopTime de todos los clips dentro de un FBX
    // ─────────────────────────────────────────────────────────────────────────
    static void SetFBXLoopTime(string fbxPath, bool loop)
    {
        ModelImporter importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
        if (importer == null)
        {
            Debug.LogWarning("[AnimatorSetup] No se encontró FBX en: " + fbxPath);
            return;
        }

        // Obtener los clips por defecto si no hay clips personalizados
        ModelImporterClipAnimation[] clips = importer.clipAnimations;
        if (clips == null || clips.Length == 0)
            clips = importer.defaultClipAnimations;

        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning("[AnimatorSetup] No se encontraron clips en: " + fbxPath);
            return;
        }

        bool changed = false;
        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i].loopTime != loop)
            {
                clips[i].loopTime = loop;
                clips[i].loopPose = loop;
                changed = true;
            }
        }

        if (changed)
        {
            importer.clipAnimations = clips;
            importer.SaveAndReimport();
            Debug.Log($"[AnimatorSetup] Loop={loop} configurado en: {fbxPath}");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Busca el primer AnimationClip dentro de un FBX
    // ─────────────────────────────────────────────────────────────────────────
    static AnimationClip FindFirstClip(string fbxPath)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
        if (assets == null || assets.Length == 0)
        {
            Debug.LogWarning("[AnimatorSetup] FBX no encontrado en: " + fbxPath);
            return null;
        }

        foreach (Object asset in assets)
        {
            if (asset is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                return clip;
        }
        return null;
    }
}

