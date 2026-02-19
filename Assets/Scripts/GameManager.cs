using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestor principal del juego. Singleton.
/// Controla vidas, monedas, victoria y derrota.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Configuración")]
    public int maxLives   = 3;
    public int coinsToWin = 20;

    [HideInInspector] public int  currentLives;
    [HideInInspector] public int  currentCoins;
    [HideInInspector] public bool isGameOver;
    [HideInInspector] public bool hasWon;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        currentLives = maxLives;
        currentCoins = 0;
        isGameOver   = false;
        hasWon       = false;
    }

    // ─────────────────────────────────────────────────────────────────────────
    /// <summary>Llamado cuando el jugador recoge una moneda.</summary>
    public void CollectCoin()
    {
        if (isGameOver) return;

        currentCoins++;
        Debug.Log($"Monedas: {currentCoins}/{coinsToWin}");

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateUI();

        if (currentCoins >= coinsToWin)
            WinGame();
    }

    // ─────────────────────────────────────────────────────────────────────────
    /// <summary>Llamado cuando el jugador pierde una vida.</summary>
    public void LoseLife()
    {
        if (isGameOver) return;

        currentLives--;
        Debug.Log($"<color=red>[GameManager]</color> Vidas restantes: {currentLives}");

        if (UIManager.Instance != null)
            UIManager.Instance.UpdateUI();

        if (currentLives <= 0)
            GameOver();
    }

    // ─────────────────────────────────────────────────────────────────────────
    /// <summary>Llamado cuando el jugador llega a la plataforma de meta.</summary>
    public void ReachGoal()
    {
        if (isGameOver) return;
        Debug.Log("<color=green>[GameManager]</color> ¡Meta alcanzada!");
        WinGame();
    }

    // ─────────────────────────────────────────────────────────────────────────
    private void WinGame()
    {
        if (isGameOver) return;
        hasWon     = true;
        isGameOver = true;
        Debug.Log("<color=yellow>[GameManager]</color> ¡¡HAS GANADO!!");

        if (UIManager.Instance != null)
            UIManager.Instance.ShowWin();

        SendGameStatsToServer();
    }

    public void GameOver()
    {
        isGameOver = true;
        hasWon     = false;
        Debug.Log("<color=red>[GameManager]</color> GAME OVER");

        if (UIManager.Instance != null)
            UIManager.Instance.ShowGameOver();

        SendGameStatsToServer();
    }

    // ─────────────────────────────────────────────────────────────────────────
    private void SendGameStatsToServer()
    {
        string status = hasWon ? "VICTORY" : "GAME_OVER";
        Debug.Log($"<color=green>[API]</color> Enviando /api/game/stats → status:{status} coins:{currentCoins} lives:{currentLives}");
    }

    // ─────────────────────────────────────────────────────────────────────────
    /// <summary>Reiniciar la escena actual.</summary>
    public void RestartGame()
    {
        Instance = null; // Permitir que el nuevo Awake registre la nueva instancia
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
