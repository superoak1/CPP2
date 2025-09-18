using UnityEngine;

public class InvincibilityCollectible : MonoBehaviour
{
    [SerializeField] private float invincibilityDuration = 5f;

    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.ApplyInvincibility(invincibilityDuration);
            Destroy(gameObject); // remove the collectible
        }
    }
}