using UnityEngine;
using System.Collections;
/// <summary>
/// Helper para reiniciar la escena tras un retardo.
/// </summary>
public class DummyRestartHelper : MonoBehaviour {
    public void RestartAfterDelay(int sceneIndex, float delay) {
        StartCoroutine(Restart(sceneIndex, delay));
    }
    IEnumerator Restart(int sceneIndex, float delay) {
        yield return new WaitForSeconds(delay);
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
    }
}