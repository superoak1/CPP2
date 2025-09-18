using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] spawnOptions;

    private void Start()
    {
        SpawnRandom();
    }

    public void SpawnRandom()
    {
        if (spawnOptions.Length == 0)
        {
            Debug.LogWarning("No spawn options assigned.");
            return;
        }

        int index = Random.Range(0, spawnOptions.Length);

        // Instantiate the selected prefab
        GameObject spawned = Instantiate(spawnOptions[index], transform.position, Quaternion.identity);

        // If it has a Collectible component, tell it who the spawner is
        Collectible collectible = spawned.GetComponent<Collectible>();
        if (collectible != null)
        {
            collectible.spawner = this;
        }
    }

    public void DestroySpawner()
    {
        Destroy(gameObject);
    }
}