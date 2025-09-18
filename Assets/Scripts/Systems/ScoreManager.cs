using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    [SerializeField] private TextMeshProUGUI scoreText;
    private int score = 0;

    private void Awake()
    {
        // Singleton setup
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }
        else
        {
            Debug.LogWarning("Score Text reference is missing!");
        }
    }
}