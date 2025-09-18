using UnityEngine;
using UnityEngine.AI;

public class GoblinHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 50;
    [SerializeField] private float cleanupDelay = 5f;

    public int CurrentHealth { get; private set; }

    private Animator anim;
    private NavMeshAgent agent;
    private GoblinAI ai;
    private Collider[] colliders;
    private bool isDead;

    void Awake()
    {
        CurrentHealth = maxHealth;
        anim = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        ai = GetComponent<GoblinAI>();
        colliders = GetComponentsInChildren<Collider>();
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;
        CurrentHealth = Mathf.Max(0, CurrentHealth - Mathf.Abs(amount));
        if (CurrentHealth == 0) Die();
    }

    private void Die()
    {
        isDead = true;

        // stop all motion/AI
        if (agent) { agent.isStopped = true; agent.ResetPath(); }
        if (ai) ai.enabled = false;

        // play death anim
        if (anim) anim.SetTrigger("Die");

        // make the corpse non-interfering
        foreach (var c in colliders) c.enabled = false;

        // optional cleanup
        Destroy(gameObject, cleanupDelay);
    }
}
