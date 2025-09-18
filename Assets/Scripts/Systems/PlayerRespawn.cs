using UnityEngine;

public class PlayerRespawner : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;

    public void RespawnPlayer(GameObject player)
    {
        if (player == null || spawnPoint == null)
        {
            Debug.LogWarning("PlayerRespawner: Missing player or spawnPoint.");
            return;
        }

        // Move player safely
        var cc = player.GetComponent<CharacterController>();
        if (cc && cc.enabled)
        {
            cc.enabled = false; // avoid snapping issues
            player.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            cc.enabled = true;
        }
        else
        {
            player.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        }

        // Reset Rigidbody movement if present
        var rb = player.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.linearVelocity = Vector3.zero;     // ✅ Unity 6 safe
            rb.angularVelocity = Vector3.zero;
        }

        // Restore health (simple full heal)
        var hp = player.GetComponent<PlayerHealth>();
        if (hp != null)
        {
            hp.Heal(9999);
        }
    }
}
