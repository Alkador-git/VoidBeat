using System.Collections;
using UnityEngine;

public class KZ0Controller : MonoBehaviour
{
    private Rigidbody2D rb;
    public Animator anim;
    private BoxCollider2D playerCollider;
    private SpriteRenderer playerSR;

    [Header("Mouvement & Auto-Run")]
    public float moveSpeed = 3.5f;

    [Header("Parkour & Saut")]
    public float jumpForce = 15f;
    public float coyoteTimeDuration = 0.1f;
    private float coyoteTimeCounter;
    private bool isGrounded;
    public Transform groundCheck;
    public LayerMask groundLayer;

    [Header("Saut Variable (Hold)")]
    public float jumpTime = 0.15f;
    private float jumpTimeCounter;
    private bool isJumping;

    [Header("Input Buffering")]
    public float bufferTime = 0.2f;
    private float jumpBufferCounter;
    private float dashBufferCounter;
    private float slideBufferCounter;

    [Header("Velocité Maximale")]
    public float maxHorizontalVelocity = 30f;
    public float maxVerticalVelocityUp = 20f;
    public float maxVerticalVelocityDown = 25f;

    [Header("Gravité en Chute")]
    public float fallGravityMultiplier = 3f;

    [Header("Glissade (Smooth)")]
    public float slideSpeedMultiplier = 1.2f;
    public float slideEaseDuration = 0.2f;
    public Transform ceilingCheck;
    public float ceilingCheckRadius = 0.2f;
    private bool isSliding = false;
    private float currentSlideLerp = 0f;
    private Vector2 originalColliderSize;

    [Header("Dash")]
    public float dashForceX = 20f;
    public float dashForceY = 12f;
    public float dashDuration = 0.3f;
    public AnimationCurve dashEase = AnimationCurve.Linear(0, 1, 1, 1);
    public float dashCooldown = 0.75f;
    private bool canDash = true;
    private bool isDashing = false;
    private bool hasTouchedGroundSinceDash = true;

    [Header("Combat")]
    public float collisionGraceDuration = 0.1f;
    private float gracePeriodCounter;

    [Header("Effets d'Impact")]
    public float knockbackDuration = 0.2f;
    private bool isKnockedBack = false;

    // --- INITIALISATION ---

    /// Initialisation des composants au lancement et enregistrement de l'événement rythmique.
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<BoxCollider2D>();
        playerSR = GetComponent<SpriteRenderer>();
        originalColliderSize = playerCollider.size;

        BeatManager.OnBeat1 += EvaluateSlideBoost;
    }

    /// Désabonnement des événements rythmiques à la destruction de l'objet.
    void OnDestroy()
    {
        BeatManager.OnBeat1 -= EvaluateSlideBoost;
    }

    // --- PHYSIQUE ET MOUVEMENT ---

    /// Gestion de la boucle de rafraîchissement des mouvements et inputs.
    void Update()
    {
        UpdateAnimations();
        if (isKnockedBack) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTimeDuration;
            hasTouchedGroundSinceDash = true;
            isJumping = false;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (gracePeriodCounter > 0) gracePeriodCounter -= Time.deltaTime;

        UpdateBuffers();

        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && !isSliding)
        {
            jumpBufferCounter = 0f;
            HandleRhythmicAction();
            Jump();
        }

        if (GetJumpHoldInput() && isJumping)
        {
            if (jumpTimeCounter > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * 1.2f);
                jumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                isJumping = false;
            }
        }

        if (GetJumpReleaseInput())
        {
            isJumping = false;
        }

        if (isDashing) return;

        rb.gravityScale = (rb.linearVelocity.y < 0) ? fallGravityMultiplier : 1.15f;
        Vector2 baseVel = GetBaseRunVelocity();
        rb.linearVelocity = ClampVelocity(baseVel);

        bool obstacleAbove = Physics2D.OverlapCircle(ceilingCheck.position, ceilingCheckRadius, groundLayer);

        if ((GetSlideInput() || slideBufferCounter > 0f || (isSliding && obstacleAbove)) && isGrounded)
        {
            if (!isSliding) StartSlide();
            slideBufferCounter = 0f;
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

        if (dashBufferCounter > 0f && canDash && hasTouchedGroundSinceDash)
        {
            dashBufferCounter = 0f;
            HandleRhythmicAction();

            Vector2 dashDir = new Vector2(Mathf.Max(0f, Input.GetAxisRaw("Horizontal")), Mathf.Max(0f, Input.GetAxisRaw("Vertical"))).normalized;
            if (dashDir == Vector2.zero) dashDir = Vector2.right;
            StartCoroutine(PerformDash(dashDir));
        }
    }

    /// Gère les durées des buffers d'input.
    private void UpdateBuffers()
    {
        if (GetJumpInput()) jumpBufferCounter = bufferTime;
        else jumpBufferCounter -= Time.deltaTime;

        if (GetDashInput()) dashBufferCounter = bufferTime;
        else dashBufferCounter -= Time.deltaTime;

        if (GetSlideInputDown()) slideBufferCounter = bufferTime;
        else slideBufferCounter -= Time.deltaTime;

        jumpBufferCounter = Mathf.Max(0, jumpBufferCounter);
        dashBufferCounter = Mathf.Max(0, dashBufferCounter);
        slideBufferCounter = Mathf.Max(0, slideBufferCounter);
    }

    /// Met à jour les variables de l'Animator.
    private void UpdateAnimations()
    {
        if (anim == null) return;
        anim.SetBool("isDamaged", isKnockedBack);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
        anim.SetBool("isSliding", isSliding);
        anim.SetBool("isDashing", isDashing);
    }

    /// Coroutine pour l'effet de recul après dégât.
    private IEnumerator KnockbackRoutine(Vector2 force)
    {
        isKnockedBack = true;
        isDashing = false;
        rb.linearVelocity = ClampVelocity(force);
        yield return new WaitForSeconds(knockbackDuration);
        isKnockedBack = false;
    }

    /// Coroutine gérant le dash et son timing.
    private IEnumerator PerformDash(Vector2 dir)
    {
        rb.gravityScale = 0f;
        canDash = false;
        isDashing = true;
        hasTouchedGroundSinceDash = false;

        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            if (isKnockedBack) break;
            float curveValue = dashEase.Evaluate(elapsed / dashDuration);
            rb.linearVelocity = (GetBaseRunVelocity() + (dir * new Vector2(dashForceX, dashForceY))) * curveValue;

            elapsed += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    /// Dessine le détecteur de plafond dans la scène Unity.
    private void OnDrawGizmosSelected()
    {
        if (ceilingCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(ceilingCheck.position, ceilingCheckRadius);
        }
    }

    /// Applique une force extérieure (recul).
    public void ApplyKnockback(Vector2 force) => StartCoroutine(KnockbackRoutine(force));

    /// Déclenche l'invulnérabilité temporaire.
    public void NotifyEnemyCollision() => gracePeriodCounter = collisionGraceDuration;

    /// Vérifie si le joueur est en période de grâce.
    public bool IsInCollisionGracePeriod() => gracePeriodCounter > 0;

    /// Applique l'impulsion verticale du saut initial.
    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        coyoteTimeCounter = 0;
        isJumping = true;
        jumpTimeCounter = jumpTime;
    }

    /// Active l'état de glissade (hitbox réduite).
    private void StartSlide() { isSliding = true; playerCollider.size = new Vector2(originalColliderSize.x, originalColliderSize.y * 0.5f); }

    /// Désactive l'état de glissade (hitbox normale).
    private void StopSlide() { isSliding = false; playerCollider.size = originalColliderSize; }

    /// Calcule la vitesse de course actuelle.
    private Vector2 GetBaseRunVelocity() => new Vector2(moveSpeed * Mathf.Lerp(1f, slideSpeedMultiplier, currentSlideLerp), rb.linearVelocity.y);

    /// Bride la vélocité selon les limites fixées.
    private Vector2 ClampVelocity(Vector2 velocity) => new Vector2(Mathf.Clamp(velocity.x, -maxHorizontalVelocity, maxHorizontalVelocity), Mathf.Clamp(velocity.y, -maxVerticalVelocityDown, maxVerticalVelocityUp));

    // --- RYTHME ET INPUTS ---

    /// Évalue et attribue ou retire du boost selon la présence d'obstacles au-dessus de la glissade lors d'un beat.
    private void EvaluateSlideBoost()
    {
        if (isSliding)
        {
            bool obstacleAbove = Physics2D.OverlapCircle(ceilingCheck.position, ceilingCheckRadius, groundLayer);
            if (BoostManager.Instance != null)
            {
                float amount = BoostManager.Instance.maxBoost * 0.05f;
                if (obstacleAbove)
                {
                    BoostManager.Instance.AddBoost(amount);
                }
                else
                {
                    BoostManager.Instance.RemoveBoost(amount);
                }
            }
        }
    }

    private bool GetJumpInput() => Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow);
    private bool GetJumpHoldInput() => Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow);
    private bool GetJumpReleaseInput() => Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.Z) || Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.UpArrow);
    private bool GetSlideInput() => Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.S);
    private bool GetSlideInputDown() => Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.S);
    private bool GetDashInput() => Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.LeftShift);

    /// Vérifie le rythme avec le BeatManager pour donner du boost standard.
    private void HandleRhythmicAction()
    {
        if (BeatManager.Instance != null && BeatManager.Instance.IsActionOnBeat()) BoostManager.Instance.AddBoost();
    }
}