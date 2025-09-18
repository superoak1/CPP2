using UnityEngine;

public class SimpleBoo : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform player;          // auto-set if empty (by tag "Player")
    [SerializeField] Transform playerCamera;    // auto-set if empty (Camera.main)

    [Header("Behavior")]
    [SerializeField, Tooltip("Degrees within which we consider the camera 'looking at' the Boo.")]
    float lookConeAngle = 55f;
    [SerializeField] float moveSpeed = 3.0f;
    [SerializeField] float stopDistance = 1.5f;
    [SerializeField] LayerMask losMask = ~0;    // line-of-sight: include your level geometry layers

    [Header("Shooting (optional)")]
    [SerializeField] GameObject fireballPrefab;
    [SerializeField] Transform shootPoint;
    [SerializeField] float fireCooldown = 1.25f;
    [SerializeField] float fireballSpeed = 16f;
    [SerializeField] int fireballDamage = 10;
    [SerializeField] float shootRange = 20f;

    Renderer[] rends;
    float lastShot = -999f;

    void Awake()
    {
        rends = GetComponentsInChildren<Renderer>();
    }

    void Start()
    {
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
        if (!playerCamera && Camera.main) playerCamera = Camera.main.transform;
        SetVisible(true);
    }

    void Update()
    {
        if (!player || !playerCamera) return;

        // Always face the player (yaw-only)
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(toPlayer);

        // Are we being looked at?
        bool blocked;
        bool seen = IsPlayerLookingAtMe(out blocked) && !blocked;

        // Visible when seen; invisible when not
        SetVisible(seen);

        if (!seen)
        {
            // Move straight forward toward player until near
            if (toPlayer.magnitude > stopDistance)
                transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }
        else
        {
            // Optionally shoot when seen and within range
            TryShoot();
        }
    }

    bool IsPlayerLookingAtMe(out bool blockedByLOS)
    {
        blockedByLOS = false;
        Vector3 camToBoo = transform.position - playerCamera.position;

        // Flatten so vertical pitch doesn't make the cone too strict
        Vector3 camF = playerCamera.forward; camF.y = 0f; camF.Normalize();
        Vector3 flatDir = camToBoo; flatDir.y = 0f;

        if (flatDir.sqrMagnitude < 0.0001f) return true;

        float angle = Vector3.Angle(camF, flatDir);
        if (Physics.Raycast(playerCamera.position, camToBoo.normalized, out RaycastHit hit, camToBoo.magnitude + 0.1f, losMask, QueryTriggerInteraction.Ignore))
        {
            // If something that's not this Boo is in between, consider not seen
            if (!hit.transform.IsChildOf(transform)) blockedByLOS = true;
        }
        return angle <= lookConeAngle;
    }

    void TryShoot()
    {
        if (!fireballPrefab || !shootPoint) return;

        if (Time.time - lastShot < fireCooldown) return;
        if (Vector3.Distance(transform.position, player.position) > shootRange) return;

        // Orient toward player head-ish and fire
        Vector3 dir = (player.position + Vector3.up * 1.4f - shootPoint.position).normalized;
        var go = Instantiate(fireballPrefab, shootPoint.position, Quaternion.LookRotation(dir));
        var fb = go.GetComponent<SimpleFireball>();
        if (fb) fb.Launch(dir * fireballSpeed, fireballDamage);

        lastShot = Time.time;
    }

    void SetVisible(bool v)
    {
        // toggle only renderers; colliders/logic keep running
        for (int i = 0; i < rends.Length; i++)
            if (rends[i]) rends[i].enabled = v;
    }
}
