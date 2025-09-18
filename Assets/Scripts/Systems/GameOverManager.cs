// Replace your whole GameOverManager.cs with this merged version
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject gameOverUI;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private GameObject lastPlayer;
    private bool isShown;

    public void ShowGameOver(GameObject player) { lastPlayer = player; InternalShow(); }
    public void ShowGameOver() { InternalShow(); }

    private void InternalShow()
    {
        if (isShown) return;
        isShown = true;
        if (gameOverUI) gameOverUI.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void HideGameOver()
    {
        if (!isShown) return;
        isShown = false;
        if (gameOverUI) gameOverUI.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Respawn()
    {
        HideGameOver();
        var respawner = FindFirstObjectByType<PlayerRespawner>(FindObjectsInactive.Include);
        if (!respawner)
        {
            Debug.LogWarning("GameOverManager: No PlayerRespawner found.");
            return;
        }
        if (!lastPlayer)
        {
            var tpc = FindFirstObjectByType<ThirdPersonController>(FindObjectsInactive.Include);
            lastPlayer = tpc ? tpc.gameObject : GameObject.FindGameObjectWithTag("Player");
        }
        if (!lastPlayer)
        {
            Debug.LogWarning("GameOverManager: No player to respawn.");
            return;
        }
        respawner.RespawnPlayer(lastPlayer);
    }

    public void TryAgain()
    {
        HideGameOver();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        HideGameOver();
        if (string.IsNullOrEmpty(mainMenuSceneName))
        {
            Debug.LogWarning("GameOverManager: mainMenuSceneName is empty.");
            return;
        }
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        HideGameOver();
        Application.Quit();
        Debug.Log("Quit requested.");
    }

    // 👇 Add this to satisfy anything still calling EndGame()
    public void EndGame() => QuitGame();
}
