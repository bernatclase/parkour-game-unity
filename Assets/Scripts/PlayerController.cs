using UnityEngine;

/// <summary>
/// Controlador 3D del jugador con sistema de Checkpoints y detección de vacío.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed    = 10f;
    public float jumpForce    = 14f;
    public float rotationSpeed = 800f;
    public float acceleration = 30f;
    public float gravityScale = 4f;

    [Header("Detección de Suelo")]
    public float groundCheckOffset   = 0.5f;
    public float groundCheckRadius   = 0.4f;
    public float groundCheckDistance = 0.7f;
    public LayerMask groundLayer;

    // Componentes
    private Rigidbody  rb;
    private Animator   animator;

    // Estado de movimiento
    private bool    isGrounded;
    private Vector3 moveDirection;
    private bool    isJumping = false;

    // Sistema de checkpoint
    private Vector3 checkpointPosition;

    // Escudo
    private bool     hasShield = false;
    private Material playerMaterial;
    private Color    baseColor;

    // Muerte / respawn
    private bool isHandlingDeath = false;

    // Sistema de entrada
    private bool useNewInput = false;

    // ─────────────────────────────────────────────────────────────────────────
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.useGravity     = false;

        // El punto de inicio ES el primer checkpoint
        checkpointPosition = transform.position;

        // Animator (puede estar en un hijo)
        animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        // Material (puede estar en un hijo)
        Renderer rend = GetComponent<Renderer>();
        if (rend == null) rend = GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            playerMaterial = rend.material;
            baseColor      = playerMaterial.color;
        }

        // GroundLayer por defecto si no está configurado en el Inspector
        if (groundLayer == 0)
        {
            // Incluir capas comunes de suelo
            groundLayer = LayerMask.GetMask("Default", "Ground", "Platforms");
            // Si ninguna de esas capas existe, detectar todo
            if (groundLayer == 0) groundLayer = ~0;
            // NOTA: NO excluimos la capa del jugador aquí porque si el jugador
            // también está en 'Default', la máscara quedaría a 0 y nunca detectaría suelo.
            // La cápsula del jugador ya queda fuera del SphereCast por geometría.
        }

#if UNITY_INPUT_SYSTEM || ENABLE_INPUT_SYSTEM
        useNewInput = true;
#endif
    }

    // ─────────────────────────────────────────────────────────────────────────
    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        HandleInput();

        // Detección de suelo
        Vector3 origin = transform.position + Vector3.up * groundCheckOffset;
        isGrounded = Physics.SphereCast(origin, groundCheckRadius, Vector3.down, out _, groundCheckDistance, groundLayer);

        if (isGrounded && rb.linearVelocity.y <= 0.1f)
            isJumping = false;

        // Salto
        if (GetJumpInput() && isGrounded && !isJumping)
        {
            Jump();
            isGrounded = false; // Forzar false este frame para que el Animator no salga de Jump inmediatamente
        }

        // Rotación hacia la dirección de movimiento
        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // Animación
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            float speedPercent = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude / moveSpeed;
            animator.SetFloat("Speed",      speedPercent);
            animator.SetBool("IsGrounded",  isGrounded);
        }

        // La detección de caída al vacío la gestiona exclusivamente KillZoneTrigger
        // para evitar que se llame FallDeath() dos veces (una por posición Y
        // y otra por el trigger físico), lo que causaba perder 2 vidas de golpe.
    }

    // ─────────────────────────────────────────────────────────────────────────
    void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        Vector3 targetVel = moveDirection * moveSpeed;
        Vector3 vel       = rb.linearVelocity;

        vel.x  = Mathf.Lerp(vel.x, targetVel.x, acceleration * Time.fixedDeltaTime);
        vel.z  = Mathf.Lerp(vel.z, targetVel.z, acceleration * Time.fixedDeltaTime);
        vel.y += Physics.gravity.y * gravityScale * Time.fixedDeltaTime;

        rb.linearVelocity = vel;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // INPUT
    // ─────────────────────────────────────────────────────────────────────────
    void HandleInput()
    {
        float h = 0f, v = 0f;

        if (useNewInput)
        {
#if UNITY_INPUT_SYSTEM || ENABLE_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null)
            {
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  h -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) h += 1f;
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed)    v += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed)  v -= 1f;
            }
#endif
        }
        else
        {
            h = Input.GetAxisRaw("Horizontal");
            v = Input.GetAxisRaw("Vertical");
        }

        moveDirection = new Vector3(h, 0, v).normalized;
    }

    bool GetJumpInput()
    {
        if (useNewInput)
        {
#if UNITY_INPUT_SYSTEM || ENABLE_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            return kb != null && kb.spaceKey.wasPressedThisFrame;
#endif
        }
        return Input.GetKeyDown(KeyCode.Space);
    }

    void Jump()
    {
        Vector3 vel = rb.linearVelocity;
        vel.y = jumpForce;
        rb.linearVelocity = vel;
        isJumping = true;
        if (animator != null && animator.runtimeAnimatorController != null) animator.SetTrigger("Jump");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CHECKPOINT
    // ─────────────────────────────────────────────────────────────────────────
    public void SetCheckpoint(Vector3 newPos)
    {
        checkpointPosition = newPos;
        Debug.Log("<color=cyan>[Checkpoint]</color> Guardado en: " + newPos);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // RESPAWN
    // ─────────────────────────────────────────────────────────────────────────
    public void Respawn()
    {
        // Parar completamente al personaje
        rb.linearVelocity  = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        moveDirection      = Vector3.zero;

        // Teletransportar al checkpoint (o inicio)
        transform.position = checkpointPosition;

        isJumping = false;

        // Quitar escudo al morir
        hasShield = false;
        UpdateShieldVisuals();

        // Pequeño impulso para no hundirse en la plataforma
        rb.AddForce(Vector3.up * 2f, ForceMode.Impulse);

        Debug.Log("<color=yellow>[Player]</color> Respawn en: " + checkpointPosition);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // MUERTE POR VACÍO
    // ─────────────────────────────────────────────────────────────────────────
    public void FallDeath()
    {
        if (isHandlingDeath) return;
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        isHandlingDeath = true;
        Debug.Log("<color=red>[Void]</color> Caída al vacío.");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseLife();

            if (!GameManager.Instance.isGameOver)
                Respawn();
            // Si es game over, GameManager muestra la pantalla final
        }
        else
        {
            // Sin GameManager, siempre respawn
            Respawn();
        }

        // El flag se resetea después de medio segundo para evitar doble trigger
        Invoke(nameof(ResetDeathFlag), 0.5f);
    }

    void ResetDeathFlag() => isHandlingDeath = false;

    // ─────────────────────────────────────────────────────────────────────────
    // DETECCIÓN DE ENEMIGOS (OnTriggerEnter)
    // ─────────────────────────────────────────────────────────────────────────
    void OnTriggerEnter(Collider other)
    {
        // IMPORTANTE: usar other.tag == "..." en vez de CompareTag() porque
        // CompareTag() lanza excepción si el tag no está definido en el Tag Manager.
        // La comparación directa nunca falla.
        if (other.tag == "Enemy")
        {
            if (hasShield)
            {
                hasShield = false;
                UpdateShieldVisuals();
                if (UIManager.Instance != null) UIManager.Instance.UpdateUI();
                Debug.Log("<color=cyan>[Shield]</color> Escudo ha bloqueado el golpe.");
            }
            else
            {
                FallDeath();
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ESCUDO
    // ─────────────────────────────────────────────────────────────────────────
    public void GainShield()
    {
        hasShield = true;
        UpdateShieldVisuals();
        if (UIManager.Instance != null) UIManager.Instance.UpdateUI();
    }

    public bool HasShield() => hasShield;

    void UpdateShieldVisuals()
    {
        if (playerMaterial != null)
            playerMaterial.color = hasShield ? new Color(0f, 0.7f, 1f) : baseColor;
    }
}
