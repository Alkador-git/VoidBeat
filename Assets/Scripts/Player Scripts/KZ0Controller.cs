using UnityEngine;
using System.Collections;

public class KZ0Controller : MonoBehaviour
{
    [Header("Mouvement & Auto-Run")]
    public float moveSpeed = 10f;

    [Header("Parkour & Saut")]
    public float jumpForce = 15f;
    private bool isGrounded;
    public Transform groundCheck;
    public LayerMask groundLayer;

    [Header("Velocité Maximale")]
    public float maxHorizontalVelocity = 30f;
    public float maxVerticalVelocity = 50f;
    public float minVerticalVelocityUp = 10f;

    [Header("Glissade (Smooth)")]
    public float slideSpeedMultiplier = 1.2f;
    public float slideEaseDuration = 0.2f;
    private bool isSliding = false;
    private float currentSlideLerp = 0f;
    private BoxCollider2D playerCollider;
    private Vector2 originalColliderSize;

    [Header("Dash (Forces Séparées)")]
    public float dashForceX = 5f;
    public float dashForceY = 2.5f;
    public float dashDuration = 0.3f;
    public AnimationCurve dashEase = AnimationCurve.Linear(0, 1, 1, 1);
    private bool canDash = true;
    private bool isDashing = false;
    private Vector2 currentDashVector = Vector2.zero;

    [Header("Effets d'Impact (Knockback)")]
    public float knockbackDuration = 0.2f;
    private bool isKnockedBack = false;

    private Rigidbody2D rb;

    // Initialise les composants du joueur au démarrage
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<BoxCollider2D>();
        originalColliderSize = playerCollider.size;
    }

    // Gère les entrées et actions du joueur chaque frame
    void Update()
    {
        if (isKnockedBack) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        if (GetJumpInput() && isGrounded && !isSliding)
        {
            HandleRhythmicAction();
            Jump();
        }

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

        if (GetDashInput() && canDash)
        {
            HandleRhythmicAction();
            float inputX = Mathf.Max(0f, Input.GetAxisRaw("Horizontal"));
            float inputY = Mathf.Max(0f, Input.GetAxisRaw("Vertical"));

            Vector2 dashDir = new Vector2(inputX, inputY).normalized;
            if (dashDir == Vector2.zero) dashDir = Vector2.right;

            StartCoroutine(PerformDash(dashDir));
        }
    }

    // Limite la vélocité aux valeurs maximales définies
    private Vector2 ClampVelocity(Vector2 velocity)
    {
        velocity.x = Mathf.Clamp(velocity.x, -maxHorizontalVelocity, maxHorizontalVelocity);
        velocity.y = Mathf.Clamp(velocity.y, -maxVerticalVelocity, maxVerticalVelocity);
        return velocity;
    }

    // Limite la vélocité avec contrainte minimale verticale (pour dash/boost)
    private Vector2 ClampVelocityWithMinimum(Vector2 velocity)
    {
        velocity.x = Mathf.Clamp(velocity.x, -maxHorizontalVelocity, maxHorizontalVelocity);

        if (velocity.y > 0)
        {
            velocity.y = Mathf.Max(velocity.y, minVerticalVelocityUp);
        }

        velocity.y = Mathf.Clamp(velocity.y, -maxVerticalVelocity, maxVerticalVelocity);
        return velocity;
    }

    // Retourne la vélocité de base avec le modificateur de glissade appliqué
    private Vector2 GetBaseRunVelocity()
    {
        float targetMultiplier = Mathf.Lerp(1f, slideSpeedMultiplier, currentSlideLerp);
        return new Vector2(moveSpeed * targetMultiplier, rb.linearVelocity.y);
    }

    // Met à jour la vélocité du joueur dans la boucle physique
    void FixedUpdate()
    {
        if (isDashing || isKnockedBack) return;

        Vector2 baseVel = GetBaseRunVelocity();
        rb.linearVelocity = ClampVelocity(baseVel);
    }


    // Applique un knockback au joueur
    public void ApplyKnockback(Vector2 force)
    {
        if (!isKnockedBack)
        {
            StartCoroutine(KnockbackRoutine(force));
        }
    }

    // Coroutine qui gère la durée et l'effet du knockback
    private IEnumerator KnockbackRoutine(Vector2 force)
    {
        isKnockedBack = true;

        rb.linearVelocity = ClampVelocity(force);

        yield return new WaitForSeconds(knockbackDuration);

        isKnockedBack = false;
    }

    // Fait sauter le joueur en modifiant sa vélocité verticale
    void Jump() => rb.linearVelocity = ClampVelocity(new Vector2(rb.linearVelocity.x, jumpForce));

    // Commence une glissade et réduit la taille du collider
    void StartSlide()
    {
        isSliding = true;
        playerCollider.size = new Vector2(originalColliderSize.x, originalColliderSize.y * 0.5f);
    }

    // Arrête une glissade et restaure la taille du collider
    void StopSlide()
    {
        isSliding = false;
        playerCollider.size = originalColliderSize;
    }

    // Coroutine qui effectue le dash avec transition en courbe d'accélération
    IEnumerator PerformDash(Vector2 dir)
    {
        rb.gravityScale = 0f;
        canDash = false;
        isDashing = true;

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            float curveValue = dashEase.Evaluate(elapsed / dashDuration);
            float fX = dir.x * dashForceX * curveValue;
            float fY = dir.y * dashForceY * curveValue;

            currentDashVector = new Vector2(fX, fY);

            Vector2 targetVelocity = new Vector2(GetBaseRunVelocity().x + currentDashVector.x, GetBaseRunVelocity().y + currentDashVector.y);

            // Appliquer minVerticalVelocityUp seulement si le personnage monte ou le dash est dirigé vers le haut
            if (dir.y > 0)
            {
                rb.linearVelocity = ClampVelocityWithMinimum(targetVelocity);
            }
            else
            {
                rb.linearVelocity = ClampVelocity(targetVelocity);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        currentDashVector = Vector2.zero;
        isDashing = false;
        rb.gravityScale = 1f;

        yield return new WaitForSeconds(0.75f);
        canDash = true;
    }

    // Vérifie si le joueur appuie sur une touche de saut
    private bool GetJumpInput() => Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetAxis("Mouse ScrollWheel") > 0;

    // Vérifie si le joueur appuie sur une touche de glissade
    private bool GetSlideInput() => Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S) || Input.GetAxis("Mouse ScrollWheel") < 0;

    // Vérifie si le joueur appuie sur une touche de dash
    private bool GetDashInput() => Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.LeftShift);

    // Gère les actions qui se déclenchent au rythme de la musique
    private void HandleRhythmicAction()
    {
        if (BeatManager.Instance != null && BeatManager.Instance.IsActionOnBeat())
            BoostManager.Instance.AddBoost();
    }
}