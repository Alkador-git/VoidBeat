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

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<BoxCollider2D>();
        originalColliderSize = playerCollider.size;
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        // --- ENTRÉES ---
        if (GetJumpInput() && isGrounded && !isSliding)
        {
            HandleRhythmicAction();
            Jump();
        }

        // Glissade
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

        // Dash avec filtrage directionnel
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

    private Vector2 GetBaseRunVelocity()
    {
        float targetMultiplier = Mathf.Lerp(1f, slideSpeedMultiplier, currentSlideLerp);
        return new Vector2(moveSpeed * targetMultiplier, rb.linearVelocity.y);
    }

    void FixedUpdate()
    {
        Vector2 baseVel = GetBaseRunVelocity();

        if (isDashing)
        {
            // Addition de la force de dash calculée à la vitesse de base
            rb.linearVelocity = new Vector2(baseVel.x + currentDashVector.x, baseVel.y + currentDashVector.y);
        }
        else
        {
            rb.linearVelocity = baseVel;
        }
    }

    // --- LOGIQUE DE MOUVEMENT ---

    void Jump() => rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

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
        canDash = false;
        isDashing = true;
        rb.gravityScale = 0;

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            float curveValue = dashEase.Evaluate(elapsed / dashDuration);

            // On applique séparément les forces sur X et Y
            float fX = dir.x * dashForceX * curveValue;
            float fY = dir.y * dashForceY * curveValue;

            currentDashVector = new Vector2(fX, fY);

            elapsed += Time.deltaTime;
            yield return null;
        }

        currentDashVector = Vector2.zero;
        rb.gravityScale = 1.35f; // Gravité un peu plus lourde
        isDashing = false;

        yield return new WaitForSeconds(0.75f);
        canDash = true;
    }

    // --- HELPERS INPUTS ---
    private bool GetJumpInput() => Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow);
    private bool GetSlideInput() => Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);
    private bool GetDashInput() => Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift) || Input.GetMouseButtonDown(1);

    private void HandleRhythmicAction()
    {
        if (BeatManager.Instance != null && BeatManager.Instance.IsActionOnBeat())
            BoostManager.Instance.AddBoost();
    }
}