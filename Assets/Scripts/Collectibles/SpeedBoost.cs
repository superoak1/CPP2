using UnityEngine;

public class SpeedBoostCollectible : MonoBehaviour
{
    [SerializeField] private float speedMultiplier = 2f;
    [SerializeField] private float boostDuration = 5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ThirdPersonController controller = other.GetComponent<ThirdPersonController>();

            if (controller != null)
            {
                controller.ApplySpeedBoost(speedMultiplier, boostDuration);
            }

            Destroy(gameObject);
        }
    }
}