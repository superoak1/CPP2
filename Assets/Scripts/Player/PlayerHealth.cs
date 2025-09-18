using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;
    private bool isInvincible;
    private Coroutine invRoutine;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void ApplyInvincibility(float duration)
    {
        if (invRoutine != null) StopCoroutine(invRoutine);
        invRoutine = StartCoroutine(InvincibilityTimer(duration));
    }

    private IEnumerator InvincibilityTimer(float duration)
    {
        isInvincible = true;
        yield return new WaitForSeconds(duration);
        isInvincible = false;
        invRoutine = null;
    }

    public void TakeDamage(int amount)
    {
        if (isInvincible) return;
        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        if (CurrentHealth == 0)
        {
            // Example: ask a PlayerRespawner in the scene to respawn us
            var respawner = FindFirstObjectByType<PlayerRespawner>();
            if (respawner) respawner.RespawnPlayer(gameObject);
        }
    }

    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + Mathf.Abs(amount));
    }
}
