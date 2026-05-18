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
    private string initialSlideFeedback = "raté";
    private bool hasPassedUnderObstacle = false;
    private int slideStartBeatCount = -1;

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

    /// Initialisation des liaisons de composants physiques et abonnement aux pulsations temporelles.
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<BoxCollider2D>();
        playerSR = GetComponent<SpriteRenderer>();
        originalColliderSize = playerCollider.size;

        BeatManager.OnBeat1 += EvaluateSlideBoost;
    }

    /// Résiliation des liaisons d'événements à la désactivation du personnage.
    void OnDestroy()
    {
        BeatManager.OnBeat1 -= EvaluateSlideBoost;
    }

    // --- PHYSIQUE ET MOUVEMENT ---

    /// Traitement analytique des rafraîchissements physiques, détections d'obstacles et mémoires d'inputs.
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
            if (!isSliding)
            {
                string captured = "raté";
                System.Action<string> callback = (f) => captured = f;
                BeatManager.OnInputFeedback += callback;

                if (BeatManager.Instance != null)
                {
                    BeatManager.Instance.IsActionOnBeat(false);
                    slideStartBeatCount = BeatManager.Instance.beatCounter;
                }

                BeatManager.OnInputFeedback -= callback;
                initialSlideFeedback = captured;
                hasPassedUnderObstacle = false;
                StartSlide();
            }
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

    /// Mise à jour temporelle et diminution linéaire des fenêtres d'amorti des inputs.
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

    /// Actualisation des bascules de variables logiques de l'arbre d'animation.
    private void UpdateAnimations()
    {
        if (anim == null) return;
        anim.SetBool("isDamaged", isKnockedBack);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
        anim.SetBool("isSliding", isSliding);
        anim.SetBool("isDashing", isDashing);
    }

    /// Routine d'application et de libération de l'état de recul cinétique après impact.
    private IEnumerator KnockbackRoutine(Vector2 force)
    {
        isKnockedBack = true;
        isDashing = false;
        rb.linearVelocity = ClampVelocity(force);
        yield return new WaitForSeconds(knockbackDuration);
        isKnockedBack = false;
    }

    /// Processus de déplacement et d'annulation de la gravité inhérent à la ruée.
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

    /// Dessine la sphère filaire matérialisant la portée de détection du plafond.
    private void OnDrawGizmosSelected()
    {
        if (ceilingCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(ceilingCheck.position, ceilingCheckRadius);
        }
    }

    /// Déclencheur externe appliquant un vecteur d'impact de recul sur le Rigidbody.
    public void ApplyKnockback(Vector2 force) => StartCoroutine(KnockbackRoutine(force));

    /// Assigne la temporisation de la période d'invulnérabilité après un choc.
    public void NotifyEnemyCollision() => gracePeriodCounter = collisionGraceDuration;

    /// Indique si la fenêtre de protection temporelle contre les ennemis est active.
    public bool IsInCollisionGracePeriod() => gracePeriodCounter > 0;

    /// Applique l'impulsion verticale du saut initial.
    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        coyoteTimeCounter = 0;
        isJumping = true;
        jumpTimeCounter = jumpTime;
    }

    /// Réduit de moitié la dimension verticale de la boîte de collision pour glisser.
    private void StartSlide() { isSliding = true; playerCollider.size = new Vector2(originalColliderSize.x, originalColliderSize.y * 0.5f); }

    /// Désactive l'état de glissade et applique la pénalité de boost si aucun obstacle n'a été croisé.
    private void StopSlide()
    {
        isSliding = false;
        playerCollider.size = originalColliderSize;

        if (!hasPassedUnderObstacle && BoostManager.Instance != null)
        {
            float max = BoostManager.Instance.maxBoost;
            BoostManager.Instance.RemoveBoost(max * 0.05f);
            BeatManager.OnInputFeedback?.Invoke("raté");
        }
    }

    /// Interpolation de la vitesse linéaire de déplacement selon le taux de glissade.
    private Vector2 GetBaseRunVelocity() => new Vector2(moveSpeed * Mathf.Lerp(1f, slideSpeedMultiplier, currentSlideLerp), rb.linearVelocity.y);

    /// Tronque le vecteur de vélocité pour ne pas outrepasser les limites structurelles.
    private Vector2 ClampVelocity(Vector2 velocity) => new Vector2(Mathf.Clamp(velocity.x, -maxHorizontalVelocity, maxHorizontalVelocity), Mathf.Clamp(velocity.y, -maxVerticalVelocityDown, maxVerticalVelocityUp));

    // --- RYTHME ET INPUTS ---

    /// Évalue l'environnement à chaque battement et attribue un boost hérité de la précision initiale ou une pénalité.
    private void EvaluateSlideBoost()
    {
        if (isSliding)
        {
            if (BeatManager.Instance != null && BeatManager.Instance.beatCounter == slideStartBeatCount)
            {
                return;
            }

            bool obstacleAbove = Physics2D.OverlapCircle(ceilingCheck.position, ceilingCheckRadius, groundLayer);
            if (BoostManager.Instance != null)
            {
                float max = BoostManager.Instance.maxBoost;
                if (obstacleAbove)
                {
                    hasPassedUnderObstacle = true;
                    if (initialSlideFeedback == "parfait")
                    {
                        BoostManager.Instance.AddBoost(max * 0.12f);
                        BeatManager.OnInputFeedback?.Invoke("parfait");
                    }
                    else if (initialSlideFeedback == "bien")
                    {
                        BoostManager.Instance.AddBoost(max * 0.08f);
                        BeatManager.OnInputFeedback?.Invoke("bien");
                    }
                    else if (initialSlideFeedback == "juste")
                    {
                        BoostManager.Instance.AddBoost(max * 0.05f);
                        BeatManager.OnInputFeedback?.Invoke("juste");
                    }
                    else
                    {
                        BoostManager.Instance.RemoveBoost(max * 0.05f);
                        BeatManager.OnInputFeedback?.Invoke("raté");
                    }
                }
                else
                {
                    BoostManager.Instance.RemoveBoost(max * 0.05f);
                    BeatManager.OnInputFeedback?.Invoke("raté");
                }
            }
        }
    }

    private bool GetJumpInput() => Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space);
    private bool GetJumpHoldInput() => Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.Space);
    private bool GetJumpReleaseInput() => Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.Z) || Input.GetKeyUp(KeyCode.Space);
    private bool GetSlideInput() => Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.S);
    private bool GetSlideInputDown() => Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.S);
    private bool GetDashInput() => Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.LeftShift);

    /// Soumet l'évaluation temporelle de l'action pour appliquer la récompense de rythme standard.
    private void HandleRhythmicAction()
    {
        if (BeatManager.Instance != null)
        {
            BeatManager.Instance.IsActionOnBeat(true);
        }
    }
}