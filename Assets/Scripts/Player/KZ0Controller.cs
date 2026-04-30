using System.Collections;
using UnityEngine;

public class KZ0Controller : MonoBehaviour
{
    // --- COMPOSANTS ---
    private Rigidbody2D rb;
    public Animator anim;
    private BoxCollider2D playerCollider;

    // --- MOUVEMENT ---
    [Header("Mouvement & Auto-Run")]
    public float moveSpeed = 10f;

    // --- SAUT & COYOTE TIME ---
    [Header("Parkour & Saut")]
    public float jumpForce = 15f;
    public float coyoteTimeDuration = 0.1f;
    private float coyoteTimeCounter;
    private bool isGrounded;
    public Transform groundCheck;
    public LayerMask groundLayer;

    // --- VÉLOCITÉ ---
    [Header("Velocité Maximale")]
    public float maxHorizontalVelocity = 30f;
    public float maxVerticalVelocityUp = 20f;
    public float maxVerticalVelocityDown = 25f;
    public float minVerticalVelocityUp = 5f;

    // --- GRAVITÉ ---
    [Header("Gravité en Chute")]
    public float fallGravityMultiplier = 3f;

    // --- GLISSADE ---
    [Header("Glissade (Smooth)")]
    public float slideSpeedMultiplier = 1.2f;
    public float slideEaseDuration = 0.2f;
    private bool isSliding = false;
    private float currentSlideLerp = 0f;
    private Vector2 originalColliderSize;

    // --- DASH ---
    [Header("Dash (Forces Séparées)")]
    public float dashForceX = 20f;
    public float dashForceY = 12f;
    public float dashDuration = 0.3f;
    public AnimationCurve dashEase = AnimationCurve.Linear(0, 1, 1, 1);
    public float dashCooldown = 0.75f;
    private bool canDash = true;
    private bool isDashing = false;
    private bool hasTouchedGroundSinceDash = true;
    private Vector2 currentDashVector = Vector2.zero;

    // --- COMBAT & GRACE PERIOD ---
    [Header("Combat Coyote (Grace Period)")]
    public float collisionGraceDuration = 0.1f;
    private float gracePeriodCounter;

    [Header("Effets d'Impact (Knockback)")]
    public float knockbackDuration = 0.2f;
    private bool isKnockedBack = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<BoxCollider2D>();
        originalColliderSize = playerCollider.size;
    }

    void Update()
    {
        if (isKnockedBack)
        {
            return;
        }

        // GESTION DU GROUNDED
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTimeDuration;
            hasTouchedGroundSinceDash = true;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (gracePeriodCounter > 0)
        {
            gracePeriodCounter -= Time.deltaTime;
        }

        // SAUT
        if (GetJumpInput() && coyoteTimeCounter > 0f && !isSliding)
        {
            HandleRhythmicAction();
            Jump();
        }

        if (isDashing)
        {
            return;
        }

        // GRAVITÉ DYNAMIQUE
        if (rb.linearVelocity.y < 0) rb.gravityScale = fallGravityMultiplier;
        else rb.gravityScale = 1.15f;

        // DÉPLACEMENT
        Vector2 baseVel = GetBaseRunVelocity();
        rb.linearVelocity = ClampVelocity(baseVel);

        // GLISSADE
        if (GetSlideInput() && isGrounded)
        {
            if (!isSliding) StartSlide();
            currentSlideLerp = Mathf.MoveTowards(currentSlideLerp, 1f, Time.deltaTime / slideEaseDuration);
        }
        else
        {
            if (isSliding) StopSlide();
            currentSlideLerp = Mathf.MoveTowards(currentSlideLerp, 0f, Time.deltaTime / slideEaseDuration);
        }

        // DASH
        if (GetDashInput() && canDash && hasTouchedGroundSinceDash)
        {
            HandleRhythmicAction();
            Vector2 dashDir = new Vector2(Mathf.Max(0f, Input.GetAxisRaw("Horizontal")), Mathf.Max(0f, Input.GetAxisRaw("Vertical"))).normalized;
            if (dashDir == Vector2.zero) dashDir = Vector2.right;
            StartCoroutine(PerformDash(dashDir));
        }

        UpdateAnimations();
    }

    // --- SYSTÈME D'ANIMATION ---
    private void UpdateAnimations()
    {
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
        anim.SetBool("isSliding", isSliding);
        anim.SetBool("isAttacking", isDashing);
        anim.SetBool("isDamaged", isKnockedBack);
        print($"Animation States - Grounded: {isGrounded}, Y Velocity: {rb.linearVelocity.y}, Sliding: {isSliding}, Dashing: {isDashing}, Damaged: {isKnockedBack}");
    }

    void FixedUpdate()
    {
        if (isDashing || isKnockedBack) return;

        float bpmFactor = BeatManager.Instance.currentBPM / BeatManager.Instance.originalMusicBPM;
        float dynamicSpeed = moveSpeed * bpmFactor;
        float targetMultiplier = Mathf.Lerp(1f, slideSpeedMultiplier, currentSlideLerp);

        rb.linearVelocity = new Vector2(dynamicSpeed * targetMultiplier, rb.linearVelocity.y);
    }

    // --- UTILITAIRES & ACTIONS ---
    public void NotifyEnemyCollision() => gracePeriodCounter = collisionGraceDuration;
    public bool IsInCollisionGracePeriod() => gracePeriodCounter > 0;

    private Vector2 ClampVelocity(Vector2 velocity)
    {
        velocity.x = Mathf.Clamp(velocity.x, -maxHorizontalVelocity, maxHorizontalVelocity);
        velocity.y = Mathf.Clamp(velocity.y, -maxVerticalVelocityDown, maxVerticalVelocityUp);
        return velocity;
    }

    private Vector2 GetBaseRunVelocity()
    {
        float targetMultiplier = Mathf.Lerp(1f, slideSpeedMultiplier, currentSlideLerp);
        return new Vector2(moveSpeed * targetMultiplier, rb.linearVelocity.y);
    }

    public void ApplyKnockback(Vector2 force)
    {
        if (!isKnockedBack) StartCoroutine(KnockbackRoutine(force));
    }

    private IEnumerator KnockbackRoutine(Vector2 force)
    {
        isKnockedBack = true;
        rb.linearVelocity = ClampVelocity(force);
        yield return new WaitForSeconds(knockbackDuration);
        isKnockedBack = false;
    }

    void Jump()
    {
        rb.linearVelocity = ClampVelocity(new Vector2(rb.linearVelocity.x, jumpForce));
        coyoteTimeCounter = 0;
    }

    void StartSlide()
    {
        isSliding = true;
        playerCollider.size = new Vector2(originalColliderSize.x, originalColliderSize.y * 0.5f);
    }

    void StopSlide()
    {
        isSliding = false;
        playerCollider.size = originalColliderSize;
    }

    IEnumerator PerformDash(Vector2 dir)
    {
        rb.gravityScale = 0f;
        canDash = false;
        isDashing = true;
        hasTouchedGroundSinceDash = false;

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            float curveValue = dashEase.Evaluate(elapsed / dashDuration);
            Vector2 targetVelocity = (GetBaseRunVelocity() + (dir * new Vector2(dashForceX, dashForceY))) * curveValue;
            rb.linearVelocity = targetVelocity;
            elapsed += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    // ENTRÉES
    private bool GetJumpInput() => Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow);
    private bool GetSlideInput() => Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.S);
    private bool GetDashInput() => Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Mouse1);

    private void HandleRhythmicAction()
    {
        if (BeatManager.Instance != null && BeatManager.Instance.IsActionOnBeat())
            BoostManager.Instance.AddBoost();
    }
}