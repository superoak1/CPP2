using UnityEngine;

public class Collectible : MonoBehaviour
{
    [HideInInspector] public RandomSpawner spawner;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Notify spawner and destroy self
            if (spawner != null)
            {
                spawner.DestroySpawner();
            }

            Destroy(gameObject);
        }
    }
}