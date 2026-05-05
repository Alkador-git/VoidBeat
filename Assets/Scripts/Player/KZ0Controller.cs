using System.Collections;
using UnityEngine;

public class KZ0Controller : MonoBehaviour
{
    // --- COMPOSANTS ---
    private Rigidbody2D rb;
    public Animator anim;
    private BoxCollider2D playerCollider;
    private SpriteRenderer playerSR;

    // --- MOUVEMENT ---
    [Header("Mouvement & Auto-Run")]
    public float moveSpeed = 3.5f;

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

    // --- GRAVITÉ ---
    [Header("Gravité en Chute")]
    public float fallGravityMultiplier = 3f;

    // --- GLISSADE ---
    [Header("Glissade (Smooth)")]
    public float slideSpeedMultiplier = 1.2f;
    public float slideEaseDuration = 0.2f;
    public Transform ceilingCheck;
    public float ceilingCheckRadius = 0.2f;
    private bool isSliding = false;
    private float currentSlideLerp = 0f;
    private Vector2 originalColliderSize;

    // --- DASH ---
    [Header("Dash")]
    public float dashForceX = 20f;
    public float dashForceY = 12f;
    public float dashDuration = 0.3f;
    public AnimationCurve dashEase = AnimationCurve.Linear(0, 1, 1, 1);
    public float dashCooldown = 0.75f;
    private bool canDash = true;
    private bool isDashing = false;
    private bool hasTouchedGroundSinceDash = true;

    [Header("Effets de Ghosting")]
    public GameObject ghostPrefab;
    public float ghostDelay = 0.05f;
    public Color ghostColor = new Color(0.12f, 0.73f, 1f, 0.5f);
    public float ghostFadeSpeed = 4f;

    // --- COMBAT ---
    [Header("Combat")]
    public float collisionGraceDuration = 0.1f;
    private float gracePeriodCounter;

    [Header("Effets d'Impact")]
    public float knockbackDuration = 0.2f;
    private bool isKnockedBack = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<BoxCollider2D>();
        playerSR = GetComponent<SpriteRenderer>();
        originalColliderSize = playerCollider.size;
    }

    void Update()
    {
        UpdateAnimations();
        if (isKnockedBack) return;
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

        if (gracePeriodCounter > 0) gracePeriodCounter -= Time.deltaTime;

        if (GetJumpInput() && coyoteTimeCounter > 0f && !isSliding)
        {
            HandleRhythmicAction();
            Jump();
        }

        if (isDashing) return;
        rb.gravityScale = (rb.linearVelocity.y < 0) ? fallGravityMultiplier : 1.15f;
        Vector2 baseVel = GetBaseRunVelocity();
        rb.linearVelocity = ClampVelocity(baseVel);

        // --- LOGIQUE DE GLISSADE ---
        bool obstacleAbove = Physics2D.OverlapCircle(ceilingCheck.position, ceilingCheckRadius, groundLayer);

        if ((GetSlideInput() || (isSliding && obstacleAbove)) && isGrounded)
        {
            if (!isSliding) StartSlide();
            currentSlideLerp = Mathf.MoveTowards(currentSlideLerp, 1f, Time.deltaTime / slideEaseDuration);
        }
        else
        {
            if (isSliding && !obstacleAbove)
            {
                StopSlide();
            }
            currentSlideLerp = Mathf.MoveTowards(currentSlideLerp, 0f, Time.deltaTime / slideEaseDuration);
        }

        if (GetDashInput() && canDash && hasTouchedGroundSinceDash)
        {
            HandleRhythmicAction();
            Vector2 dashDir = new Vector2(Mathf.Max(0f, Input.GetAxisRaw("Horizontal")), Mathf.Max(0f, Input.GetAxisRaw("Vertical"))).normalized;
            if (dashDir == Vector2.zero) dashDir = Vector2.right;
            StartCoroutine(PerformDash(dashDir));
        }
    }

    private void UpdateAnimations()
    {
        if (anim == null) return;
        anim.SetBool("isDamaged", isKnockedBack);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
        anim.SetBool("isSliding", isSliding);
        anim.SetBool("isDashing", isDashing);
    }

    // --- COROUTINES & ACTIONS ---

    private IEnumerator KnockbackRoutine(Vector2 force)
    {
        isKnockedBack = true;
        isDashing = false;
        rb.linearVelocity = ClampVelocity(force);
        yield return new WaitForSeconds(knockbackDuration);
        isKnockedBack = false;
    }

    private IEnumerator PerformDash(Vector2 dir)
    {
        rb.gravityScale = 0f;
        canDash = false;
        isDashing = true;
        hasTouchedGroundSinceDash = false;

        float elapsed = 0f;
        float ghostTimer = 0f;

        while (elapsed < dashDuration)
        {
            if (isKnockedBack) break;
            float curveValue = dashEase.Evaluate(elapsed / dashDuration);
            rb.linearVelocity = (GetBaseRunVelocity() + (dir * new Vector2(dashForceX, dashForceY))) * curveValue;

            ghostTimer += Time.deltaTime;
            if (ghostTimer >= ghostDelay)
            {
                SpawnGhost();
                ghostTimer = 0f;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void SpawnGhost()
    {
        if (ghostPrefab == null || playerSR == null) return;
        GameObject ghostObj = Instantiate(ghostPrefab);
        if (ghostObj.TryGetComponent<DashGhost>(out DashGhost ghostScript))
        {
            ghostScript.Init(playerSR.sprite, transform.position, transform.rotation, transform.localScale, ghostColor, ghostFadeSpeed);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (ceilingCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(ceilingCheck.position, ceilingCheckRadius);
        }
    }

    public void ApplyKnockback(Vector2 force) => StartCoroutine(KnockbackRoutine(force));
    public void NotifyEnemyCollision() => gracePeriodCounter = collisionGraceDuration;
    public bool IsInCollisionGracePeriod() => gracePeriodCounter > 0;
    private void Jump() { rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce); coyoteTimeCounter = 0; }
    private void StartSlide() { isSliding = true; playerCollider.size = new Vector2(originalColliderSize.x, originalColliderSize.y * 0.5f); }
    private void StopSlide() { isSliding = false; playerCollider.size = originalColliderSize; }
    private Vector2 GetBaseRunVelocity() => new Vector2(moveSpeed * Mathf.Lerp(1f, slideSpeedMultiplier, currentSlideLerp), rb.linearVelocity.y);
    private Vector2 ClampVelocity(Vector2 velocity) => new Vector2(Mathf.Clamp(velocity.x, -maxHorizontalVelocity, maxHorizontalVelocity), Mathf.Clamp(velocity.y, -maxVerticalVelocityDown, maxVerticalVelocityUp));
    private bool GetJumpInput() => Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow);
    private bool GetSlideInput() => Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.S);
    private bool GetDashInput() => Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.LeftShift);
    private void HandleRhythmicAction() { if (BeatManager.Instance != null && BeatManager.Instance.IsActionOnBeat()) BoostManager.Instance.AddBoost(); }
}