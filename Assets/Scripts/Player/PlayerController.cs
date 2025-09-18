using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;
    [Tooltip("How quickly we interpolate speed changes for animations.")]
    [SerializeField] private float speedLerp = 10f;

    [Header("Grounding")]
    [SerializeField] private Transform groundCheck;    // optional; if null, uses CharacterController.isGrounded
    [SerializeField] private float groundRadius = 0.2f;
    [SerializeField] private LayerMask groundMask = ~0;

    [Header("Animation (optional)")]
    [SerializeField] private Animator animator;        // assign if you have one
    [SerializeField] private string animSpeedParam = "Speed";
    [SerializeField] private string animGroundedParam = "IsGrounded";
    [SerializeField] private string animJumpTrigger = "Jump";

    private CharacterController controller;
    private float velocityY;
    private float currentSpeedForAnim;

    // Speed boost support
    private float baseMoveSpeed;
    private Coroutine boostRoutine;
    private float activeBoostMultiplier = 1f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        baseMoveSpeed = moveSpeed;
    }

    private void Update()
    {
        HandleMovement();
        HandleJumpAndGravity();
        UpdateAnimator();
    }

    private void HandleMovement()
    {
        // Input (legacy Input Manager). Swap to the new Input System if you prefer.
        float h = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        float v = Input.GetAxisRaw("Vertical");   // W/S or Up/Down

        Vector2 input = new Vector2(h, v);
        if (input.sqrMagnitude > 1f) input.Normalize();

        // IMPORTANT: Move relative to the PLAYER'S facing.
        // Your CameraRotation.cs already rotates this transform’s yaw with the mouse.
        Vector3 moveDir = transform.forward * input.y + transform.right * input.x;

        float finalSpeed = baseMoveSpeed * activeBoostMultiplier;
        Vector3 velocity = moveDir * finalSpeed;

        // Vertical component handled in HandleJumpAndGravity
        velocity.y = velocityY;

        controller.Move(velocity * Time.deltaTime);

        // For animation smoothing (0..1 based on input intensity)
        float targetAnimSpeed = Mathf.Clamp01(moveDir.magnitude);
        currentSpeedForAnim = Mathf.Lerp(currentSpeedForAnim, targetAnimSpeed, Time.deltaTime * speedLerp);
    }

    private void HandleJumpAndGravity()
    {
        bool grounded = IsGrounded();

        if (grounded && velocityY < 0f)
        {
            // small downward force to keep grounded
            velocityY = -2f;
        }

        if (grounded && Input.GetButtonDown("Jump"))
        {
            velocityY = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if (animator && !string.IsNullOrEmpty(animJumpTrigger)) animator.SetTrigger(animJumpTrigger);
        }

        velocityY += gravity * Time.deltaTime;
    }

    private bool IsGrounded()
    {
        if (groundCheck != null)
        {
            return Physics.CheckSphere(groundCheck.position, groundRadius, groundMask, QueryTriggerInteraction.Ignore);
        }
        return controller.isGrounded;
    }

    private void UpdateAnimator()
    {
        if (!animator) return;

        if (!string.IsNullOrEmpty(animSpeedParam))
            animator.SetFloat(animSpeedParam, currentSpeedForAnim);

        if (!string.IsNullOrEmpty(animGroundedParam))
            animator.SetBool(animGroundedParam, IsGrounded());
    }

    /// <summary>
    /// Called by SpeedBoostCollectible. Multiplies move speed for a duration.
    /// Safe to call repeatedly; takes the highest multiplier and refreshes the timer.
    /// </summary>
    public void ApplySpeedBoost(float multiplier, float duration)
    {
        activeBoostMultiplier = Mathf.Max(activeBoostMultiplier, multiplier);
        if (boostRoutine != null) StopCoroutine(boostRoutine);
        boostRoutine = StartCoroutine(SpeedBoostTimer(duration));
    }

    private IEnumerator SpeedBoostTimer(float duration)
    {
        float t = duration;
        while (t > 0f)
        {
            t -= Time.deltaTime;
            yield return null;
        }
        activeBoostMultiplier = 1f;
        boostRoutine = null;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }
    }
#endif
}
