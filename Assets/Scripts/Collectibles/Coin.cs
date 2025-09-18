using UnityEngine;

public class Coin : MonoBehaviour
{
    public int coinValue = 1;
    public AudioClip collectSound;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Add to score
            ScoreManager.instance.AddScore(coinValue);

            // Play sound at coin's position
            if (collectSound != null)
                AudioSource.PlayClipAtPoint(collectSound, transform.position);

            // Destroy coin
            Destroy(gameObject);
        }
    }
}