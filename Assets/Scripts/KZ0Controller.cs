using UnityEngine;

public class KZ0Controller : MonoBehaviour
{
    [Header("Mouvement & Auto-Run")]
    public float moveSpeed = 10f;

    [Header("Parkour & Saut")]
    public float jumpForce = 15f;
    private bool isGrounded;
    public Transform groundCheck;
    public LayerMask groundLayer;

    [Header("Glissade")]
    public float slideSpeedMultiplier = 1.2f;
    private bool isSliding = false;
    private BoxCollider2D playerCollider;
    private Vector2 originalColliderSize;

    [Header("Dash Multidirectionnel")]
    public float dashForce = 20f;
    private bool canDash = true;
    private bool isDashing = false;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<BoxCollider2D>();
        originalColliderSize = playerCollider.size;
    }

    void Update()
    {
        // Détection du sol
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        // Gestion du Saut
        if (Input.GetButtonDown("Jump") && isGrounded && !isSliding)
        {
            Jump();
        }

        // Gestion de la Glissade
        if (Input.GetKeyDown(KeyCode.S) && isGrounded)
        {
            StartSlide();
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            StopSlide();
        }

        // Dash Multidirectionnel
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            Vector2 dashDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
            if (dashDir == Vector2.zero) dashDir = Vector2.right; // Dash par défaut vers l'avant
            StartCoroutine(PerformDash(dashDir));
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            if (BeatManager.Instance.IsActionOnBeat())
            {
                BoostManager.Instance.AddBoost(); // Récompense le tempo parfait
                Jump();
            }
            else
            {
                Jump(); // Il saute quand même mais ne gagne rien
            }
        }

    }

    void FixedUpdate()
    {
        if (isDashing) return;

        // Auto-Run : On force la vélocité X
        float currentXSpeed = isSliding ? moveSpeed * slideSpeedMultiplier : moveSpeed;

        // Garder la vélocité Y actuelle pour ne pas perturber la gravité ou le saut
        rb.linearVelocity = new Vector2(currentXSpeed, rb.linearVelocity.y);
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    void StartSlide()
    {
        isSliding = true;
        // Réduction de la taille du collider pour passer sous les débris
        playerCollider.size = new Vector2(originalColliderSize.x, originalColliderSize.y * 0.5f);
    }

    void StopSlide()
    {
        isSliding = false;
        playerCollider.size = originalColliderSize;
    }

    System.Collections.IEnumerator PerformDash(Vector2 dir)
    {
        canDash = false;
        isDashing = true;
        rb.gravityScale = 0; // On ignore la gravité pendant le dash pour la précision
        rb.linearVelocity = dir * dashForce;

        yield return new WaitForSeconds(0.2f); // Durée du dash

        rb.gravityScale = 1f; // On remet une gravité forte pour le feeling
        isDashing = false;

        // Note : Le dash sera rechargé par les actions rythmiques plus tard
        yield return new WaitForSeconds(0.5f);
        canDash = true;
    }
}
