using UnityEngine;

public class GameOverTrigger : MonoBehaviour
{
    [SerializeField] private GameOverManager gameOverManager;

    private void Awake()
    {
        if (gameOverManager == null)
            gameOverManager = FindFirstObjectByType<GameOverManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && gameOverManager != null)
        {
            gameOverManager.ShowGameOver(other.gameObject);
        }
    }
}
