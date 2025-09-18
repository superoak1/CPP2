using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GoblinAI : MonoBehaviour
{
    public enum State { Patrol, Chase, Attack }

    [Header("References")]
    [Tooltip("If left empty, will find by tag 'Player' at runtime.")]
    public Transform player;
    private PlayerHealth playerHealth;
    private NavMeshAgent agent;
    private Animator anim; // optional

    [Header("Patrol")]
    [Tooltip("Assign at least 2 points. The goblin will loop through them.")]
    public Transform[] patrolPoints;
    [Tooltip("How close is 'arrived' at a patrol point.")]
    public float patrolPointTolerance = 0.3f;
    private int patrolIndex = 0;

    [Header("Detection")]
    [Tooltip("Max distance to notice the player.")]
    public float detectionRadius = 12f;
    [Tooltip("Field of view in degrees (centered on forward). Set to 360 for omnidirectional.")]
    [Range(0f, 360f)] public float fieldOfView = 120f;
    [Tooltip("Layers to consider as obstacles for line of sight checks.")]
    public LayerMask obstacleMask = ~0;

    [Header("Chase")]
    [Tooltip("Distance at which the goblin will give up chase if player goes out of range for 'loseSightTime' seconds.")]
    public float maxChaseDistance = 18f;
    [Tooltip("How long can the goblin lose sight before returning to patrol.")]
    public float loseSightTime = 2f;
    private float loseSightTimer = 0f;

    [Header("Attack")]
    [Tooltip("Stop and attack when within this range.")]
    public float attackRange = 1.6f;
    [Tooltip("Seconds between attacks.")]
    public float attackCooldown = 1.2f;
    [Tooltip("Damage per hit to PlayerHealth.")]
    public int attackDamage = 10;
    private float lastAttackTime = -999f;

    [Header("Animation (optional)")]
    public string animMoveParam = "Speed";
    public string animAttackTrigger = "Attack";
    public string animHitTrigger = "Hit";
    public string animDieTrigger = "Die";

    private State state = State.Patrol;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        if (!player)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
        if (player) playerHealth = player.GetComponent<PlayerHealth>();
    }

    void Start()
    {
        if (patrolPoints != null && patrolPoints.Length > 0)
            GoToNextPatrolPoint();
    }

    void Update()
    {
        // Optional animator speed param
        if (anim && !string.IsNullOrEmpty(animMoveParam))
            anim.SetFloat(animMoveParam, agent.velocity.magnitude);

        switch (state)
        {
            case State.Patrol: TickPatrol(); break;
            case State.Chase: TickChase(); break;
            case State.Attack: TickAttack(); break;
        }
    }

    // --- PATROL ---
    void TickPatrol()
    {
        if (CanSeePlayer())
        {
            state = State.Chase;
            return;
        }

        if (patrolPoints == null || patrolPoints.Length == 0 || !agent.isOnNavMesh) return;

        if (!agent.pathPending && agent.remainingDistance <= patrolPointTolerance)
            GoToNextPatrolPoint();
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0 || !agent.isOnNavMesh) return;
        agent.stoppingDistance = 0f;
        agent.isStopped = false;
        agent.SetDestination(patrolPoints[patrolIndex].position);
        patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
    }

    // --- CHASE ---
    void TickChase()
    {
        if (!player || !agent.isOnNavMesh) { state = State.Patrol; return; }

        float dist = Vector3.Distance(transform.position, player.position);

        // Lost too far for too long? Return to patrol.
        bool seeing = CanSeePlayer();
        if (!seeing)
        {
            loseSightTimer += Time.deltaTime;
            if (loseSightTimer >= loseSightTime || dist > maxChaseDistance)
            {
                loseSightTimer = 0f;
                state = State.Patrol;
                GoToNextPatrolPoint();
                return;
            }
        }
        else
        {
            loseSightTimer = 0f;
        }

        // Move toward player
        agent.stoppingDistance = attackRange * 0.9f;
        if (agent.isStopped) agent.isStopped = false;
        agent.SetDestination(player.position);

        // Rotate to face the player smoothly
        Vector3 dir = (player.position - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }

        // Enter attack if close enough and line of sight
        if (dist <= attackRange && HasLineOfSight())
        {
            state = State.Attack;
            agent.isStopped = true;
        }
    }

    // --- ATTACK ---
    void TickAttack()
    {
        if (!player || !agent.isOnNavMesh) { state = State.Patrol; return; }

        float dist = Vector3.Distance(transform.position, player.position);

        // If player moved out of range, go back to chase
        if (dist > attackRange || !CanSeePlayer())
        {
            state = State.Chase;
            agent.isStopped = false;
            return;
        }

        // Face the player
        Vector3 dir = (player.position - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 12f);

        // Attack on cooldown
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;

            if (anim && !string.IsNullOrEmpty(animAttackTrigger))
                anim.SetTrigger(animAttackTrigger);

            // Simple immediate damage application (you can hook an animation event instead)
            if (playerHealth != null)
                playerHealth.TakeDamage(attackDamage);
        }
    }

    // --- SENSING ---
    bool CanSeePlayer()
    {
        if (!player) return false;

        // Distance check
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > detectionRadius) return false;

        // Angle check
        if (fieldOfView < 360f)
        {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToPlayer);
            if (angle > fieldOfView * 0.5f) return false;
        }

        // LOS check
        return HasLineOfSight();
    }

    bool HasLineOfSight()
    {
        if (!player) return false;

        Vector3 origin = transform.position + Vector3.up * 1.0f; // eye height
        Vector3 target = player.position + Vector3.up * 1.0f;

        if (Physics.Raycast(origin, (target - origin).normalized, out RaycastHit hit, detectionRadius, ~0, QueryTriggerInteraction.Ignore))
        {
            // True if we directly hit the player
            return hit.transform == player;
        }
        return false;
    }

    public void AnimEvent_DealDamage()
    {
        if (playerHealth == null) return;

        // extra LOS guard in case player moved
        if (Vector3.Distance(transform.position, player.position) <= attackRange && HasLineOfSight())
        {
            playerHealth.TakeDamage(attackDamage);
        }
    }


#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // FOV arcs (approximate)
        if (fieldOfView < 360f)
        {
            Vector3 left = Quaternion.Euler(0, -fieldOfView * 0.5f, 0) * transform.forward;
            Vector3 right = Quaternion.Euler(0, fieldOfView * 0.5f, 0) * transform.forward;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + left * detectionRadius);
            Gizmos.DrawLine(transform.position, transform.position + right * detectionRadius);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
#endif
}
