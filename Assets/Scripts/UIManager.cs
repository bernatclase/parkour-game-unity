using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Gestiona toda la interfaz de usuario del juego.
/// Muestra vidas, monedas, escudo, mensajes de victoria y derrota.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Textos")]
    public Text livesText;
    public Text coinsText;
    public Text messageText;
    public Text shieldText;
    public GameObject restartButton;

    private Coroutine pulseCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Ocultar elementos que deben estar ocultos al inicio
        SafeSetActive(restartButton, false);
        if (messageText != null) messageText.gameObject.SetActive(false);
    }

    void Start()
    {
        // Esperar un frame para asegurarnos de que GameManager ya existe
        StartCoroutine(DelayedInit());
    }

    IEnumerator DelayedInit()
    {
        yield return null; // esperar un frame
        UpdateUI();

        if (restartButton != null)
        {
            Button btn = restartButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(OnRestartButton);
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ACTUALIZAR UI
    // ─────────────────────────────────────────────────────────────────────────
    public void UpdateUI()
    {
        if (GameManager.Instance == null) return;

        // --- Vidas ---
        if (livesText != null)
        {
            string hearts = "";
            for (int i = 0; i < GameManager.Instance.currentLives; i++)
                hearts += "♥ ";
            livesText.text = hearts.TrimEnd();
            if (livesText.text == "") livesText.text = "♥ 0";
            TriggerPulse(livesText.gameObject);
        }

        // --- Monedas ---
        if (coinsText != null)
        {
            coinsText.text = "★ " + GameManager.Instance.currentCoins + " / " + GameManager.Instance.coinsToWin;
            TriggerPulse(coinsText.gameObject);
        }

        // --- Escudo ---
        if (shieldText != null)
        {
            PlayerController player = FindAnyObjectByType<PlayerController>();
            if (player != null && player.HasShield())
            {
                shieldText.text  = "🛡 ESCUDO ACTIVO";
                shieldText.color = new Color(0.5f, 0.8f, 1f);
            }
            else
            {
                shieldText.text = "";
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // WIN / GAME OVER
    // ─────────────────────────────────────────────────────────────────────────
    public void ShowWin()
    {
        if (messageText != null)
        {
            messageText.gameObject.SetActive(true);
            messageText.text  = "¡¡HAS GANADO!!";
            messageText.color = Color.yellow;
            StartCoroutine(AnimateWinText());
        }
        SafeSetActive(restartButton, true);
    }

    public void ShowGameOver()
    {
        if (messageText != null)
        {
            messageText.gameObject.SetActive(true);
            messageText.text  = "GAME OVER";
            messageText.color = Color.red;
        }
        SafeSetActive(restartButton, true);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BOTÓN REINICIAR
    // ─────────────────────────────────────────────────────────────────────────
    public void OnRestartButton()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RestartGame();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ANIMACIONES
    // ─────────────────────────────────────────────────────────────────────────
    void TriggerPulse(GameObject target)
    {
        if (target == null) return;
        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
        pulseCoroutine = StartCoroutine(DoPulse(target));
    }

    IEnumerator DoPulse(GameObject target)
    {
        RectTransform rect = target.GetComponent<RectTransform>();
        if (rect == null) yield break;

        float elapsed = 0f;
        float duration = 0.25f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float scale = Mathf.Lerp(1.25f, 1f, t);
            rect.localScale = Vector3.one * scale;
            yield return null;
        }
        rect.localScale = Vector3.one;
    }

    IEnumerator AnimateWinText()
    {
        if (messageText == null) yield break;
        RectTransform rect = messageText.GetComponent<RectTransform>();
        float elapsed = 0f;
        float duration = 0.6f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float scale = 1f + Mathf.Sin(elapsed * 6f) * 0.15f;
            rect.localScale = Vector3.one * scale;
            yield return null;
        }
        rect.localScale = Vector3.one;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // HELPER
    // ─────────────────────────────────────────────────────────────────────────
    void SafeSetActive(GameObject go, bool active)
    {
        if (go != null) go.SetActive(active);
    }
}
